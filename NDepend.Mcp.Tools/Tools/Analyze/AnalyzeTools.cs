
using NDepend.Analysis;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Tools.Initialize;

namespace NDepend.Mcp.Tools.Analyze;

[McpServerToolType]
public static class AnalyzeTools {

    internal const string TOOL_RUN_ANALYSIS_NAME = Constants.TOOL_NAME_PREFIX + "run-analysis";

    [McpServerTool(Name = TOOL_RUN_ANALYSIS_NAME, Idempotent = false, Destructive = false, OpenWorld = false, ReadOnly = true),
     Description($"""
                 {Constants.PROMPT_CALL_INITIALIZE}
                 
                 **Runs NDepend analysis** on the initialized solution to reflect the latest code changes.
                 
                 ## Use-Cases
                 - User requests: "run analysis", "analyze the code", "refresh/update results"
                 - Post-fix verification: "I fixed the issues, check again", "verify my changes"
                 - After code changes or recompilation when updated metrics are needed
                 
                 ## Workflow:
                 
                 1. **CRITICAL:** Solution MUST be rebuilt in DEBUG mode first, or results will reflect stale code.
                 2. Call `{TOOL_RUN_ANALYSIS_NAME}`
                 """)]
    public static async Task<bool> RunAnalysisTool(
            INDependService service,
            ILogger<AnalyzeToolsLog> logger,

            McpServer server,
            RequestContext<CallToolRequestParams> context,
            
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
            service.InitializeFromAnalysisResult(analysisResult, logger, reportProgressProc, out _);
            return true;
        }, cancellationToken);
    }


    internal const string TOOL_RUN_ANALYSIS_BUILD_REPORT_NAME = Constants.TOOL_NAME_PREFIX + "run-analysis-build-report";

    [McpServerTool(Name = TOOL_RUN_ANALYSIS_BUILD_REPORT_NAME, Idempotent = false, Destructive = false, OpenWorld = false, ReadOnly = true),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  Runs NDepend analysis and generates an interactive HTML report for visualization.
                  
                  ## When to Use
                  
                  - User requests: "show me a web report", "generate HTML report", "visualize the analysis"
                  - User wants interactive exploration of results
                  - Visual/graphical output needed (not plain text summaries)
                  
                  ## Post-Execution Steps
                  
                  - Extract the HTML report file path from the response
                  - Immediately open the HTML file in a web view/browser
                  - Inform the user the interactive report is ready
                  
                  ## Returns
                  
                  - Confirmation message with the absolute file path to the generated HTML report.
                  
                  ## Notes
                  
                  **CRITICAL:** Never return just the file path — always render/display the HTML content
                  - This tool automatically refreshes analysis data (equivalent to running `{TOOL_RUN_ANALYSIS_NAME}`)
                  - The HTML report may take a few moments to generate for large codebases
                  """)]
    public static async Task<string> RunAnalysisAndBuildReportTool(
            INDependService service,
            ILogger<AnalyzeToolsLog> logger,

            McpServer server,
            RequestContext<CallToolRequestParams> context,

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

            service.InitializeFromAnalysisResult(analysisResult, logger, reportProgressProc, out _);
            return result;
        }, cancellationToken);
    }


}



