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

            //
            // Possibly the Agent provide an .ndproj file directly (NDepend project file)
            // by-passing the demand for the solution path to infer the .ndproj
            //
            if (ProjectHelpers.TryGetNDependProjectFromPath(solutionFilePath, out IAbsoluteFilePath? projectFilePath)) {
                goto INIT_FROM_PROJECT_FILE;
            }

            //
            // Check solution file path argument
            //
            if (!SolutionHelpers.TryGetExistingSolutionFilePath(
                   solutionFilePath,
                   logger,
                   out IAbsoluteFilePath solutionFilePathTyped,
                   out List<IAbsoluteFilePath> mruSlnFilePaths
                   )) {

                // If solution or project not resolved, 
                // we show a dialog (on Windows only) to let the user select an existing NDepend project file
                projectFilePath = await ProjectHelpers.AskTheUserForNDependProjectAsync();
                if (projectFilePath != null) {
                    goto INIT_FROM_PROJECT_FILE;
                }
                ThrowAnMcpExceptionAskingLLMToChooseAmongMRUSolutions(logger, solutionFilePath, mruSlnFilePaths);
            }

            // Try get the .ndproj attached to the solution ro side-by-side with the solution
            if (ProjectFromSolutionHelpers.TryGetNDependProjectFromSolution(logger, solutionFilePathTyped, out projectFilePath)) {
                goto INIT_FROM_PROJECT_FILE;
            }

            // If no project found from solution, create it and run analysis
            return CreateNDependProjectSideBySideWithSolutionAndAnalyzeIt(service, logger, solutionFilePathTyped, reportProgressProc);

            INIT_FROM_PROJECT_FILE:
            if (!service.InitializeFromProject(projectFilePath!.ToString()!, logger, reportProgressProc, out Session session)) {
                logger.LogErrorAndThrow($"Failed to initialize the MCP server from the project `{projectFilePath.ToString()}`.");
            }
            return AnalysisResultInfo.FromSession(session);
        }, cancellationToken);
    }



    private static void ThrowAnMcpExceptionAskingLLMToChooseAmongMRUSolutions(ILogger<InitializeToolsLog> logger, string solutionFilePath,
        List<IAbsoluteFilePath> mruSlnFilePaths) {
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
        throw new McpException(sb.ToString());
    }



    private static AnalysisResultInfo CreateNDependProjectSideBySideWithSolutionAndAnalyzeIt(
                INDependService service, 
                ILogger<InitializeToolsLog> logger,
                IAbsoluteFilePath solutionFilePath, 
                Action<int>? reportProgressProc) {
        var project = ProjectFromSolutionHelpers.CreateNDependProjectSideBySideWithTheSolution(
            logger,
            solutionFilePath);

        var analysisResult = project.RunAnalysisWithLog(
            logger,
            service.StopWatchingForNewAnalysisResult,
            reportProgressProc);

        if (!service.InitializeFromAnalysisResult(analysisResult, logger, _ => { }, out Session session)) {
            logger.LogErrorAndThrow($"Failed to initialize the MCP server from the analysis result `{analysisResult.AnalysisResultRef.AnalysisResultFilePath.ToString()}` of the project `{project.Properties.FilePath.ToString()}`.");
        }
        return AnalysisResultInfo.FromSession(session);
    }



}



