using System.Diagnostics;
using NDepend.Analysis;
using NDepend.Path;
using NDepend.Project;

namespace NDepend.Mcp.Helpers {
    internal static class ProjectHelpers {

        internal static IProject CreateNDependProjectSideBySideWithTheSolution(ILogger logger,  IAbsoluteFilePath solutionFilePath) {
            var projectFilePath = ProjectPathHelpers.GetSideBySideProjectFile(solutionFilePath);
            Debug.Assert(!projectFilePath.Exists); // Coz we searched for side-by-side file before calling this method
            var projectManager = new NDependServicesProvider().ProjectManager;
            var project = projectManager.CreateBlankProject(projectFilePath, solutionFilePath.FileNameWithoutExtension);
            project.CodeToAnalyze.SetIDEFiles([new IDEFile(solutionFilePath, "", IDEFileRootDirResolvingInfo.Default)]);
            projectManager.SaveProject(project);
            return project;
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
