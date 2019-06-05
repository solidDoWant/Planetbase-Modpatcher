using Mono.Cecil;

namespace Patcher
{
    public class PlanetbaseResolver : BaseAssemblyResolver
    {
        private readonly DefaultAssemblyResolver _defaultResolver;

        public string FirstPassPath { get; }
        public string UnityPath { get; }
        public string UiPath { get; }

        public PlanetbaseResolver(string firstPassPath, string unityPath, string uiPath)
        {
            _defaultResolver = new DefaultAssemblyResolver();

            FirstPassPath = firstPassPath;
            UnityPath = unityPath;
            UiPath = uiPath;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            try
            {
                return _defaultResolver.Resolve(name);
            }
            catch (AssemblyResolutionException ex)
            {
                if (ex.AssemblyReference.Name.Equals("UnityEngine"))
                {
                    return AssemblyDefinition.ReadAssembly(UnityPath);
                }

                if (ex.AssemblyReference.Name.Equals("Unity.UI"))
                {
                    return AssemblyDefinition.ReadAssembly(UiPath);
                }

                if (ex.AssemblyReference.Name.Equals("Assembly-CSharp-firstpass"))
                {
                    return AssemblyDefinition.ReadAssembly(FirstPassPath);
                }

                throw;
            }
        }
    }
}
