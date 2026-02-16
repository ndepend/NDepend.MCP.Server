
using ModelContextProtocol;
using NDepend.Helpers;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;


namespace NDepend.Mcp.Tools.CodeQuery;

[McpServerToolType]
public static partial class CodeQueryTools {
    
    internal const string TOOL_CODE_QUERY_NAME = Constants.TOOL_NAME_PREFIX + "gen-code-query-and-rule";
    
    [McpServerTool(Name = TOOL_CODE_QUERY_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  # PURPOSE:
                  
                  **ALWAYS** use this tool whenever the user requests an NDepend code query or a code rule to analyze their codebase.
                  It provides complete documentation, NDepend model APIs and guidance to generate accurate and effective queries.
                  
                  **OPTIONAL**: if the user ask something complex about its code base that cannot be handled by others NDepend tools, you MUST this tool to generate a custom code queries.
                  In this situation you MUST verify that the query compile with the tool `{TOOL_COMPILE_QUERY}` that can return compile error if any.
                  Finally you MUST call the tool `{TOOL_RUN_QUERY}` to run the generated code query and show result to the users.
                  
                  # WORKFLOW:
                  
                  ## STEP 1: DETERMINE QUERY KIND
                  
                  Determine if the user wants a `{CodeQueryKind.CODE_QUERY_LIST}`, a `{CodeQueryKind.CODE_RULE}`, a `{CodeQueryKind.QUALITY_GATE}`, a `{CodeQueryKind.QUERYING_ISSUE_AND_RULE}`, a `{CodeQueryKind.TREND_METRIC}` or a `{CodeQueryKind.CODE_QUERY_SCALAR}` ?
                    
                  When identifying `{CodeQueryKind.QUERYING_ISSUE_AND_RULE}` avoid any 'orderby' clause that could prevent the generated query to run.
                    
                  ## STEP 2: IDENTIFY FEATURES THAT THE GENERATED QUERY WILL USE
                  
                  Analyze what aspects of code to query and deduce ALL applicable features.
                  **IMPORTANT**
                  - Don’t hesitate to include multiple features if the query needs to analyze different parts of the code.
                  - When in doubt, include the feature anyway. It’s better to over-specify than to miss an aspect.
                  - It is common to request 5 features or more.
                  - If a call fails, it’s likely because you provided incorrect feature inputs. In this case, review the list of features and add any that are missing.
                  - Correct features are: 
                  `{CodeQueryFeature.ESSENTIAL}`, `{CodeQueryFeature.LINE_OF_CODE}`, `{CodeQueryFeature.MAINTAINABILITY}`, `{CodeQueryFeature.COMPLEXITY}`, `{CodeQueryFeature.COVERAGE}`, `{CodeQueryFeature.COMMENT}`, `{CodeQueryFeature.USAGE_DEPENDENCY}`, `{CodeQueryFeature.PARENT_CHILDREN_RELATIONSHIP}`, `{CodeQueryFeature.INHERITANCE_AND_BASE_CLASS}`, `{CodeQueryFeature.INTERFACE}`, `{CodeQueryFeature.SOLID_PRINCIPLES}`, `{CodeQueryFeature.CLEAN_ARCHITECTURE}`, `{CodeQueryFeature.ENCAPSULATION_AND_VISIBILITY}`, `{CodeQueryFeature.STATE_MUTABILITY}`, `{CodeQueryFeature.DIFF_SINCE_BASELINE}`, `{CodeQueryFeature.NAMING}`, `{CodeQueryFeature.ATTRIBUTE}`, `{CodeQueryFeature.SOURCE_FILE_DECLARATION}`, `{CodeQueryFeature.EVENT_PATTERN}`, `{CodeQueryFeature.CONSTRUCTOR_INSTANTIATION}`
                  
                  ## STEP 3: SUBSEQUENT CALLS
                  
                  You can call this tool multiple times to get additional feature prompts as needed while building the code query.
                  
                  ## STEP 4: VERIFY GENERATED QUERY BY COMPILING IT
                  
                  **ALWAYS** invoke `{TOOL_COMPILE_QUERY}` immediately after generating a query or rule to ensure it compiles before executing it or presenting it to the user.
                  If compilation errors are reported:
                  - Analyze the errors carefully.
                  - Regenerate a corrected version of the query.
                  - Repeat the compilation check as needed.
                  - You may call this tool multiple times and request additional feature prompts to resolve issues.

                  ## STEP 5: SHOW THE GENERATED QUERY TO THE USER
                  
                  **MANDATORY**: Once a query or rule is generated **SHOW THE QUERY TO THE USER**.
                                                
                  ## STEP 6: EVENTUALLY RUN THE GENERATED QUERY OR RULE
                  
                  **OPTIONAL**: If the user requests execution, or if you generated a code query to handled a complex request, invoke the tool `{TOOL_RUN_QUERY}` with the generated query or rule.
                  
                  # RESULT:
                  
                  For each feature, this tool will provide a detailed prompt that explains how to construct the corresponding code query or code rule.
                  
                  This will help you generate the requested code query or code rule accurately.
                  
                  The first feature prompt returned will always be `{CodeQueryFeature.ESSENTIAL}` to ensure basic code query structure.
                  
                  The second feature prompt returned will be the code query kind prompt, if applicable.
                  
                  # CODE QUERY FORMATTING:
                  
                  Always format the query as plain text easy to copy paste, no JSON, no XML, no markdown.
                  
                  # EXAMPLE USAGE:
                  
                  Used when the user ask for:
                  "Which methods pair call each other"
                  "Which ctor that register to some events in a non disposable class"
                  "Which base classes that are using one or more derived classes, exhibit unwanted calls"
                  "Generate a code query to check for the SOLID open close principle and run it"
                  "Generate a code rule to enforce Clean Architecture and execute it"
                  "Gen a code query to match classes mutually dependent, run it and export the result to a graph"
                  "Generate a code query that identifies methods that are both poorly maintainable and insufficiently tested."
                  "Generate a query to identify methods that are too long, insufficiently commented, overly complex, or otherwise problematic."
                  "Write an NDepend code rule that enforces type names to start with an uppercase letter."
                  "Write a CQLinq query that matches all public API elements in the Domain assemblies."
                  "Write a query to select methods whose source files are under the 'Customer' directory."
                  "Generate a query to match less maintainable new method created baseline"
                  "Generate a code query that lists all code elements decorated with one or more attributes, including the parameter values applied to each attribute instance."
                  "Generate a code query to list types with poor encapsulation"
                  "Write a query to identify which types are immutable, partially immutable, or fully mutable to understand design patterns. For each type match the various fields."
                  "Generate a query to detect fat interfaces and list types using only a small portion of it"
                  "Generate a query to list pair of methods calling each others"
                  "Generate an NDepend trend metric that measures the amount of code coverage on JustMyCode only"
                  "Generate a Quality Gate to prevent any new issue since the baseline in any namespace related to 'Domain'"
                  "Write a code rule that matches all nested types publicly visible"
                  "Generate a code query to match disposable type that register to events in ctor but don't deregister in the Dispose() method"
                  "Generate a query to match issues related to code smells and run it"
                  """
                 )]
    public static async Task<CodeQueryFeaturePromptInfo[]> GenerateCodeQueryTool(
                INDependService service,
                ILogger<CodeQueryToolsLog> logger,


                [Description(
                    $"""
                     Specifies the kind of code query to generate.
                     
                     A corresponding prompt will explain how to generate the requested kind of code query.
                     
                     The kind may be one of the following:
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
                      This parameter is a list of features that the requested code query may target.
                      For each listed feature, the tool returns a detailed prompt explaining how to use that feature when generating the query.
                      
                      Select ALL applicable features in relation with the query asked.
                      **IMPORTANT**
                      - Don’t hesitate to include multiple features if the query needs to analyze different parts of the code.
                      - When in doubt, include the feature anyway. It’s better to over-specify than to miss an aspect.
                      - It is common to request 5 features or more.
                      
                      **YOU MUST** Only pass feature strings that exactly match these identifiers below. 
                      **DO NOT** pass any other strings or variations.
                      
                      Available features (`identifier` :`explanation`) pass these exact `identifier` strings:
                      
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

                [Description("A cancellation token for interrupting and canceling the operation.")]
                CancellationToken cancellationToken) {


        logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             Invoking {TOOL_CODE_QUERY_NAME} with arguments: 
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




