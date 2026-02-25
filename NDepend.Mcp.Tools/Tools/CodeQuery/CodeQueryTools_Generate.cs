
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
                   
                   Returns detailed prompts for query kind and each feature explaining query construction. First prompt is always `{CodeQueryFeature.ESSENTIAL}`
                   
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


                [Description(
                    $"""
                     Specifies query kind. Tool returns generation instructions.
                     
                     ONLY use an exact identifier below (`identifier` :`explanation`):
                     `{CodeQueryKind.CODE_QUERY_LIST}` : {CodeQueryKind.CODE_QUERY_LIST_EXPL}
                     `{CodeQueryKind.CODE_RULE}` : {CodeQueryKind.CODE_RULE_EXPL}
                     `{CodeQueryKind.QUALITY_GATE}` : {CodeQueryKind.QUALITY_GATE_EXPL}
                     `{CodeQueryKind.QUERYING_ISSUE_AND_RULE}` : {CodeQueryKind.QUERYING_ISSUE_AND_RULE_EXPL}
                     `{CodeQueryKind.TREND_METRIC}` : {CodeQueryKind.TREND_METRIC_EXPL}
                     `{CodeQueryKind.CODE_QUERY_SCALAR}` : {CodeQueryKind.CODE_QUERY_SCALAR_EXPL}
                     """)]
                string codeQueryKind,

                [Description(
                    $"""
                     Features the query targets. Tool returns generation prompt for each.
                     
                     Select ALL applicable features. Include 5+ if needed - better to over-specify than miss aspects.
                     
                     ONLY use exact identifiers below (`identifier` :`explanation`):
                     `{CodeQueryFeature.ESSENTIAL}` : {CodeQueryFeature.ESSENTIAL_EXPL}
                     
                     `{CodeQueryFeature.LINE_OF_CODE}` : {CodeQueryFeature.LINE_OF_CODE_EXPL}
                     `{CodeQueryFeature.ESSENTIAL}`: {CodeQueryFeature.ESSENTIAL_EXPL}
                     `{CodeQueryFeature.LINE_OF_CODE}`: {CodeQueryFeature.LINE_OF_CODE_EXPL}
                     `{CodeQueryFeature.MAINTAINABILITY}`: {CodeQueryFeature.MAINTAINABILITY_EXPL}
                     `{CodeQueryFeature.COMPLEXITY}`: {CodeQueryFeature.COMPLEXITY_EXPL}
                     `{CodeQueryFeature.COVERAGE}`: {CodeQueryFeature.COVERAGE_EXPL}
                     `{CodeQueryFeature.COMMENT}`: {CodeQueryFeature.COMMENT_EXPL}
                     
                     `{CodeQueryFeature.USAGE_DEPENDENCY}`: {CodeQueryFeature.USAGE_DEPENDENCY_EXPL}
                     `{CodeQueryFeature.PARENT_CHILDREN_RELATIONSHIP}`: {CodeQueryFeature.PARENT_CHILDREN_RELATIONSHIP_EXPL}
                     `{CodeQueryFeature.INHERITANCE_AND_BASE_CLASS}`: {CodeQueryFeature.INHERITANCE_AND_BASE_CLASS_EXPL}
                     `{CodeQueryFeature.INTERFACE}`: {CodeQueryFeature.INTERFACE_EXPL}
                     `{CodeQueryFeature.SOLID_PRINCIPLES}`: {CodeQueryFeature.SOLID_PRINCIPLES_EXPL}
                     `{CodeQueryFeature.CLEAN_ARCHITECTURE}`: {CodeQueryFeature.CLEAN_ARCHITECTURE_EXPL}
                     `{CodeQueryFeature.ENCAPSULATION_AND_VISIBILITY}`: {CodeQueryFeature.ENCAPSULATION_AND_VISIBILITY_EXPL}
                     `{CodeQueryFeature.STATE_MUTABILITY}`: {CodeQueryFeature.STATE_MUTABILITY_EXPL}
                     
                     `{CodeQueryFeature.DIFF_SINCE_BASELINE}`: {CodeQueryFeature.DIFF_SINCE_BASELINE_EXPL}
                     `{CodeQueryFeature.NAMING}`: {CodeQueryFeature.NAMING_EXPL}
                     `{CodeQueryFeature.ATTRIBUTE}`: {CodeQueryFeature.ATTRIBUTE_EXPL}
                     `{CodeQueryFeature.SOURCE_FILE_DECLARATION}`: {CodeQueryFeature.SOURCE_FILE_DECLARATION_EXPL}
                     `{CodeQueryFeature.EVENT_PATTERN}`: {CodeQueryFeature.EVENT_PATTERN_EXPL}
                     `{CodeQueryFeature.CONSTRUCTOR_INSTANTIATION}`: {CodeQueryFeature.CONSTRUCTOR_INSTANTIATION_EXPL}
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
            string kindPrompt = codeQueryKind switch {
                CodeQueryKind.CODE_RULE => CodeQueryKind.CODE_RULE_PROMPT,
                CodeQueryKind.QUALITY_GATE => CodeQueryKind.QUALITY_GATE_PROMPT,
                CodeQueryKind.QUERYING_ISSUE_AND_RULE => CodeQueryKind.QUERYING_ISSUE_AND_RULE_PROMPT,
                CodeQueryKind.TREND_METRIC => CodeQueryKind.TREND_METRIC_PROMPT,
                CodeQueryKind.CODE_QUERY_SCALAR => CodeQueryKind.CODE_QUERY_SCALAR_PROMPT,
                _ => ""
            };
            if(kindPrompt.Length > 0) {
                prompts.Add(codeQueryKind, new CodeQueryFeaturePromptInfo(codeQueryKind, kindPrompt));
            }

            // Append feature prompts
            foreach (var feature in features) {
                if(prompts.ContainsKey(feature)) { continue; }
                string prompt = feature switch {
                    CodeQueryFeature.LINE_OF_CODE => CodeQueryFeature.LINE_OF_CODE_PROMPT,
                    CodeQueryFeature.MAINTAINABILITY => CodeQueryFeature.MAINTAINABILITY_PROMPT,
                    CodeQueryFeature.COMPLEXITY => CodeQueryFeature.COMPLEXITY_PROMPT,
                    CodeQueryFeature.COVERAGE => CodeQueryFeature.COVERAGE_PROMPT,
                    CodeQueryFeature.COMMENT => CodeQueryFeature.COMMENT_PROMPT,
                    CodeQueryFeature.USAGE_DEPENDENCY => CodeQueryFeature.USAGE_DEPENDENCY_PROMPT,
                    CodeQueryFeature.PARENT_CHILDREN_RELATIONSHIP => CodeQueryFeature.PARENT_CHILDREN_RELATIONSHIP_PROMPT,
                    CodeQueryFeature.INHERITANCE_AND_BASE_CLASS => CodeQueryFeature.INHERITANCE_AND_BASE_CLASS_PROMPT,
                    CodeQueryFeature.INTERFACE => CodeQueryFeature.INTERFACE_PROMPT,
                    CodeQueryFeature.SOLID_PRINCIPLES => CodeQueryFeature.SOLID_PRINCIPLES_PROMPT,
                    CodeQueryFeature.CLEAN_ARCHITECTURE => CodeQueryFeature.CLEAN_ARCHITECTURE_PROMPT,
                    CodeQueryFeature.ENCAPSULATION_AND_VISIBILITY => CodeQueryFeature.ENCAPSULATION_AND_VISIBILITY_PROMPT,
                    CodeQueryFeature.STATE_MUTABILITY => CodeQueryFeature.STATE_MUTABILITY_PROMPT,
                    CodeQueryFeature.DIFF_SINCE_BASELINE => CodeQueryFeature.DIFF_SINCE_BASELINE_PROMPT,
                    CodeQueryFeature.NAMING => CodeQueryFeature.NAMING_PROMPT,
                    CodeQueryFeature.ATTRIBUTE => CodeQueryFeature.ATTRIBUTE_PROMPT,
                    CodeQueryFeature.SOURCE_FILE_DECLARATION => CodeQueryFeature.SOURCE_FILE_DECLARATION_PROMPT,
                    CodeQueryFeature.EVENT_PATTERN => CodeQueryFeature.EVENT_PATTERN_PROMPT,
                    CodeQueryFeature.CONSTRUCTOR_INSTANTIATION => CodeQueryFeature.CONSTRUCTOR_INSTANTIATION_PROMPT,
                    _ => throw new McpException($"Unknown code query feature: {feature}")
                };
                prompts.Add(feature, new CodeQueryFeaturePromptInfo(feature, prompt));
            }
            return prompts.Values.ToArray();
        }, cancellationToken);

    }


}




