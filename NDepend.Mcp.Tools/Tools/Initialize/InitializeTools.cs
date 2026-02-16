using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;
using NDepend.Path;
using NDepend.Mcp.Helpers;
using System.Diagnostics;

namespace NDepend.Mcp.Tools.Initialize;

[McpServerToolType]
public static class InitializeTools {


    [McpServerTool(Name = Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME, Idempotent = false, Destructive = false, OpenWorld = false, ReadOnly = true),
     Description(
        $"""
          The `{Constants.NDEPEND_MCP_SERVER}` provides focused, high-density .NET code analysis capabilities.
          
          **CRITICAL INITIALIZATION REQUIREMENT:**
          You MUST call `{Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME}` as your first action before using any other tools in this suite. This tool initializes the analysis session with your solution data.
          
          **REQUIRED PARAMETER:**
          - **YOU MUST PROVIDE** either the .NET solution absolute file path OR the .NET solution file name.

          **Important Session Management:**
          Once initialized, reuse the same MCP server instance for ALL subsequent tool calls in this conversation. 
          Re-initialization is computationally expensive and should be avoided.
          
          **Typical Workflow:**
          1. Call `{Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME}` with the solution path
          2. Use other analysis tools as needed throughout the session. 
             All tools will reference the initialized solution data
          3. Re-use the same `{Constants.NDEPEND_MCP_SERVER}` instance as long as the same solution is involved
          4. Only start a new `{Constants.NDEPEND_MCP_SERVER}` instance if switching to a different solution file
          
          **Result**
          
          - If initialization succeeds, the tool returns an empty string array (`[]`).  
            → This means the workspace is ready and no further action is required.
          
          - If initialization fails, the tool returns a string array containing the file paths of the Most Recently Used Visual Studio and VS Code solution files.
          
            In this case:
            1. ***YOU MUST DISPLAY THE RETURNED SOLUTIONS AS A NUMBERED LIST***.
            2. Ask the user to select the solution to analyze.
            3. Call this tool again, passing the selected solution file path as a parameter.
          """
        )]
    public static async Task<string[]> InitializeFromSolutionTool(
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
            C:\Users\YourName\source\repos\MySolution\MySolution.slnx
            MySolution.slnx
            MySolution
            """)]
          // TODO -> Investigate opened VS solution instances to find the solution file path and name if the provided workspace parameter is not a valid solution file path or name.
          //         https://chatgpt.com/share/698eefe3-29c0-800d-a5d4-0cde6780bfa0
          string solutionFilePath,

          [Description("A cancellation token for interrupting and canceling the operation.")]
          CancellationToken cancellationToken
        ) {

        logger.LogInformation(
            $"""
            {LogHelpers.TOOL_LOG_SEPARATOR}
            Invoking { Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME} with argument: workspace `{ solutionFilePath}`.
            """);

        // Report analysis progress
        Action<int>? reportProgressProc = McpProgressHelpers.GetReportProgressProc(
            server, 
            context, 
            cancellationToken);

        return await Task.Run(() => {

            //
            // Check solution file path argument
            //
            if(!SolutionHelpers.TryGetValidSolutionFilePath(
                   solutionFilePath, 
                   logger, 
                   out IAbsoluteFilePath solutionFilePathTyped,
                   out List<IAbsoluteFilePath> mruSlnFilePaths
                   )) {
                return mruSlnFilePaths.Select(p => p.ToString()!).ToArray();
            }

            // Try get the .ndproj attached to the solution
            bool isSlnxExt = solutionFilePathTyped.HasExtension(".slnx");
            if (   ( isSlnxExt && ProjectPathHelpers.TryGetProjectFromSlnx(solutionFilePathTyped, out IAbsoluteFilePath? projectFilePath))
                || (!isSlnxExt && ProjectPathHelpers.TryGetProjectFromSln(solutionFilePathTyped, out projectFilePath))) {
                logger.LogInformation($"Found the NDepend project `{projectFilePath!.ToString()}` attached to the solution.");
                goto INIT_FROM_PROJECT_FILE;
            } 
            logger.LogInformation($"Cannot get an NDepend project attached to the solution `{solutionFilePathTyped.ToString()}`.");

            // Try get the .ndproj side by side with the solution file
            if (ProjectPathHelpers.TryGetSideBySideProjectFile(solutionFilePathTyped, out projectFilePath)) {
                logger.LogInformation($"Found the NDepend project `{projectFilePath.ToString()}` side-by-side with the solution.");
                goto INIT_FROM_PROJECT_FILE;
            }

            // If no side-byte-side project create it and run analysis
            logger.LogInformation(
                $"""
                 No NDepend project side-by-side with the solution `{solutionFilePathTyped.ToString()}` found.
                 Create the project `{ ProjectPathHelpers.GetSideBySideProjectFile(solutionFilePathTyped).ToString()}` and run analysis.
                 """);
            var project = ProjectHelpers.CreateNDependProjectSideBySideWithTheSolution(
                logger, 
                solutionFilePathTyped);

            var analysisResult = project.RunAnalysisWithLog(
                logger, 
                service.StopWatchingForNewAnalysisResult,
                reportProgressProc);

            if(!service.InitializeFromAnalysisResult(analysisResult, logger)) {
                logger.LogErrorAndThrow($"Failed to initialize the MCP server from the analysis result `{analysisResult.AnalysisResultRef.AnalysisResultFilePath.ToString()}` of the project `{project.Properties.FilePath.ToString()}`.");
            }


         INIT_FROM_PROJECT_FILE:
            if(!service.InitializeFromProject(projectFilePath!.ToString()!, logger, reportProgressProc)) {
                logger.LogErrorAndThrow($"Failed to initialize the MCP server from the project `{projectFilePath.ToString()}`.");
            }
            return [];
        }, cancellationToken);
    }


    
}



