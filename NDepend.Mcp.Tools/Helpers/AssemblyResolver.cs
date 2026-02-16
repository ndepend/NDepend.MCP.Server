using System.Diagnostics;

namespace NDepend.Mcp.Helpers {

    public sealed class AssemblyResolver {

        public AssemblyResolver(string relativePathToLib) {
            // Assert we have a relative path to the NDepend lib folder!
            Debug.Assert(!string.IsNullOrEmpty(relativePathToLib));
            Debug.Assert(relativePathToLib.Length >= 5);
            Debug.Assert(relativePathToLib[0] == '.');
            Debug.Assert(relativePathToLib.EndsWith(System.IO.Path.DirectorySeparatorChar + "lib", StringComparison.OrdinalIgnoreCase));

            relativePathToLib += System.IO.Path.DirectorySeparatorChar;
            m_RelativePathToLib = relativePathToLib;
        }

        private readonly string m_RelativePathToLib;

        public Assembly? AssemblyResolveHandler(object? sender, ResolveEventArgs args) {
            Debug.Assert(args != null);

            var assemblyName = new AssemblyName(args.Name);
            Debug.Assert(assemblyName != null);

            var assemblyNameString = assemblyName.Name;
            Debug.Assert(!string.IsNullOrEmpty(assemblyNameString));

            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Debug.Assert(!string.IsNullOrEmpty(location));

            string relativePathToLib = m_RelativePathToLib;

            string GetAsmFilePath(string relativePathToLibTmp) {
                var sb = new StringBuilder(relativePathToLibTmp);
                sb.Append(assemblyName.Name).Append(".dll");
                return System.IO.Path.Combine(location, sb.ToString());
            }

            if (assemblyNameString.Equals("System.Management",StringComparison.OrdinalIgnoreCase) ||
                assemblyNameString.Equals("System.CodeDom", StringComparison.OrdinalIgnoreCase)) {

                // Find the highest .NET version available in the NDepend redistributable
                // It contains both assemblies, start with .NET 20.0 and go down till .NET 10.0
                for (int i = 20; i >= 10; i--) {
                    string relativePathToLibTmp = m_RelativePathToLib.Replace(@"\Lib", $@"\net{i}.0", StringComparison.OrdinalIgnoreCase);
                    string asmFilePathTmp = GetAsmFilePath(relativePathToLibTmp);
                    if (File.Exists(asmFilePathTmp)) {
                        relativePathToLib = relativePathToLibTmp;
                        break;
                    }
                }
            }

            var asmFilePath = GetAsmFilePath(relativePathToLib);
            if (!File.Exists(asmFilePath)) { return null; }

            var assembly = Assembly.LoadFrom(asmFilePath);
            return assembly;
        }
    }
}
