
using ModelContextProtocol;
using NDepend.Helpers;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;


namespace NDepend.Mcp.Tools.CodeQuery;

[McpServerToolType]
public static partial class CodeQueryTools {
    
    internal const string TOOL_GEN_QUERY_NAME = Constants.TOOL_NAME_PREFIX + "gen-code-query-and-rule";
    
    [McpServerTool(Name = TOOL_GEN_QUERY_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                   {Constants.PROMPT_CALL_INITIALIZE}
                   
                   # Generating Code Query or Rule
                   
                   **REQUIRED:** You MUST use this tool anytime when user requests to generate or write an NDepend code queries/rules.
                   This tool then provides you with documentation, APIs, and guidance for accurate query generation.
                   
                   **OPTIONAL**: For complex codebase requests not handled by other NDepend tools call this tool to generate custom query.
                   
                   ## Workflow
                   
                   1. **Determine the kind** of query to generate
                     
                   2. **Identify ALL features** the generated query needs
                   
                   3. **Multiple calls**: Call this tool multiple times as needed while building queries if more features are required
                   
                   4. **Verify generated query**: **ALWAYS** compile generated query with the tool `{TOOL_RUN_QUERY_NAME}` with `compileOnly` set to `True`. Fix compilation errors if any and regenerate until successful.
                   
                   5. **Show query**: **ALWAYS** Present query as code to user
                   
                   6. **Execute**: If requested, run with {TOOL_RUN_QUERY_NAME}
                   
                   ## Result
                   
                   Returns detailed prompts for query kind and each feature explaining query construction.
                   
                   ## Example Usage
                   "Methods that call each other"
                   "Generate a rule to enforce SOLID principles"
                   "Fin Clean Architecture violations"
                   "Poorly maintainable and insufficiently tested methods"
                   "Types with poor encapsulation"
                   "Immutable vs mutable types"
                   "Disposable types not deregistering events"
                   """
                 )]
    public static async Task<CodeQueryFeaturePromptInfo[]> GenerateCodeQueryTool(
                INDependService service,
                ILogger<CodeQueryToolsLog> logger,


                [Description(CodeQueryKind.QUERY_KIND_PARAM_DESC)]
                string codeQueryKind,

                [Description(
                    $"""
                     Features the query targets. Tool returns generation prompt for each.
                     
                     {CodeQueryFeature.QUERY_FEATURE_PARAM_DESC}
                     """)]
                string[] features,

                CancellationToken cancellationToken) {


        logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             Invoking {TOOL_GEN_QUERY_NAME} with arguments: 
                codeQueryKind: `{codeQueryKind}`
                features: `{features.Aggregate("', '")}`
             """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {
            
            var prompts = new Dictionary<string, CodeQueryFeaturePromptInfo> {
                // Always include ESSENTIAL prompt, even if not specified in parameters
                {CodeQueryFeature.ESSENTIAL, 
                 new CodeQueryFeaturePromptInfo(CodeQueryFeature.ESSENTIAL, CodeQueryFeature.ESSENTIAL_PROMPT)}
            };

            // Eventually include code-query-kind prompt
            if (CodeQueryKind.TryGetKindPrompt(codeQueryKind, out string kindPrompt)) {
                prompts.Add(codeQueryKind, new CodeQueryFeaturePromptInfo(codeQueryKind, kindPrompt));
            }

            // Append feature prompts
            foreach (var feature in features) {
                if(prompts.ContainsKey(feature)) { continue; }
                if(!CodeQueryFeature.TryGetFeaturePrompt(feature, out string prompt)) {
                    throw new McpException($"Unknown code query feature: {feature}");
                }
                prompts.Add(feature, new CodeQueryFeaturePromptInfo(feature, prompt));
            }
            return prompts.Values.ToArray();
        }, cancellationToken);

    }


}




