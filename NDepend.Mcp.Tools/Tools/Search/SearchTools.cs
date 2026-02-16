using NDepend.Helpers;
using NDepend.CodeModel;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;
using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Search;


[McpServerToolType]
public static class SearchTools {

    internal const string TOOL_SEARCH_CODE_ELEMENTS_NAME = Constants.TOOL_NAME_PREFIX + "search-code-elements";

    [McpServerTool(Name = TOOL_SEARCH_CODE_ELEMENTS_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  # Code Search
                  
                  Search and discover code elements across your codebase using flexible criteria including name, kind, source file, or change status since a baseline.
                  
                  # Purpose and Use-Cases
                  
                  Use this tool when the user is asking questions like:
                  
                  **Discovery & Navigation:**
                  - "Which classes are related to authentication?"
                  - "What methods exist in the UserController?"
                  
                  **Pattern & Convention:**
                  - "Which classes follow the *Manager pattern?"
                  - "Which methods are named Handle*?"
                  
                  **File-Based:**
                  - "What's in the UserService.cs file?"
                  - "List everything in src/services/*"
                  
                  **Change Tracking:**
                  - "Which methods were added since the baseline?"
                  - "Show me all modified classes"
                  
                  ## Response Structure
                  
                  ### 1. Enable Single-Click Navigation
                  
                  Every code element MUST include:
                  - **Full qualified name** (e.g., `UserService.Authenticate`, `CustomerRepository`)
                  - **Clickable source location**: `ElementName (file.ext:line)`
                  
                  ### 2. Indexed List
                  Number all elements with 1-based indexing for easy reference.
                  - Enables user follow-up: "Show me more about method #5"
                  """)]
    public static async Task<SearchPaginatedResult> SearchCodeElementsTool( 
                INDependService service,
                ILogger<SearchToolsLog> logger,

                [Description("An opaque token representing the pagination position after the last returned result. Set to null to start from the beginning.")]
                string? cursor,

                [Description("Maximum number of code elements to include per page. Must not exceed 500 to prevent LLM prompt overflow.")]
                int pageSize,

                [Description(
                    $"""
                      Specify whether to search for code elements from the current analysis or from the baseline snapshot.
                      Value can be either `{CurrentOrBaselineHelpers.CURRENT}` per default, or `{CurrentOrBaselineHelpers.BASELINE}`.
                      """)]
                string currentOrBaseline,

                [Description(
                    $"""
                     Filters code elements by change status.
                     Valid values are `{CodeChangeStatusSinceBaselineHelpers.STATUS_DEFAULT}`, `{CodeChangeStatusSinceBaselineHelpers.STATUS_NEW}`, `{CodeChangeStatusSinceBaselineHelpers.STATUS_MODIFIED}`, `{CodeChangeStatusSinceBaselineHelpers.STATUS_UNCHANGED}`,`{CodeChangeStatusSinceBaselineHelpers.STATUS_REMOVED}`.
                     A null value means `{CodeChangeStatusSinceBaselineHelpers.STATUS_DEFAULT}`.
                     `{CodeChangeStatusSinceBaselineHelpers.STATUS_REMOVED}` can only work if the parameter currentOrBaseline is set to `{CurrentOrBaselineHelpers.BASELINE}`.
                     """)]
                string? filterCodeElementChangeStatus,

                [Description(
                    $"""
                     A list of code elements kinds to search for.
                     Possible values are `{CodeElementKindHelpers.KIND_ALL}`, '${CodeElementKindHelpers.KIND_ASSEMBLY}`, `{CodeElementKindHelpers.KIND_NAMESPACE}`, `{CodeElementKindHelpers.KIND_TYPE}` and `{CodeElementKindHelpers.KIND_METHOD}`.
                     """)]
                string[] filterCodeElementKinds,

                [Description(
                    """
                     Specify the source file name that contains the code elements to search for, including its extension (e.g., .cs for C# files).
                     If multiple source files share the same name, all of them will be included.
                     Set this value to null to search for code elements across all source files.
                     """)]
                string? filterFileName,

                [Description(
                    $"""
                     A substring used to filter by code element simple name.
                     {CodeElementKindHelpers.SIMPLE_NAME_EXPLANATION}
                     """)]
                string? filterSimpleNamePattern,

                [Description(
                    $"""
                     A substring used to filter by the parent’s name of the target code element.
                     {CodeElementKindHelpers.PARENT_NAME_EXPLANATION}
                     """)]
                string? filterParentNamePattern,

                [Description("Filter by parent project name. ")]
                string? filterProjectName,

                [Description("A cancellation token for interrupting and canceling the operation.")]
                CancellationToken cancellationToken) {

        logger.LogInformation(
           $"""
            {LogHelpers.TOOL_LOG_SEPARATOR}
            Invoking {TOOL_SEARCH_CODE_ELEMENTS_NAME} with arguments: 
               cursor=`{cursor ?? "0"}`
               pageSize=`{pageSize}`
               currentOrBaseline=`{currentOrBaseline}`
               filterCodeElementChangeStatus=`{filterCodeElementChangeStatus ?? "<default>"}`
               filterCodeElementKinds: `{filterCodeElementKinds.Aggregate("', '")}`
               filterFileName: `{filterFileName ?? "<any>"}`
               filterSimpleNamePattern: `{filterSimpleNamePattern ?? "<any>"}`
               filterParentNamePattern: `{filterParentNamePattern ?? "<any>"}`
               filterProjectName: `{filterProjectName ?? "<any>"}`
            """);

        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {

            // Apply currentOrBaseline
            ICodeBaseView codeBase = session.GetApplicationCurrentOrBaseline(logger, currentOrBaseline, out CurrentOrBaseline currentOrBaselineVal);

            // Fill codeElements
            List<ICodeElement> codeElements = [];
            var kinds = CodeElementKindHelpers.GetKindOfCodeElementVal(logger, filterCodeElementKinds);
            codeElements.AppendElementsOfKinds(kinds, codeBase);
            
            // Apply other filters
            codeElements.FilterByChangeStatus(logger, filterCodeElementChangeStatus, currentOrBaselineVal, session.CompareContext);
            codeElements.FilterByFileName(filterFileName);
            codeElements.FilterBySimpleNamePattern(filterSimpleNamePattern);
            codeElements.FilterByParentNamePattern(filterParentNamePattern);
            codeElements.FilterByProjectName(filterProjectName);

            var elemInfo = codeElements.Select(codeElement => new CodeElementInfo(codeElement,filterFileName)).ToList();
            var paginatedResult = PaginatedResult.Build(logger, elemInfo, cursor, pageSize, out var paginatedElemsInfo);
            return new SearchPaginatedResult(paginatedElemsInfo, paginatedResult);

        }, cancellationToken);

    }


}


