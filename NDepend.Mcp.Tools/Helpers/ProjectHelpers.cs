using System.Diagnostics;
using NDepend.Analysis;
using NDepend.Path;
using NDepend.Project;

namespace NDepend.Mcp.Helpers {
    internal static class ProjectHelpers {


        internal static bool TryGetNDependProjectFromPath(string filePath, out IAbsoluteFilePath? projectFilePath) {
            projectFilePath = null;
            if (!filePath.EndsWith(".ndproj", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }
            string projectFilePathStr = filePath;
            if (projectFilePathStr.TryGetAbsoluteFilePath(out projectFilePath) &&
                projectFilePath.Exists) {
                return true;
            }

            // Check if there is a recent project with the same file name as the provided one (which is not an absolute path here)
            var projectManager = new NDependServicesProvider().ProjectManager;
            var projectRefs = projectManager.GetMostRecentlyUsedProjects();
            var projectRef = projectRefs.FirstOrDefault(p => p.ProjectFilePath.ToString()!.EndsWith(projectFilePathStr, StringComparison.OrdinalIgnoreCase));
            if (projectRef == null) {
                return false;
            }
            projectFilePath = projectRef.ProjectFilePath;
            return true;
        }


        internal static async Task<IAbsoluteFilePath?> AskTheUserForNDependProjectAsync() {
            if (!OperatingSystem.IsWindows()) { return null; } // Show project dialog only works on Windows

            // Get the directory where .NET Core app is running
            string? coreAppDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if(coreAppDirectory == null) { return null; }

            // Resolve the chooser exe path relative to it
            // Not found in case of MCP SSE server, only MCP Stdio server running on local can show a project choose form
            string projectChooserExePath = System.IO.Path.Combine(coreAppDirectory, "NDepend.Project.Chooser.exe");
            if(!File.Exists(projectChooserExePath)) { return null; } 

            var processInfo = new ProcessStartInfo {
                FileName = projectChooserExePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);

            // Read the output
            string output = await process!.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0 ||
                !output.TryGetAbsoluteFilePath(out IAbsoluteFilePath? projectFilePath)) {
                return null;
            }
            return projectFilePath;
        }





        extension(IProject project) {
            internal IAnalysisResult RunAnalysisWithLog(ILogger logger, Action stopWatchingForNewAnalysisResultProc, Action<int>? reportProgressProc = null) {
                // Avoid the risk to load the analysis result about to be created by RunAnalysis()
                stopWatchingForNewAnalysisResultProc();
                return project.RunAnalysis(
                    log => {
                        switch (log.Category) {
                            case AnalysisLogCategory.Info: logger.LogInformation(log.Description); break;
                            case AnalysisLogCategory.Warning: logger.LogWarning(log.Description); break;
                            case AnalysisLogCategory.Error: logger.LogError(log.Description); break;
                        }
                    },
                    progressLog => {
                        reportProgressProc?.Invoke(progressLog.EstimatedPercentageDone);
                    });
            }

            internal IAnalysisResult RunAnalysisAndBuildReportWithLog(ILogger logger, Action stopWatchingForNewAnalysisResultProc, Action<int>? reportProgressProc = null) {
                // Avoid the risk to load the analysis result about to be created by RunAnalysis()
                stopWatchingForNewAnalysisResultProc();
                var analysisResult = project.RunAnalysisAndBuildReport(
                    log => {
                        switch (log.Category) {
                            case AnalysisLogCategory.Info: logger.LogInformation(log.Description); break;
                            case AnalysisLogCategory.Warning: logger.LogWarning(log.Description); break;
                            case AnalysisLogCategory.Error: logger.LogError(log.Description); break;
                        }
                    },
                    progressLog => {
                        reportProgressProc?.Invoke(progressLog.EstimatedPercentageDone);
                    });
                BrowserHelpers.OpenLocalHtml(analysisResult.AnalysisResultRef.ReportFilePath, logger);
                return analysisResult;
            }
        }
    }
}
