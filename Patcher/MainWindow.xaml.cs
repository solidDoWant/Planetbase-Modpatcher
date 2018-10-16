using System.Linq;
using System.Net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Web.Script.Serialization;
using System.IO.Compression;

namespace Patcher
{
    using System;
    using System.IO;
    using System.Windows;
    using Microsoft.Win32;

    //Completely rewritinig thisi is pretty high up on my todo list, but it's good enough for the initial release.

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string LocalPBAssemblyPath = "Assembly-CSharp.dll";
        public const string LocalFrameworkAssemblyPath = "PlanetbaseFramework.zip";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = buttonSelect.ActualWidth + 30 + labelDll.ActualWidth
                         + (string.IsNullOrEmpty(labelDll.Text) ? 5 : 10);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
                          {
                              Filter = "Assembly-CSharp.dll|Assembly-CSharp.dll",
                              Multiselect = false,
                              Title = "Choose Assembly-CSharp.dll location",
                              InitialDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Planetbase\Planetbase_Data\Managed"
            };
            if (!ofd.ShowDialog().GetValueOrDefault(false)) return;

            labelDll.Text = ofd.FileName;
            FrameworkElement_OnSizeChanged(this, null);
            buttonPatch.IsEnabled = true;
        }

        private async void ButtonBase2_OnClick(object sender, RoutedEventArgs e)
        {
            //Complain if invalid file selection
            if (string.IsNullOrEmpty(labelDll.Text))
            {
                return;
            }
            if (!File.Exists(labelDll.Text))
            {
                return;
            }

            buttonSelect.IsEnabled = false;
            buttonPatch.IsEnabled = false;

            //Copy/backup DLL
            buttonPatch.Content = "Copying working DLL...";

            await labelDll.Dispatcher.InvokeAsync(
                () =>
                    {
                        if (File.Exists("Assembly-CSharp.dll"))
                        {
                            File.Delete("Assembly-CSharp.dll");
                        }

                        File.Copy(labelDll.Text, LocalPBAssemblyPath);

                        if (File.Exists(LocalFrameworkAssemblyPath))
                        {
                            File.Delete(LocalFrameworkAssemblyPath);
                        }

                        if (File.Exists("PlanetbaseFramework.dll"))
                        {
                            File.Delete("PlanetbaseFramework.dll");
                        }

                        if (File.Exists("Assembly-CSharp2.dll"))
                        {
                            File.Delete("Assembly-CSharp2.dll");
                        }
                    });

            //Download framework from GitHub
            DownloadFramework();

            //Patch the DLL
            //Add reference to framework
            var planetbaseModule = ModuleDefinition.ReadModule(LocalPBAssemblyPath);
            var gameManagerType = planetbaseModule.Types.FirstOrDefault(type => type.FullName.Equals("Planetbase.GameManager"));
            
            var frameworkAssembly = AssemblyDefinition.ReadAssembly("PlanetbaseFramework.dll");
            frameworkAssembly.Name.Version = new Version(0, 0, 0, 0);
            var frameworkModule = frameworkAssembly.MainModule;

            planetbaseModule.AssemblyReferences.Add(frameworkAssembly.Name);

            //Add call to mod loader initialization method
            var gameMangerConstructor = gameManagerType.Methods.FirstOrDefault(method =>
                method.FullName.Equals("System.Void Planetbase.GameManager::.ctor()"));

            var modLoadType =
                frameworkModule.Types.FirstOrDefault(type => type.FullName.Equals("PlanetbaseFramework.Modloader"));
            var loadModMethodDefinition = modLoadType.Methods.FirstOrDefault(method => method.Name.Equals("LoadMods"));

            var planetbaseScopeLoadModMethodDefinition = planetbaseModule.ImportReference(loadModMethodDefinition);

            var loadModInstruction = Instruction.Create(OpCodes.Call, planetbaseScopeLoadModMethodDefinition);
            
            gameMangerConstructor.Body.Instructions.Insert(gameMangerConstructor.Body.Instructions.Count - 1, loadModInstruction);
            
            //Add call to mod loader update method
            var updateMethod = gameManagerType.Methods.FirstOrDefault(method => method.Name.Equals("update"));

            var updateModMethodDefinition =
                modLoadType.Methods.FirstOrDefault(method => method.Name.Equals("UpdateMods"));

            var planetbaseScopeUpdateModMethodDefinition = planetbaseModule.ImportReference(updateModMethodDefinition);

            var updateModInstruction = Instruction.Create(OpCodes.Call, planetbaseScopeUpdateModMethodDefinition);

            updateMethod.Body.Instructions.Insert(updateMethod.Body.Instructions.Count - 1, updateModInstruction);


            //Update the version number
            var definitionsType = planetbaseModule.Types.FirstOrDefault(type => type.Name.Equals("Definitions"));
            definitionsType.Fields.FirstOrDefault(field => field.Name.Equals("VersionNumber")).Constant += "[P 2.2]";

            //Make all fields public
            var fields = planetbaseModule.Types
                .Where(type => type.Namespace.Equals("Planetbase") && type.HasFields) //Get all types
                .SelectMany(type => type.Fields);    //Get all fields

            var privateFields = fields.Where(field => !field.IsPublic);

            foreach (var field in privateFields)
            {
                field.IsPublic = true;
            }

            foreach (var field in fields)
            {
                field.HasConstant = false;
            }

            //Make all methods public
            var privateMethods = planetbaseModule.Types.Where(type => type.Namespace.Equals("Planetbase") && type.HasMethods) //Get all types
                .SelectMany(type => type.Methods)    //Get all methods
                .Where(Methods => !Methods.IsPublic);   //Get all private methods

            foreach (var method in privateMethods)
            {
                method.IsPublic = true;
            }

            //Add hooks for prefab replacement
            var moduleTypeLoadPrefabMethod = planetbaseModule.GetType("Planetbase", "ModuleType").Methods
                .FirstOrDefault(method => method.Name.Equals("loadPrefab"));
            moduleTypeLoadPrefabMethod.IsVirtual = true;

            //Add hooks for new menu item
            var setGameStateTitleMethod = gameManagerType.Methods
                .FirstOrDefault(method => method.Name.Equals("setGameStateTitle"));

            var titleGameStateReplacementConstructor = frameworkAssembly.MainModule.Types.FirstOrDefault(type =>
                    type.FullName.Equals("PlanetbaseFramework.GameStateTitleReplacement")).Methods
                .FirstOrDefault(method => method.Name.Equals(".ctor"));

            var planetbaseScopeTitleGameStateReplacementConstructor = planetbaseModule.ImportReference(titleGameStateReplacementConstructor);

            setGameStateTitleMethod.Body.Instructions
                    .FirstOrDefault(instruction => instruction.OpCode == OpCodes.Newobj).Operand =
                planetbaseScopeTitleGameStateReplacementConstructor;

            //Remove bad references
            var mscorlib4Reference = planetbaseModule.AssemblyReferences.FirstOrDefault(assembly =>
                assembly.Name.Equals("mscorlib") && assembly.Version.Equals(new Version(4, 0, 0, 0)));
            planetbaseModule.AssemblyReferences.Remove(mscorlib4Reference);

            var planetbaseFrameworkReference = planetbaseModule.AssemblyReferences.FirstOrDefault(reference => reference.Name.Equals("Assembly-CSharp"));
            planetbaseModule.AssemblyReferences.Remove(planetbaseFrameworkReference);

            //Set the .NET version to 4.0.0.0
            var mscorlibReference =
                planetbaseModule.AssemblyReferences.FirstOrDefault(assembly => assembly.Name.Equals("mscorlib") && assembly.Version.Equals(new Version(2, 0, 5, 0)));
            mscorlibReference.Version = new Version(4, 0, 0, 0);
            mscorlibReference.PublicKeyToken = null;

            //Save the file
            planetbaseModule.Write("Assembly-CSharp2.dll");
            
            buttonPatch.Content = "Done!";
        }

        private static void DownloadFramework()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //Stupid .net
            var client = new WebClient();
            client.Headers.Add("user-agent", "a potato");

            dynamic deserializedJson = new JavaScriptSerializer().DeserializeObject(
                client.DownloadString("https://api.github.com/repos/soliddowant/planetbase-framework/releases/latest"));
            string frameworkUrl =
                ((dynamic[]) deserializedJson["assets"]).FirstOrDefault(asset =>
                    asset["content_type"].Equals("application/x-zip-compressed"))["browser_download_url"];

            client.DownloadFile(frameworkUrl, LocalFrameworkAssemblyPath);
            ZipFile.ExtractToDirectory(LocalFrameworkAssemblyPath, "./");
        }
    }
}