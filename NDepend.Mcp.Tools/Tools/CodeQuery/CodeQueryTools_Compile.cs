using NDepend.CodeQuery;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.CodeQuery; 
public static partial class CodeQueryTools {
    internal const string TOOL_COMPILE_QUERY = 
        Constants.TOOL_NAME_PREFIX + "compile-code-query-or-rule";

    [McpServerTool(
        Name = TOOL_COMPILE_QUERY, 
        Idempotent = false, 
        ReadOnly = true, 
        Destructive = false, 
        OpenWorld = false),
     Description($"""
        {Constants.PROMPT_CALL_INITIALIZE}
               
        PURPOSE:
               
        Validates code queries/rules for syntax errors and compilation issues. 
               
        ALWAYS call this after generating code with the tool `{TOOL_CODE_QUERY_NAME}` to verify validity before running it or before showing it to the user. 
               
        Returns error list with descriptions and positions if compilation fails, or empty list if successful.
        """)]
    public static async Task<QueryCompilationError[]> CompileCodeQueryOrRule(
                INDependService service,
                ILogger<CodeQueryToolsLog> logger,

                [Description("The code query or code rule as a string, to compile and verify its correctness.")]
                string codeQueryOrRule,

                CancellationToken cancellationToken) {
        logger.LogInformation(
        $"""
         {LogHelpers.TOOL_LOG_SEPARATOR}
         Invoking {TOOL_COMPILE_QUERY} with arguments: 
            codeQueryOrRule: `{codeQueryOrRule}`
         """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {

            IQueryCompiled queryCompiled = codeQueryOrRule.Compile(session.CompareContext);

            if (!queryCompiled.HasErrors) {
                return Array.Empty<QueryCompilationError>();
            }

            var queryCompiledError = queryCompiled.QueryCompiledError;
            var errors = new List<QueryCompilationError>();
            foreach (IQueryCompilationError error in queryCompiledError.Errors) {
                errors.Add(new QueryCompilationError() {
                    Description = error.Description,
                    SubStringStartPos = error.SubStringStartPos,
                    SubStringLength = error.SubStringLength
                });
            }
            return errors.ToArray();
        }, cancellationToken);
    }
}
