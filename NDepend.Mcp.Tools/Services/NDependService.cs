using System.Diagnostics;
using ModelContextProtocol;
using NDepend.Analysis;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Tools.Common;
using NDepend.Path;
using NDepend.Project;

namespace NDepend.Mcp.Services;

public class NDependService : INDependService {

    private Session? m_Session;
    public bool IsInitialized(out Session session) {
        session = m_Session!;
        return m_Session != null;
    }

    public async Task<Session> InitializeFromProjectOrSolutionAsync(string slnOrNdprojFilePath, ILogger logger, Action<int>? reportProgressProc) {
        StopWatchingForNewAnalysisResult();

        // Resolve which .ndproj to load from the provided .ndproj OR .sln/.slnx path
        // (logic moved here from InitializeTools.InitFromSolutionOrNdprojFilePath).
        IAbsoluteFilePath projectFilePath = await ResolveNDependProjectFilePathAsync(slnOrNdprojFilePath, logger);

        return InitializeFromNDependProjectFile(projectFilePath, logger, reportProgressProc);
    }

    // Resolve the .ndproj to load: an .ndproj passed directly, the .ndproj attached to / side-by-side
    // with a provided .sln/.slnx (created when missing), or one the user picks in the chooser dialog.
    // Throws an McpException listing the MRU solutions when nothing can be resolved.
    private static async Task<IAbsoluteFilePath> ResolveNDependProjectFilePathAsync(string slnOrNdprojFilePath, ILogger logger) {
        // Possibly the caller provided an .ndproj file directly, by-passing the demand for a solution path.
        if (ProjectHelpers.TryGetNDependProjectFromPath(slnOrNdprojFilePath, out IAbsoluteFilePath? projectFilePath)) {
            return projectFilePath!;
        }

        // Check the solution file path argument.
        if (!SolutionHelpers.TryGetExistingSolutionFilePath(
                slnOrNdprojFilePath,
                logger,
                out IAbsoluteFilePath solutionFilePath,
                out List<IAbsoluteFilePath> mruSlnFilePaths)) {

            // Solution/project not resolved: on Windows show a dialog to let the user pick an .ndproj.
            projectFilePath = await ProjectHelpers.AskTheUserForNDependProjectAsync();
            if (projectFilePath != null) { return projectFilePath; }
            throw BuildMcpExceptionToChooseAmongMRUSolutions(logger, slnOrNdprojFilePath, mruSlnFilePaths);
        }

        // Get the .ndproj attached to the solution or side-by-side with it.
        if (ProjectFromSolutionHelpers.TryGetNDependProjectFromSolution(logger, solutionFilePath, out projectFilePath)) {
            return projectFilePath!;
        }

        // No project found from the solution: create it side-by-side (analysis runs on first use).
        return ProjectFromSolutionHelpers.CreateNDependProjectSideBySideWithTheSolution(logger, solutionFilePath).Properties.FilePath;
    }

    private static McpException BuildMcpExceptionToChooseAmongMRUSolutions(ILogger logger, string solutionFilePath, List<IAbsoluteFilePath> mruSlnFilePaths) {
        logger.LogError(
            $"""
             The provided solution file path `{solutionFilePath}` is not valid.
             Thrown an McpException with the list of most recently used solution file paths to let the user or the LLM pick one and call again with the selected solution file path.
             """);
        var sb = new StringBuilder(
            $"""
             ERROR: The solution file path `{solutionFilePath}` is not valid or doesn't exist.
             ACTION REQUIRED: Select a path from the Most Recently Used solutions below and call `{Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME}` using the selected path.
             """);
        foreach (string path in mruSlnFilePaths.Select(p => p.ToString()!)) {
            sb.Append($@"
- `{path}`");
        }
        return new McpException(sb.ToString());
    }

    // Load the given .ndproj, get (or run) its analysis result + baseline, and build the Session.
    private Session InitializeFromNDependProjectFile(IAbsoluteFilePath projectFilePath, ILogger logger, Action<int>? reportProgressProc) {
        logger.LogInformation($"Initializing from the NDepend project file `{projectFilePath}`.");

        if (!projectFilePath.Exists) {
            logger.LogErrorAndThrow($"The NDepend project file `{projectFilePath}` does not exist.");
        }
        var projectManager = new NDependServicesProvider().ProjectManager;
        IProject? project = projectManager.LoadProject(projectFilePath);

        IAnalysisResult result;
        IAnalysisResult baselineResult; // If no baseline result, define it as the current result (which will mean no diff)
        if (!project.TryGetMostRecentAnalysisResultRef(out IAnalysisResultRef resultRef)) {
            // If no analysis result available for project, run analysis on this project to get one
            logger.LogInformation($"The NDepend project file `{projectFilePath}` has no analysis result available. Run the analysis.");
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

        Session session = CreateNewSessionWhichStartComputeIssuesAsync(logger, result, baselineResult);
        logger.LogInformation("Issues computed and initialization ok.");
        return session;
    }

    public bool InitializeFromAnalysisResult(IAnalysisResult analysisResult, ILogger logger, Action<int>? reportProgressProc, out Session session) {
        logger.LogInformation($"Initialize from the analysis result obtained on {analysisResult.AnalysisResultRef.Date.GetString()}.");
        IAnalysisResult baselineResult = analysisResult; // If no baseline result, define it as the current result (which will mean no diff)
        if (IsInitialized(out Session sessionTmp)) {
            var ar = sessionTmp.AnalysisResult!;
            logger.LogInformation($"Baseline is the previous analysis result obtained on {ar.AnalysisResultRef.Date.GetString()}.");
            baselineResult = ar;

        } else if (analysisResult.AnalysisResultRef.Project.BaselineDuringAnalysis.TryGetAnalysisResultRefToCompareWith(out IAnalysisResultRef baselineRef) == TryGetAnalysisResultRefToCompareWithResult.DoCompareWith) {
            logger.LogInformation($"Loading the baseline analysis result obtained on {baselineRef.Date.GetString()}.");
            baselineResult = baselineRef.Load();
        }
        session = CreateNewSessionWhichStartComputeIssuesAsync(logger, analysisResult, baselineResult);
        logger.LogInformation("Issues computed and initialization ok.");
        return true;
    }


    private Session CreateNewSessionWhichStartComputeIssuesAsync(
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
        return session;
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
        InitializeFromAnalysisResult(analysisResult, @params.Logger, _ => { }, out _);
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

