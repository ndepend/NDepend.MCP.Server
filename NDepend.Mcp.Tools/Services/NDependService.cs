using NDepend.Analysis;
using NDepend.Mcp.Helpers;
using NDepend.Path;
using NDepend.Project;

namespace NDepend.Mcp.Services;

public class NDependService : INDependService {

    private Session? m_Session;
    public bool IsInitialized(out Session session) {
        session = m_Session!;
        return m_Session != null;
    }

    public bool InitializeFromProject(string projectFilePathStr, ILogger logger, Action<int>? reportProgressProc = null) {
        StopWatchingForNewAnalysisResult();
        logger.LogInformation($"Initializing from the NDepend project file `{projectFilePathStr}`.");

        if(!projectFilePathStr.TryGetAbsoluteFilePath(out IAbsoluteFilePath projectFilePath)) {
            logger.LogErrorAndThrow($"The NDepend project file path `{projectFilePathStr}` is not a valid absolute file path.");
        }

        if (!projectFilePath.Exists) {
            logger.LogErrorAndThrow($"The NDepend project file `{projectFilePath}` does not exist.");
        }
        var projectManager = new NDependServicesProvider().ProjectManager;
        IProject? project = projectManager.LoadProject(projectFilePath);

        
        IAnalysisResult result;
        IAnalysisResult baselineResult; // If no baseline result, define it as the current result (which will mean no diff)

        if (!project.TryGetMostRecentAnalysisResultRef(out IAnalysisResultRef resultRef)) {
            // If no analysis result available for project, run analysis on this project to get one
            logger.LogErrorAndThrow($"The NDepend project file `{projectFilePath}` has no analysis result available. Run the analysis.");
            result =  project.RunAnalysisWithLog(logger, this.StopWatchingForNewAnalysisResult, reportProgressProc);
            baselineResult = result;

        } else {
            var loadTask = Task.Run(() => resultRef.Load(progressLog => { reportProgressProc?.Invoke(progressLog.EstimatedPercentageDone); }));

            if (project.BaselineDuringAnalysis.TryGetAnalysisResultRefToCompareWith(out IAnalysisResultRef baselineRef) == TryGetAnalysisResultRefToCompareWithResult.DoCompareWith) {
                // Concurrent load of current snapshot (or run analysis) and baseline snapshot (if any)
                logger.LogInformation(
                    $"Loading both the current analysis result obtained on {resultRef.Date.GetString()} and the baseline analysis result obtained on {baselineRef.Date.GetString()}.");
                var loadBaselineTask = Task.Run(() => baselineRef.Load());
                Task.WhenAll(loadTask, loadBaselineTask).Wait();
                baselineResult = loadBaselineTask.Result;
            } else {
                logger.LogInformation(
                    $"Loading the current analysis result obtained on {resultRef.Date.GetString()}. No baseline analysis result available.");
                loadTask.Wait();
                baselineResult = loadTask.Result;
            }
            result = loadTask.Result;
        }

        CreateNewSessionWhichStartComputeIssuesAsync(logger, result, baselineResult);
        logger.LogInformation("Issues computed and initialization ok.");
        return true;
    }

    public bool InitializeFromAnalysisResult(IAnalysisResult analysisResult, ILogger logger, Action<int>? reportProgressProc = null) {
        logger.LogInformation($"Initialize from the analysis result obtained on {analysisResult.AnalysisResultRef.Date.GetString()}.");
        IAnalysisResult baselineResult = analysisResult; // If no baseline result, define it as the current result (which will mean no diff)
        if (IsInitialized(out Session session)) {
            var ar = session.AnalysisResult!;
            logger.LogInformation($"Baseline is the previous analysis result obtained on {ar.AnalysisResultRef.Date.GetString()}.");
            baselineResult = ar;

        } else if (analysisResult.AnalysisResultRef.Project.BaselineDuringAnalysis.TryGetAnalysisResultRefToCompareWith(out IAnalysisResultRef baselineRef) == TryGetAnalysisResultRefToCompareWithResult.DoCompareWith) {
            logger.LogInformation($"Loading the baseline analysis result obtained on {baselineRef.Date.GetString()}.");
            baselineResult = baselineRef.Load();
        }
        CreateNewSessionWhichStartComputeIssuesAsync(logger, analysisResult, baselineResult);
        logger.LogInformation("Issues computed and initialization ok.");
        return true;
    }


    private void CreateNewSessionWhichStartComputeIssuesAsync(
            ILogger logger,
            IAnalysisResult result,
            IAnalysisResult baselineResult) {

        // Set a file watcher so when a new analysis result is created, we reload it to get the latest issues
        var ndarWatcher = new NdarWatcher(
            new NewAnalysisResultParams(result.AnalysisResultRef.Project, logger, result.AnalysisResultRef.AnalysisResultFilePath),
            this.OnNewAnalysisResultHandler);
        // NOTE: we could watch for assemblies and re-analyze when they've all been compiled!!
        //       but this would interfer with the NDepend VS extension which trigger analysis already.
        //IAbsoluteFilePath[] appAsmFilePaths = result.CodeBase.Application.Assemblies.Select(a => a.FilePath).ToArray();

        // Build the Session object that which starts async computation of compare-context and then issues
        var session = new Session(result, baselineResult, logger, ndarWatcher);
        if (m_Session != null) {
            m_Session.Dispose();
        }
        m_Session = session;
    }


    // This handler method is called when a new analysis result (.ndar file) is available for the currently loaded project.
    private void OnNewAnalysisResultHandler(NewAnalysisResultParams @params) {
        if (!@params.Project.TryGetMostRecentAnalysisResultRef(out IAnalysisResultRef resultRef) ||
            !resultRef.AnalysisResultFilePath.Equals(@params.NewNdarFilePath)) {
            return;
        }
        @params.Logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             New analysis result detected `{@params.NewNdarFilePath.ToString()}` obtained at {resultRef.Date}. Load it!
             """);
        IAnalysisResult analysisResult = resultRef.Load();
        InitializeFromAnalysisResult(analysisResult, @params.Logger);
    }

    // Stop watching for new analysis result 
    // This method is called when a new analysis is executed from this MCP server
    public void StopWatchingForNewAnalysisResult() {
        var session = m_Session;
        if(session != null) {
            session.StopWatchingForNewAnalysisResult();
            m_Session = null;
        }
    }


}

