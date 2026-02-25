using NDepend.Helpers;
using NDepend.CodeModel;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;
using NDepend.Path;
using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Dependency;

// Marker class for ILogger<T> category specific to DependencyToolsLog

[McpServerToolType]
public static class DependencyTools {


    internal const string TOOL_LIST_DEPENDENCIES_NAME = Constants.TOOL_NAME_PREFIX + "list-dependencies";

    const string MAX_PAGE_SIZE = "100";

    // , use ndepend
    [McpServerTool(Name = TOOL_LIST_DEPENDENCIES_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  # Dependencies Collection
                  
                  Collect and show dependencies for target code elements.
                  By default, generates an SVG HTML graph and opens it in the browser.
                  
                  ## Use-Cases
                  
                  Use this tool to answer questions about code dependencies and impact.
                  
                  ### Callers
                  - Which code calls this method or class?  
                  - Show call graphs or usage across namespaces.
                  
                  ### Callees
                  - What does this method call?  
                  - Show dependencies of a class or controller.
                  
                  ### Impact Analysis
                  - What breaks if I change this method or class?  
                  - Show blast radius of a change.
                  
                  ### Call Chains
                  - Show full call chain or execution path.  
                  - Trace requests, indirect callers, or dependency trees.
                  
                  ### Refactoring
                  - What uses this code before refactoring?  
                  - Can I safely delete or modify it?
                  
                  ### Architecture
                  - Detect cross-layer or external dependencies.  
                  - Identify circular dependencies.
                  
                  ### Code Review
                  - Review new or changed code dependencies.  
                  - Identify newly introduced dependencies.
                  
                  ## Response Formatting
                  
                  ### 1. Clickable Locations (MANDATORY)
                  Always show: **Full qualified name** `(file.ext:line)` for each dependent and target code element displayed
                  Example: `method ClientLog() (UserService.cs:45)`
                  
                  ### 2. Number All Dependencies (1-based) (MANDATORY)
                  Format: `1. `, `2. `, `3. `
                  Enables: `Show me more about dependency #5`
                  
                  ### 3. Group by Target
                  
                  Organize results per target code element.
                  
                  ### 4. Separate BCL
                  
                  - BCL = System.* & Microsoft.* (no source info)  
                  - Show in a distinct subsection labeled ".NET Framework Dependencies"  
                  - List only full qualified names; keep app code in main section
                  
                  ### 5. Dependency Count
                  
                  State total dependencies per target (e.g., "Found 15 dependencies for `UserService.Authenticate`").
                  
                  ### 6. Dependency Direction
                  Label each dependency:  
                  - `→ Calls` (outgoing)  
                  - `← Called by` (incoming)  
                  - `⇄ Entangled` (both)
                  
                  ### 7. Indirect Depth
                  
                  Show depth for indirect dependencies:  
                  - Direct = omit depth  
                  - Indirect = `[depth 2]`, `[depth 3]`, etc.
                  """)]
    public static async Task<ListDependenciesPaginatedResult> ListDependenciesTool(
                INDependService service,
                ILogger<DependencyToolsLog> logger,

                [Description(PaginatedResult.PAGINATION_CURSOR_DESC)]
                int cursor,

                [Description($"Max number of dependencies per page (≤ {MAX_PAGE_SIZE}) to avoid LLM prompt overflow.")]
                int pageSize,

                [Description(
                    $"""
                    Search for dependencies {CodeElementApplyFilter.FROM_CURRENT_OR_BASELINE_ENUM}.
                    """)]
                string currentOrBaseline,

                [Description(
                    $"""
                    Filter code elements dependency target by change status: {CodeElementApplyFilter.CHANGE_STATUS_ENUM}
                    """)]
                string? filterCodeElementChangeStatus,

                [Description(
                    $"""
                     A list of code elements kinds to search for.
                     {CodeElementApplyFilter.FILTER_ELEM_KIND_ENUM}
                     """)]
                string[] filterCodeElementKinds,

                [Description(CodeElementApplyFilter.FILTER_FILE_NAME_DESC)]
                string? filterFileName,

                [Description(CodeElementApplyFilter.FILTER_SIMPLE_NAME_DESC)]
                string? filterSimpleNamePattern,

                [Description(CodeElementApplyFilter.FILTER_PARENT_NAME_DESC)]
                string? filterParentNamePattern,

                [Description(CodeElementApplyFilter.FILTER_PARENT_PROJECT_NAME_DESC)]
                string? filterProjectName,

                [Description(
                    $"""
                     List of dependency kinds to retrieve.
                     **Default**: Retrieve direct callers callees and entangled with the value `{DependencyKindHelpers.KIND_ALL_DIRECT}` unless otherwise specified.
                     Possible values are `{DependencyKindHelpers.KIND_ALL_DIRECT}`,`{DependencyKindHelpers.KIND_DIRECT_CALLER}`, `{DependencyKindHelpers.KIND_DIRECT_CALLEE}`, `{DependencyKindHelpers.KIND_ENTANGLED}`, `{DependencyKindHelpers.KIND_INDIRECT_CALLER}`, `{DependencyKindHelpers.KIND_INDIRECT_CALLEE}`, `{DependencyKindHelpers.KIND_INDIRECT_ENTANGLED}`, `{DependencyKindHelpers.KIND_ALL}`.
                     """)]
                string[] dependencyKindsStrings,


                [Description(
                    """
                     Set to `True` by default to export the graph, unless the user specify otherwise.
                     """)]
                bool exportGraph,

                CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
            {LogHelpers.TOOL_LOG_SEPARATOR}
            Invoking {TOOL_LIST_DEPENDENCIES_NAME} with arguments: 
               -cursor= `{cursor}`
               -pageSize= `{pageSize}`
               -currentOrBaseline=`{currentOrBaseline}`
               
               -filterCodeElementChangeStatus= `{filterCodeElementChangeStatus ?? "<default>"}`
               -filterCodeElementKinds: `{filterCodeElementKinds.Aggregate("', '")}`
               -filterFileName: `{filterFileName ?? "<any>"}`
               -filterSimpleNamePattern: `{filterSimpleNamePattern ?? "<any>"}`
               -filterParentNamePattern: `{filterParentNamePattern ?? "<any>"}`
               -filterProjectName: `{filterProjectName ?? "<any>"}`
               
               -dependencyKindsStrings: `{dependencyKindsStrings.Aggregate("', '")}`
            """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {

            // Apply currentOrBaseline
            ICodeBaseView codeBaseView = session.GetApplicationCurrentOrBaseline(logger, currentOrBaseline, out CurrentOrBaseline currentOrBaselineVal);

            // Fill codeElements
            List<ICodeElement> codeElementsTarget = [];
            var codeElementKinds = CodeElementKindHelpers.GetKindOfCodeElementVal(logger, filterCodeElementKinds);
            codeElementsTarget.AppendElementsOfKinds(codeElementKinds, codeBaseView);

            // Apply other filters
            codeElementsTarget.FilterByChangeStatus(logger, filterCodeElementChangeStatus, currentOrBaselineVal, session.CompareContext);
            codeElementsTarget.FilterByFileName(filterFileName);
            codeElementsTarget.FilterBySimpleNamePattern(filterSimpleNamePattern);
            codeElementsTarget.FilterByParentNamePattern(filterParentNamePattern);
            codeElementsTarget.FilterByProjectName(filterProjectName);

            // Obtain dependencies
            DependencyKind dependencyKinds = DependencyKindHelpers.GetDependencyKinds(logger, dependencyKindsStrings);
            var dependencies = new List<DependencyInfo>();
            foreach (var codeElementTarget in codeElementsTarget) {
                FillDependencies.Go(dependencies, codeElementTarget, dependencyKinds);
            }

            // exportGraph
            if (exportGraph) { ExportGraph(logger, dependencyKinds, codeElementsTarget); }

            var paginatedResult = PaginatedResult.Build(
                logger,
                dependencies,
                cursor,
                pageSize,
                MAX_PAGE_SIZE,
                out var paginatedElemsInfo);
            return new ListDependenciesPaginatedResult(paginatedElemsInfo, paginatedResult);
        }, cancellationToken);

    }

