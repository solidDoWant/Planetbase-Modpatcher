using Mono.Cecil;

namespace Patcher
{
    public class PBResolver : BaseAssemblyResolver
    {
        private readonly DefaultAssemblyResolver _defaultResolver;

        public string FirstPassPath { get; }
        public string UnityPath { get; }
        public string UIPath { get; }

        public PBResolver(string FirstPassPath, string UnityPath, string UIPath)
        {
            _defaultResolver = new DefaultAssemblyResolver();

            this.FirstPassPath = FirstPassPath;
            this.UnityPath = UnityPath;
            this.UIPath = UIPath;
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
                    return AssemblyDefinition.ReadAssembly(UIPath);
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
