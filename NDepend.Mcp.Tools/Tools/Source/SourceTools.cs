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
                  
                  Retrieves current or baseline source file content as raw text from NDepend.
                  
                  ## Use-Cases
                  
                  Use this tool when the user wants to:
                  
                  - **View code:** show implementation, method/class source, code around a line  
                  - **Understand logic:** explain what it does or how it works  
                  - **Review changes:** inspect new/updated code since baseline 
                  - **Debug:** show failing or problematic code  
                  - **Learn by example:** sample implementations or patterns  
                  - **Refactor:** view code before improving it  
                  - **Follow-up results:** open code from metrics, dependencies, search, or quality checks
                  """)]
    public static async Task<object> GetSourceTool(
                INDependService service,
                ILogger<SourceToolsLog> logger,

                [Description(
                    $"""
                    Search for source files {CodeElementApplyFilter.FROM_CURRENT_OR_BASELINE_ENUM}.
                    """)]
                string currentOrBaseline,

                [Description(
                        "Specify the source file name with extension (e.g., .cs). " +
                        "If multiple files share the name, only one is returned."
                    )]
                string fileName,

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
                  
                  Invokes NDepend's diff action to compare a source file's current content against its baseline using the configured source compare tool.
                  
                  ## Use-Cases
                  
                  User asks questions like:
                  
                  **Comparison since the Baseline:**
                  
                  - "Show me the before and after"
                  - "compare with the old version"
                  - "What's changed in this method?"
                  - "show differences since baseline"
                  
                  Note: This tool runs on Windows only — not supported on Linux or macOS.
                  """)]

    public static async Task<string> DiffSourceTool(
                INDependService service,
                ILogger<RuleToolsLog> logger,

                [Description(
                    """
                    Provide the filename with extension (e.g. .cs).
                    If multiple files share the same name, only one will be diffed.
                    """)]
                string fileName,

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



