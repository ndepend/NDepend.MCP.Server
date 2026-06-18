using System;
using System.Diagnostics;

namespace NDepend.Mcp.Helpers {

    // Single source of truth for "are we running from inside the NDepend dev tree?".
    // Signal: NDepend.AI.dll sits next to the running assemblies (bin\<Config>\net10.0).
    // Used to pick the NDepend.API resolution path AND the log directory.
    // NB: System.IO.Path is fully qualified because this lives under the NDepend.* namespace,
    //     where unqualified 'Path' would bind to the NDepend.Path namespace (same as AssemblyResolver.cs).
    public static class NDependRuntimeContext {

        // bin\<Config>\net10.0\NDepend.AI.dll present  =>  dev tree; otherwise shipped redistributable.
        private static bool IsInNDependDevTree { get; } =
            System.IO.File.Exists(System.IO.Path.Combine(AppContext.BaseDirectory, "NDepend.AI.dll"));

        // Relative path (from the executing assembly folder) to the NDepend 'Lib' folder
        // (NDepend.API.dll + backend):
        //   - dev tree:        ..\Lib                       (one level up: bin\<Config>\Lib)
        //   - redistributable: ..\..\..\..\ndepend\Lib
        // Built with Path.Combine so it's correct on Windows AND Linux/macOS.
        private static string RelativePathToLib { get; } =
            IsInNDependDevTree
                ? System.IO.Path.Combine("..", "Lib")
                : System.IO.Path.Combine("..", "..", "..", "..", "ndepend", "Lib");

        // Creates the resolver for the current context and hooks it to AssemblyResolve.
        // Returns it so the caller keeps it alive (prevents GC).
        public static AssemblyResolver RegisterAssemblyResolver() {
            return new AssemblyResolver(RelativePathToLib);
        }
    }
}
