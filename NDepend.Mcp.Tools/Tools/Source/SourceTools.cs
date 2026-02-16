using NDepend.CodeModel;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;


namespace NDepend.Mcp.Tools.Source;



[McpServerToolType]
public static class SourceTools {

    internal const string TOOL_SOURCE_NAME = Constants.TOOL_NAME_PREFIX + "get-sources";
    internal const string TOOL_DIFF_SOURCE_NAME = Constants.TOOL_NAME_PREFIX + "diff-sources";

    

    [McpServerTool(Name = TOOL_SOURCE_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  # Source Code Retrieval
                  
                  Get source code as raw text from NDepend
                  Both retrieving current and baseline source files are supported
                  
                  # Purpose and Use-Cases
                  
                  Use this tool when the user asks questions like:
                  
                  **Viewing Code:**
                  - "Show me the code for UserService.Authenticate"
                  - "Let me see the implementation"
                  - "Display the source code"
                  - "What does this method look like?"
                  - "Show me the actual code"
                  - "Can I see the implementation of this class?"
                  
                  **Understanding Implementation:**
                  - "How is this method implemented?"
                  - "What does this function do?"
                  - "Walk me through this code"
                  - "Explain how this works"
                  - "What's the logic in this method?"
                  - "How does the authentication work?"
                  
                  **Code Review:**
                  - "Review the code for this method"
                  - "Show me what changed"
                  - "Let's look at the implementation"
                  - "Can you review this code?"
                  - "What does the new code look like?"
                  - "Show me the code I need to review"
                  
                  **Debugging & Troubleshooting:**
                  - "Show me where the error occurs"
                  - "Let me see the code that's failing"
                  - "Display the problematic method"
                  - "What's in this function that could cause the bug?"
                  - "Show me the code around line 45"
                  
                  **Learning & Examples:**
                  - "Show me an example of dependency injection"
                  - "How do you implement this pattern?"
                  - "Can I see a sample implementation?"
                  - "Show me how to use this API"
                  - "Give me an example of this class"
                  
                  **Refactoring Analysis:**
                  - "Show me the code before I refactor it"
                  - "Let me see what needs to be improved"
                  - "Display the method we discussed"
                  - "Show me the complex code that needs simplification"
                  
                  **Following Up on Other NDepend MCP Tools:**
                  - After metrics: "Show me the code with low maintainability"
                  - After dependencies: "Let me see what dependency #3 does"
                  - After search: "Display the code for result #2"
                  - After quality gates: "Show the code that's failing coverage"
                  """)]
    public static async Task<object> GetSourceTool(
                INDependService service,
                ILogger<SourceToolsLog> logger,

                [Description(
                   $"""
                    Specify whether to search for source files from the current analysis or from the baseline snapshot.
                    Value can be either `{CurrentOrBaselineHelpers.CURRENT}` per default, or `{CurrentOrBaselineHelpers.BASELINE}`.
                    """)]
                string currentOrBaseline,

                [Description(
                    """
                    Specify the source file name with extension like .cs for C# source file.
                    If several source files have the same name, only one will be returned.
                    """)]
                string fileName,

                [Description("A cancellation token for interrupting and canceling the operation.")]
                CancellationToken cancellationToken) {


        logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             Invoking {TOOL_SOURCE_NAME} with arguments: 
                currentOrBaseline=`{currentOrBaseline}`
                fileName=`{fileName}`
             """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {
            ICodeBase codeBase = session.AnalysisResult.CodeBase;
            if (CurrentOrBaselineHelpers.GetCurrentOrBaselineVal(logger, currentOrBaseline) == CurrentOrBaseline.Baseline) {
                codeBase = session.CompareContext.OlderCodeBase;
            }

            ISourceFile? sourceFile = codeBase.SourceFiles.FirstOrDefault(s => s.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if(sourceFile == null) {
                logger.LogErrorAndThrow($"No source file with name `{fileName}` found.");
            }

            if(!codeBase.TryGetSourceFileContent(sourceFile!.FilePath, out string content, out string failureReason)) {
                logger.LogErrorAndThrow(
                    $"""
                     Cannot fetch the source file content for `{fileName}`.
                     Reason: {failureReason}
                     """);
            }


            return new SourceResult {
                SourceFilePath = sourceFile.FilePath.ToString()!,
                CurrentOrBaseline = currentOrBaseline,
                SourceCode = content
            };
        }, cancellationToken);

    }




    [McpServerTool(Name = TOOL_DIFF_SOURCE_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  # Source Code Diff Action
                  
                  This tool invokes the NDepend diff action on a source file to compare its current content with its baseline content.
                  The source compare tool specified in NDepend options is used to perform the diff.
                  
                  # Purpose and Use-Cases
                  
                  Use this tool when the user asks questions like:
                  
                  **Comparison since the Baseline:**
                  - "Show me both current and baseline implementations"
                  - "Show me both implementations"
                  - "Compare this with the old version"
                  - "Let me see the before and after"
                  - "What's changed in this method?"
                  - "Show me the differences since the baseline"
                  
                  Notice that this tool only works on Windows, not on Linux and MacOS.
                  """)]

    public static async Task<string> DiffSourceTool(
                INDependService service,
                ILogger<RuleToolsLog> logger,

                [Description(
                    """
                    Specify the source file name with extension like .cs for C# source file.
                    If several source files have the same name, only one will be diffed.
                    """)]
                string fileName,

                [Description("A cancellation token for interrupting and canceling the operation.")]
                CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             Invoking {TOOL_DIFF_SOURCE_NAME} with arguments: 
                fileName=`{fileName}`
             """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {
            ICodeBase codeBase = session.AnalysisResult.CodeBase;
            
            ISourceFile? sourceFile = codeBase.SourceFiles.FirstOrDefault(s => s.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (sourceFile == null) {
                logger.LogErrorAndThrow($"No source file with name `{fileName}` found.");
            }

            ICodeElement? type = sourceFile!.CodeElements.FirstOrDefault(c => c.IsType);
            if (type == null) {
                logger.LogErrorAndThrow($"No type declaration found in `{sourceFile.FilePathString}`.");
            }

            if(!type.TryDiffSource(session.CompareContext, out string failureReason)) {
                logger.LogErrorAndThrow(
                    $"""
                     Cannot diff the source file content with its baseline version.
                     File path: `{sourceFile.FilePathString}`.
                     Reason: {failureReason}
                     """);
            }

            return $"""
                    The source file has been diffed properly with its baseline version.
                    File path: `{sourceFile.FilePathString}`.
                    """;
        }, cancellationToken);

    }


}



