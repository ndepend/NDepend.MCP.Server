using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Web;
using NDepend.DotNet.VisualStudio;
using NDepend.Mcp.Tools.Common;
using NDepend.Path;


namespace NDepend.Mcp.Helpers {
    internal static class SolutionHelpers {


        //
        // Heuristic to recover the solution file path when the one provided by the Agent/LLM is not valid.
        // Despite our instructions, Copilot can provide solution file path like
        //   "File.sln"
        //   "Path\File.sln"
        //   "C:\WrongPath\File.sln"
        //   "WrongFile.sln"
        // Might need to find the solution file path in the VS MRU list by matching only the solution file name without path and without extension
        //
        internal static bool TryGetValidSolutionFilePath(
                string solutionFilePathStr, 
                ILogger<InitializeToolsLog> logger, 
                out IAbsoluteFilePath solutionFilePath,
                out List<IAbsoluteFilePath> mruSlnFilePaths) {

            mruSlnFilePaths = GetMRUSlnFilePaths();

            if (!solutionFilePathStr.TryGetAbsoluteFilePath(out solutionFilePath)) {
                if (!TryGetMRUSlnFilePathFromMalformedSlnFilePath(
                        solutionFilePathStr, mruSlnFilePaths, 
                        out IAbsoluteFilePath? tmp)) {
                    logger.LogError(
                        $"The provided solution file path `{solutionFilePathStr}` is not a valid absolute file path (including the drive letter).");
                    return false;
                }
                solutionFilePath = tmp!;
            }

            string slnExt = solutionFilePath.FileExtension;
            bool isSlnxExt = false;
            switch (slnExt.ToLowerInvariant()) {
                case ".sln": break;
                case ".slnx": isSlnxExt = true; break;
                default:
                    logger.LogError($"The provided solution file path `{solutionFilePathStr}` has not a solution extension (.sln or .slnx).");
                    return false;
            }
            if (!solutionFilePath.Exists) {
                // Need to check both .sln and .slnx, the agent sometime provide the path with the wrong extension
                var solutionFilePathTyped = (isSlnxExt ? solutionFilePathStr.Substring(0, solutionFilePathStr.Length - 1) : solutionFilePathStr + "x").ToAbsoluteFilePath();
                if (solutionFilePathTyped.Exists) {
                    solutionFilePath = solutionFilePathTyped;
                    //isSlnxExt = !isSlnxExt;
                } else if (!TryGetMRUSlnFilePathFromMalformedSlnFilePath(solutionFilePath.FileNameWithoutExtension, mruSlnFilePaths, out var tmp)) {
                    solutionFilePath = tmp!;
                } else {
                    logger.LogError($"The provided solution file path `{solutionFilePathStr}` does not exist.");
                    return false;
                }
            }
            logger.LogInformation($"Initialize the {Constants.NDEPEND_MCP_SERVER} with the solution `{solutionFilePath.ToString()}`.");
            return true;
        }





        private static bool TryGetMRUSlnFilePathFromMalformedSlnFilePath(
                string malformed, List<IAbsoluteFilePath> mruSlnFilePaths, out IAbsoluteFilePath? solutionFilePathTyped) {
            // Extract only the solution file name without path and without extension
            int indexSep = malformed.LastIndexOfAny(['\\', '/']);
            string fileName = indexSep > 0 ? malformed.Substring(indexSep) : malformed;

            // Remove extension if any, the agent sometime provide the solution file name with the extension, but sometime without extension
            foreach (var ext in new [] { ".sln", ".slnx", ".csproj", ".vbproj", ".proj" }) {
                if(fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) {
                    fileName = malformed.Substring(0, malformed.Length - ext.Length);
                    break;
                }
            }
            return TryGetMRUSlnFilePathFromSlnName(fileName, mruSlnFilePaths, out solutionFilePathTyped) ||
                   // Sometime Copilot provide PartialSolutionName only!
                   TryGetMRUSlnFilePathWhichContainSlnName(fileName, mruSlnFilePaths, out solutionFilePathTyped);
        }

        private static bool TryGetMRUSlnFilePathFromSlnName(
                string solutionFileNameWithNoExtension, 
                List<IAbsoluteFilePath> mruSlnFilePaths, 
                out IAbsoluteFilePath? solutionFilePathTyped) {
            solutionFilePathTyped = mruSlnFilePaths
                .FirstOrDefault(p => 
                    p.FileNameWithoutExtension.Equals(solutionFileNameWithNoExtension, StringComparison.OrdinalIgnoreCase));
            return solutionFilePathTyped != null;
        }

