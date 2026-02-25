
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NDepend.Mcp.Helpers;
using NDepend.Path;

namespace NDepend.Project.Chooser {

    internal sealed class Program {


        // Need a static field to prevent GC collection
        static readonly AssemblyResolver s_AssemblyResolver = new(@"..\..\..\..\ndepend\Lib");

        [STAThread]
        // Need this .NET Framework executable because the NDepend API 
        //   projectManager.ShowDialogChooseAnExistingProject
        // only works on Windows, .NET Framework so far 
        static void Main(string[] args) {
            AppDomain.CurrentDomain.AssemblyResolve += s_AssemblyResolver.AssemblyResolveHandler;
            MainSub();
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // So we let a chance to resolved NDepend.API.Dll
        private static void MainSub() {
            var projectManager = new NDependServicesProvider().ProjectManager;
            if (!projectManager.ShowDialogChooseAnExistingProject(MainWindowHandle, out IProject? project)) {
                Environment.Exit(1);
            }
            IAbsoluteFilePath projectFilePath = project.Properties.FilePath;
            Console.WriteLine(projectFilePath.ToString());
            Environment.Exit(0);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr MainWindowHandle => GetConsoleWindow();
    }
}