    private static void ExportGraph(
                ILogger<DependencyToolsLog> logger,
                DependencyKind dependencyKinds,
                List<ICodeElement> codeElementsTarget) {
        int count = codeElementsTarget.Count;
        if (count == 0) { return; }

        string fileName = count == 1 ?
            codeElementsTarget[0].SimpleName :
            $"{count.ToString()}_elements_";

        // Remove forbidden chars in fileName (like '<' '>') to avoid export failure due to invalid file name
        fileName = new string(
            fileName
                .Where(c => !PathHelpers.ForbiddenCharInPath.Contains(c))
                .ToArray());

        var tempHtmlPath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"{fileName}.{System.IO.Path.GetRandomFileName()}.html"
        ).ToAbsoluteFilePath();
        
        if (count == 1) {
            // single codeElementsTarget => call TryExportSvgGraphFocusOnNode()
            if (codeElementsTarget[0].TryExportSvgGraphFocusOnNode(
                    tempHtmlPath,
                    ExportGraphSettings.Default,
                    out string failureReason1)) {
                BrowserHelpers.OpenHtmlDiagram(logger, tempHtmlPath);
            } else {
                logger.LogCannotExportGraphError(tempHtmlPath, failureReason1);
            }
            return;
        }

        // several codeElementsTarget => call TryExportSvgGraphCallersCallees()
        bool caller = dependencyKinds.HasFlag(DependencyKind.DirectCaller) || dependencyKinds.HasFlag(DependencyKind.IndirectCaller);
        bool callee = dependencyKinds.HasFlag(DependencyKind.DirectCallee) || dependencyKinds.HasFlag(DependencyKind.IndirectCallee);
        DependencyDirection dd =
                !caller ? DependencyDirection.Callees :
                !callee ? DependencyDirection.Callers :
                          DependencyDirection.Both;
        uint callDepth =
            dependencyKinds.HasFlag(DependencyKind.IndirectCaller) ||
            dependencyKinds.HasFlag(DependencyKind.IndirectCallee) ||
            dependencyKinds.HasFlag(DependencyKind.IndirectEntangled) ?
                                    uint.MaxValue : 1;

        // Take only first 7 code elements to avoid too complex code query
        // and also because of the 16 columns max in query result
        // hence if 2 columns per code element (Callers and Callees) + the matched code element, we have max 1+(7x2) columns
        // (both CallersCallees and FocusOnNode nodes set are obtained from a generated code query)
        if (codeElementsTarget.Take(7).TryExportSvgGraphCallersCallees(
                dd, callDepth,
                tempHtmlPath,
                ExportGraphSettings.Default,
                out string failureReason2)) {
            BrowserHelpers.OpenHtmlDiagram(logger, tempHtmlPath);
        } else {
            logger.LogCannotExportGraphError(tempHtmlPath, failureReason2);
        }

    }


}