        private static bool TryGetMRUSlnFilePathWhichContainSlnName(
                string solutionFileNameWithNoExtension, 
                List<IAbsoluteFilePath> mruSlnFilePaths, 
                out IAbsoluteFilePath? solutionFilePathTyped) {
            solutionFilePathTyped = mruSlnFilePaths
                .FirstOrDefault(p =>
                    p.FileNameWithoutExtension.Contains(solutionFileNameWithNoExtension, StringComparison.OrdinalIgnoreCase));
            return solutionFilePathTyped != null;
        }



        private static List<IAbsoluteFilePath> GetMRUSlnFilePaths() {

            var mruSlnFilePaths = new List<IAbsoluteFilePath>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                // Get the MRU solution file paths from installed Visual Studio versions (only on Windows)
                var vsManager = new NDependServicesProvider().VisualStudioManager;
                foreach (var vsVersion in new[] { VisualStudioVersion.V18_2026, VisualStudioVersion.V17_2022, VisualStudioVersion.V16_2019 }) {
                    if (vsManager.IsVisualStudioVersionInstalled(vsVersion)) {
                        var list = vsManager.GetMostRecentlyUsedVisualStudioSolutionOrProject(vsVersion);
                        mruSlnFilePaths.AddRange(list);
                    }
                }
            }

            // Get the MRU solution file paths from VSCode
            var mruSlnFilePathsVSCode = GetMRUSlnFilePathsFromVSCode();
            mruSlnFilePaths.AddRange(mruSlnFilePathsVSCode);

            // Avoid duplicates, Distinct() keep the file paths in order
            return mruSlnFilePaths.Distinct().ToList(); 
        }


        // Get the MRU solution file paths from VSCode 
        private static List<IAbsoluteFilePath> GetMRUSlnFilePathsFromVSCode() {
            var list = new List<IAbsoluteFilePath>();

            string storagePath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                // Windows:  %APPDATA%\Code\User\globalStorage\storage.json
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                storagePath = System.IO.Path.Combine(appData, "Code", "User", "globalStorage", "storage.json");
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                // MacOS:   /Users/<user>/Library/Application Support/Code/User/globalStorage/storage.json
                string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                storagePath = System.IO.Path.Combine(home, "Library", "Application Support", "Code", "User", "globalStorage", "storage.json");
            } else { 
                // Assume 	Linux: /home/<user>/.config/Code/User/globalStorage/storage.json
                string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                storagePath = System.IO.Path.Combine(home, ".config", "Code", "User", "globalStorage", "storage.json");
            }

            // Check if file exists
            if (!File.Exists(storagePath)) {
                return list;
            }

            try {
                // Read and parse JSON from  %APPDATA%\Code\User\globalStorage\storage.json
                // {
                // ...
                //   "profileAssociations": {
                //       "workspaces": {
                //           "file:///c%3A/Dir1": "__default__profile__",
                //           "file:///c%3A/Dir2": "__default__profile__",
                //       },
                //       ...
                string json = File.ReadAllText(storagePath);
                using JsonDocument doc = JsonDocument.Parse(json);

                // Navigate to profileAssociations.workspaces
                if (doc.RootElement.TryGetProperty("profileAssociations", out JsonElement profileAssoc)) {
                    if (profileAssoc.TryGetProperty("workspaces", out JsonElement workspaces)) {
                        // Extract directories from workspace keys

                        foreach (JsonProperty workspace in workspaces.EnumerateObject()) {
                            string key = workspace.Name;

                            // Remove "file:///" prefix if present
                            const string FILE_PREFIX = "file:///";
                            if (key.StartsWith(FILE_PREFIX, StringComparison.OrdinalIgnoreCase)) {
                                key = key.Substring(FILE_PREFIX.Length);
                            }

                            // URL decode to handle encoded characters like %3A -> :
                            string decodedPath = HttpUtility.UrlDecode(key);

                            if (!decodedPath.TryGetAbsoluteDirectoryPath(out IAbsoluteDirectoryPath dir) ||
                               !dir.Exists) { continue; }

                            FillWithSlnFileIn(dir, list);
                        }
                    }
                }
            } catch {
                // ignored
            }

            return list;
        }

        private static void FillWithSlnFileIn(IAbsoluteDirectoryPath dir, List<IAbsoluteFilePath> list) {
            foreach(var file in dir.ChildrenFilesPath) {
                if(file.HasExtension(".slnx") || file.HasExtension(".sln")) {
                    list.Add(file);
                }
            }
        }
    }
}
