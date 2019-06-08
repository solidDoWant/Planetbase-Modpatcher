using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Patcher
{
    public class PatchBuilder
    {
        public const string Version = "2.2.0.0";

        /// <summary>
        /// Patch the assembly
        /// </summary>
        public static void Patch(string assemblyPath, string assemblyFirstPassPath, string unityEnginePath,
            string unityEngineUiPath, string frameworkPath, string patchedAssemblyPath)
        {
            //The custom resolver ensures that the right assemblies are loaded on recompilation
            using (var resolver = new PlanetbaseResolver(assemblyFirstPassPath, unityEnginePath, unityEngineUiPath))
            {
                resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetDirectoryName(assemblyPath)));

                using (var planetbaseModule =
                    ModuleDefinition.ReadModule(assemblyPath, new ReaderParameters {AssemblyResolver = resolver}))
                using (var frameworkAssembly = AssemblyDefinition.ReadAssembly(frameworkPath,
                    new ReaderParameters {AssemblyResolver = resolver}))
                {
                    AddFrameworkReference(frameworkAssembly, planetbaseModule);
                    AddModCalls(frameworkAssembly, planetbaseModule);

                    UpdateFields(planetbaseModule);
                    UpdateMethods(planetbaseModule);

                    UpdateReferences(planetbaseModule);

                    //Save the module
                    planetbaseModule.Write(patchedAssemblyPath);
                }
            }
        }

        /// <summary>
        /// Adds a reference to the framework into the game
        /// </summary>
        private static void AddFrameworkReference(AssemblyDefinition frameworkAssembly, ModuleDefinition planetbaseModule)
        {
            frameworkAssembly.Name.Version = new Version(0, 0, 0, 0);

            planetbaseModule.AssemblyReferences.Add(frameworkAssembly.Name);
        }

        /// <summary>
        /// Adds calls to load and update mods
        /// </summary>
        private static void AddModCalls(AssemblyDefinition frameworkAssembly, ModuleDefinition planetbaseModule)
        {
            //Get the game manager constructor from the game
            var gameManagerType =
                planetbaseModule.Types.First(type => type.FullName.Equals("Planetbase.GameManager"));

            //Get the initialization method from the framework
            var frameworkModule = frameworkAssembly.MainModule;

            var modLoaderType =
                frameworkModule.Types.First(type => type.FullName.Equals("PlanetbaseFramework.ModLoader"));

            AddModInitializationCall(gameManagerType, modLoaderType, planetbaseModule);
            AddModUpdateCall(gameManagerType, modLoaderType, planetbaseModule);
        }

        /// <summary>
        /// Adds a call to the mod loader initialization method
        /// </summary>
        private static void AddModInitializationCall(TypeDefinition gameManagerType, TypeDefinition modLoaderType,
            ModuleDefinition planetbaseModule)
        {
            var gameMangerConstructor = gameManagerType.Methods.First(method =>
                method.FullName.Equals("System.Void Planetbase.GameManager::.ctor()"));

            
            var loadModMethodDefinition = modLoaderType.Methods.First(method => method.Name.Equals("LoadMods"));

            //Add a reference to the method (and it's encapsulating type)
            var planetbaseScopeLoadModMethodDefinition =
                planetbaseModule.ImportReference(loadModMethodDefinition);

            //Create the method call and insert the instruction
            var loadModInstruction = Instruction.Create(OpCodes.Call, planetbaseScopeLoadModMethodDefinition);

            gameMangerConstructor.Body.Instructions.Insert(gameMangerConstructor.Body.Instructions.Count - 1,
                loadModInstruction);
        }

        /// <summary>
        /// Adds a call to the mod loader updater method
        /// </summary>
        private static void AddModUpdateCall(TypeDefinition gameManagerType, TypeDefinition modLoaderType, ModuleDefinition planetbaseModule)
        {
            //Add call to mod loader update method
            var updateMethod = gameManagerType.Methods.First(method => method.Name.Equals("update"));

            var updateModMethodDefinition =
                modLoaderType.Methods.First(method => method.Name.Equals("UpdateMods"));

            var planetbaseScopeUpdateModMethodDefinition =
                planetbaseModule.ImportReference(updateModMethodDefinition);

            var updateModInstruction =
                Instruction.Create(OpCodes.Call, planetbaseScopeUpdateModMethodDefinition);

            updateMethod.Body.Instructions.Insert(updateMethod.Body.Instructions.Count - 1,
                updateModInstruction);
        }

        /// <summary>
        /// Update the properties of fields
        /// </summary>
        private static void UpdateFields(ModuleDefinition planetbaseModule)
        {
            //Get all game types
            var fieldTypes = planetbaseModule.Types
                .Where(type => type.Namespace.Equals("Planetbase") && type.HasFields && !type.IsEnum);

            SetFieldsToPublic(fieldTypes);
        }

        /// <summary>
        /// Set the private and protected fields to public
        /// </summary>
        private static void SetFieldsToPublic(IEnumerable<TypeDefinition> fieldTypes)
        {
            //Make all fields public
            var privateFields = fieldTypes
                .Where(
                    type => !type.Name.StartsWith("SoundDefinition") &&
                            !IsAssignableFromType(type, "UnityEngine.MonoBehaviour")
                ) //For some reason setting the fields on "SoundDefinition" to public crashes the application
                .SelectMany(type => type.Fields)
                .Where(field => !field.IsPublic);

            foreach (var field in privateFields)
            {
                field.IsPublic = true;
            }
        }

        /// <summary>
        /// Recursively checks to see if the given type is a child of the parent with the provided name
        /// </summary>
        private static bool IsAssignableFromType(TypeReference childType, string fullParentName)
        {
            while (true)
            {
                if (childType == null) return false;

                if (childType.FullName == fullParentName)
                    return true;

                childType = childType.Resolve().BaseType;
            }
        }

        /// <summary>
        /// Update the properties of the methods
        /// </summary>
        private static void UpdateMethods(ModuleDefinition planetbaseModule)
        {
            var planetbaseMethods = planetbaseModule.Types
                .Where(type => type.Namespace.Equals("Planetbase") && type.HasMethods) //Get all types
                .SelectMany(type => type.Methods)
                .Where(method => !method.IsConstructor)
                .ToArray(); //Get all methods, less constructors

            foreach (var method in planetbaseMethods.Where(methods => !methods.IsPublic))
            {
                method.IsPublic = true;
            }

            foreach (var method in planetbaseMethods.Where(method => !method.IsStatic && !method.IsVirtual))
            {
                method.IsVirtual = true;
            }
        }

        /// <summary>
        /// Update the references to use the correct mscorlib
        /// </summary>
        private static void UpdateReferences(ModuleDefinition planetbaseModule)
        {
            var mscorlib4Reference = planetbaseModule.AssemblyReferences.First(assembly =>
                assembly.Name.Equals("mscorlib") && assembly.Version.Equals(new Version(2, 0, 0, 0)));
            planetbaseModule.AssemblyReferences.Remove(mscorlib4Reference);
        }
    }
}