
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NDepend.Mcp.Helpers;
using NDepend.Path;

namespace NDepend.Project.Chooser {

    internal sealed class Program {


        // Need a static field to prevent GC collection. Creating it also hooks AssemblyResolve and picks
        // ..\Lib (dev tree) or ..\..\..\..\ndepend\Lib (redistributable) via NDependRuntimeContext.
        static readonly AssemblyResolver s_AssemblyResolver = NDependRuntimeContext.RegisterAssemblyResolver();

        [STAThread]
        // Need this .NET Framework executable because the NDepend API
        //   projectManager.ShowDialogChooseAnExistingProject
        // only works on Windows, .NET Framework so far
        static void Main(string[] args) {
            AppDomain.CurrentDomain.AssemblyResolve += s_AssemblyResolver.AssemblyResolveHandler;

            MainSub(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // So we let a chance to resolved NDepend.API.Dll
        private static void MainSub(string[] args) {

            // "sln" argument: show the Visual Studio solutions/projects selection dialog instead of the NDepend project one.
            if (args.Length > 0 && string.Equals(args[0], "sln", StringComparison.OrdinalIgnoreCase)) {
                ChooseSolutions();
                return;
            }

            var projectManager = new NDependServicesProvider().ProjectManager;
            if (!projectManager.ShowDialogChooseAnExistingProject(MainWindowHandle, out IProject? project)) {
                Environment.Exit(1);
            }
            IAbsoluteFilePath projectFilePath = project.Properties.FilePath;
            Console.WriteLine(projectFilePath.ToString());
            Environment.Exit(0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // So we let a chance to resolved NDepend.API.Dll
        private static void ChooseSolutions() {
            var visualStudioManager = new NDependServicesProvider().VisualStudioManager;
            if (!visualStudioManager.ShowDialogSelectVisualStudioSolutionsOrProjects(MainWindowHandle, out ICollection<IAbsoluteFilePath> solutionsOrProjectsFilePaths) ||
                solutionsOrProjectsFilePaths.Count == 0) {
                Environment.Exit(1);
            }
            // So far only filter the first solution chosen, because the MCP tool that calls this chooser only supports one solution at a time.
            string output = solutionsOrProjectsFilePaths.First().ToString();
            Console.WriteLine(output);
            Environment.Exit(0);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr MainWindowHandle => GetConsoleWindow();
    }
}
