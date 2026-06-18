using ModelContextProtocol;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;
using NDepend.Path;
using NDepend.Mcp.Helpers;


namespace NDepend.Mcp.Tools.Initialize;

[McpServerToolType]
public static class InitializeTools {


    [McpServerTool(Name = Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME, Idempotent = false, Destructive = false, OpenWorld = false, ReadOnly = true),
     Description(
        $"""
          The `{Constants.NDEPEND_MCP_SERVER}` provides focused, high-density .NET code analysis.
          
          **CRITICAL INITIALIZATION REQUIREMENT:**
          You MUST call `{Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME}` first before any other tools, to initialize the session with your solution.
          
          **CRITICAL SESSION MANAGEMENT:** Reuse the same MCP server instance for all tool calls; re-initialization is costly.
          
          **Typical Workflow:**
          
          1. Call `{Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME}` with the solution path
          2. Use other tools; all reference the initialized solution.
          3. Reuse the same `{Constants.NDEPEND_MCP_SERVER}` for the same solution.
          4. Start a new `{Constants.NDEPEND_MCP_SERVER}` instance only for a different solution.
          
          **Result:**
          
          - Success: Returns loaded analysis info.
          - Failure: Throws `McpException` with a list of Most Recently Used solution paths, choose one and call `{Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME}` again.
          """
        )]
    public static async Task<AnalysisResultInfo> InitializeFromSolutionTool(
          INDependService service,
          ILogger<InitializeToolsLog> logger,
          McpServer server,
          RequestContext<CallToolRequestParams> context,

          // Solution name might be ok since we investigate Most Recently Used VS and VSCode list
          // but the absolute path is better to avoid any ambiguity and to be sure the solution file is found.
          [Description(
            """
            **REQUIRED**: The .NET solution absolute file path OR the .NET solution file name.

            Example: 
            "C:\Users\name\source\repos\MySolution\MySolution.slnx"
            "MySolution.slnx"
            "MySolution"
            """)]
          // TODO -> Investigate opened VS solution instances to find the solution file path and name if the provided workspace parameter is not a valid solution file path or name.
          //         https://chatgpt.com/share/698eefe3-29c0-800d-a5d4-0cde6780bfa0
          string solutionFilePath,

          CancellationToken cancellationToken
        ) {

        logger.LogInformation(
            $"""
            {LogHelpers.TOOL_LOG_SEPARATOR}
            Invoking {Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME} with argument: workspace `{solutionFilePath}`.
            """);

        // Report analysis progress
        Action<int>? reportProgressProc = McpProgressHelpers.GetReportProgressProc(
            server,
            context,
            cancellationToken);

        return await Task.Run(async () => {
            // The full project/solution resolution + load now lives in the service (reused by the
            // MCP-server startup path too — see McpServerBootstrapBase.LoadNDependProjectAsync).
            Session session = await service.InitializeFromProjectOrSolutionAsync(solutionFilePath, logger, reportProgressProc);
            return AnalysisResultInfo.FromSession(session);
        }, cancellationToken);
    }


}



