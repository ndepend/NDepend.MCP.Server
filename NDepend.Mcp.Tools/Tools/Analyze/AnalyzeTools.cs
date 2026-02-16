
using NDepend.Analysis;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;
using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.Analyze;

[McpServerToolType]
public static class AnalyzeTools {

    internal const string TOOL_RUN_ANALYSIS_NAME = Constants.TOOL_NAME_PREFIX + "run-analysis";

    [McpServerTool(Name = TOOL_RUN_ANALYSIS_NAME, Idempotent = false, Destructive = false, OpenWorld = false, ReadOnly = true),
     Description($"""
                 {Constants.PROMPT_CALL_INITIALIZE}
                  
                  **Purpose:**
                 Executes NDepend analysis on the initialized solution to refresh and update analysis results based on the current codebase state.
                 
                 **When to use this tool:**
                 - User explicitly requests: "run analysis", "analyze the code", "refresh the results", "update the results"
                 - Post-fix verification: "I fixed the issues, check again", "verify my changes"
                 - After code changes: "I updated the code, run analysis", "reanalyze after my modifications"
                 - After recompilation when user wants updated metrics
                 
                 **Prerequisites - MUST complete before calling:**
                 **CRITICAL:** The solution MUST be rebuilt in DEBUG mode first, or analysis will reflect stale/outdated code.
                 
                 **Recommended workflow:**
                 1. **Verify/trigger rebuild:** Ensure solution is rebuilt in DEBUG mode
                    - If uncertain, rebuild the solution before proceeding
                 2. **Run analysis:** Call `{TOOL_RUN_ANALYSIS_NAME}`
                 3. **Review results:** Use appropriate query tools to examine updated findings
                 4. **Report to user:** Summarize changes, improvements, or remaining issues
                 """)]
    public static async Task<bool> RunAnalysisTool(
            INDependService service,
            ILogger<AnalyzeToolsLog> logger,

            McpServer server,
            RequestContext<CallToolRequestParams> context,
            [Description("A cancellation token for interrupting and canceling the operation.")]
            CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
            {LogHelpers.TOOL_LOG_SEPARATOR}
            Invoking {TOOL_RUN_ANALYSIS_NAME}
            """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        // Show user analysis progress
        Action<int>? reportProgressProc = McpProgressHelpers.GetReportProgressProc(server, context, cancellationToken);

        return await Task.Run(() => {
            var project = session.Project;
            IAnalysisResult analysisResult = project.RunAnalysisWithLog(logger, service.StopWatchingForNewAnalysisResult, reportProgressProc);
            service.InitializeFromAnalysisResult(analysisResult, logger, reportProgressProc);
            return true;
        }, cancellationToken);
    }


    internal const string TOOL_RUN_ANALYSIS_BUILD_REPORT_NAME = Constants.TOOL_NAME_PREFIX + "run-analysis-build-report";

    [McpServerTool(Name = TOOL_RUN_ANALYSIS_BUILD_REPORT_NAME, Idempotent = false, Destructive = false, OpenWorld = false, ReadOnly = true),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  **Purpose:**
                  Executes NDepend analysis and generates an interactive HTML report for data visualization and exploration.
                  
                  **When to use this tool:**
                  - User explicitly requests: "show me a web report", "generate HTML report", "visualize the analysis"
                  - User wants interactive exploration: "I want to explore the results", "show me an interactive view"
                  - User asks for visual/graphical analysis outputs (NOT plain text summaries)
                  
                  **Required post-execution steps:**
                  1. Extract the HTML report file path from the tool's response
                  2. IMMEDIATELY open the HTML file in a web view/browser to display results
                  3. Inform the user that the interactive report is now available for viewing
                  
                  **Returns:** A message confirming report generation and providing the absolute file path to the HTML report.
                  
                  **CRITICAL:** Never return just the file path string to the user. Always render/display the HTML content.
                  
                  **Important notes:**
                  - This tool automatically refreshes analysis data (equivalent to running `{TOOL_RUN_ANALYSIS_NAME}`)
                  - If user only needs text-based results, use analysis query tools instead to avoid unnecessary HTML generation
                  - The HTML report may take a few moments to generate for large codebases
                  """)]
    public static async Task<string> RunAnalysisAndBuildReportTool(
            INDependService service,
            ILogger<AnalyzeToolsLog> logger,

            McpServer server,
            RequestContext<CallToolRequestParams> context,
            [Description("A cancellation token for interrupting and canceling the operation.")]
            CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             Invoking {TOOL_RUN_ANALYSIS_BUILD_REPORT_NAME}
             """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        // Show user analysis progress
        Action<int>? reportProgressProc = McpProgressHelpers.GetReportProgressProc(server, context, cancellationToken);

        return await Task.Run(() => {
            var project = session.Project;
            IAnalysisResult analysisResult = project.RunAnalysisAndBuildReportWithLog(logger, service.StopWatchingForNewAnalysisResult, reportProgressProc);
            string result =
$@"Report generated successfully!
View it here: ""file:///{analysisResult.AnalysisResultRef.ReportFilePath.ToString()!}""";
            logger.LogInformation(result);

            service.InitializeFromAnalysisResult(analysisResult, logger, reportProgressProc);
            return result;
        }, cancellationToken);
    }


    internal const string TOOL_GET_ANALYSIS_RESULT_INFO_NAME = Constants.TOOL_NAME_PREFIX + "get-analysis-result-info";

    [McpServerTool(Name = TOOL_GET_ANALYSIS_RESULT_INFO_NAME, Idempotent = true, Destructive = false, OpenWorld = false, ReadOnly = true),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  **Purpose:**
                  Retrieves metadata about the current analysis result and baseline result, including dates, project names, and file paths.
                  
                  **When to use this tool:**
                  - User asks: "when was the analysis run?", "what project is being analyzed?", "show analysis details"
                  - User wants to know: "what's the baseline date?", "what are the current analysis dates?"
                  - Debugging or verification: "which analysis version am I looking at?", "what's the project path?"
                  
                  **Returns:**
                  Information about both current and baseline analysis results including:
                  - Analysis result date and time
                  - Project name
                  - Project file path
                  - Baseline analysis result date and time
                  
                  **Note:** This tool does not run analysis, only retrieves information about existing loaded results.
                  """)]
    public static async Task<AnalysisResultInfo> GetAnalysisResultInfoTool(
            INDependService service,
            ILogger<AnalyzeToolsLog> logger,
            [Description("A cancellation token for interrupting and canceling the operation.")]
            CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             Invoking {TOOL_GET_ANALYSIS_RESULT_INFO_NAME}
             """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }
        return await Task.Run(() => {
            var result = session.AnalysisResult;
            var baseline = session.BaselineResult;

            return new AnalysisResultInfo(
                result.AnalysisResultRef.Date,
                result.AnalysisResultRef.Project.Properties.Name,
                result.AnalysisResultRef.Project.Properties.FilePath.ToString()!,
                baseline.AnalysisResultRef.Date
            );
        }, cancellationToken);
    }

}



