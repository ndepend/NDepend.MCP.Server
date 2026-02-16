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

    // , use ndepend
    [McpServerTool(Name = TOOL_LIST_DEPENDENCIES_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  # Dependencies Collection
                  
                  Collect and present dependencies for one or more target code elements with comprehensive context and navigation support.
                  By default, this tool generates an SVG HTML dependency graph for the user’s request and opens it in the default browser.
                  
                  # Purpose and Use-Cases
                  
                  Use this tool when the user asks questions like:
                  
                  **Understanding Usage (Callers):**
                  - "What dependencies for this class?"
                  - "What calls this method?"
                  - "Which callers for classes in this namespace?"
                  - "Where is UserService.Authenticate used?"
                  - "Generate a call graph for the function XYZ"
                  
                  **Understanding Dependencies (Callees):**
                  - "What does this method call?"
                  - "Show me the dependencies of OrderProcessor"
                  - "Which services does the controller depend on?"
                  
                  **Impact Analysis:**
                  - "If I change this method, what breaks?"
                  - "What would be affected by modifying this class?"
                  - "Show me the blast radius of this change"
                  - "What code depends on this API?"
                  
                  **Call Chain Exploration:**
                  - "Show me the full call chain"
                  - "Show a call graph for this class"
                  - "What's the path from A to B?"
                  - "Trace the execution flow"
                  - "How does the request reach the database?"
                  - "Show indirect callers"
                  - "What's the dependency tree?"
                  
                  **Refactoring Planning:**
                  - "Before I refactor this, what uses it?"
                  - "Can I safely delete this method?"
                  - "Show all coupling to this class"
                  - "What would break if I remove this?"
                  
                  **Architectural Analysis:**
                  - "Does the UI layer call the database directly?"
                  - "Show me cross-layer dependencies"
                  - "What depends on external libraries?"
                  - "Are there circular dependencies?"
                  
                  **Debugging & Troubleshooting:**
                  - "How is this method being invoked?"
                  - "Trace back to the entry point"
                  - "What calls this error handler?"
                  - "Show the call stack for this code"
                  
                  **Code Review:**
                  - "What's affected by this new method?"
                  - "Review dependencies of changed code"
                  - "Show new dependencies introduced"
                  
                  ## Response Structure
                  
                  ### 1. Enable Single-Click Navigation
                  
                  Every dependent and target code element displayed MUST include:
                  - **Full qualified name** (e.g., `UserService.Authenticate`, `CustomerRepository`)
                  - **Clickable source location**: `ElementName (file.ext:line)`
                  
                  ### 2. Group by Target
                  
                  Organize results by target code element, with each target as a distinct section.
                  
                  ### 3. Segregate .NET Base Class Library (BCL) from Application types
                  
                  - BCL elements are those in the namespace System.* and Microsoft.*.
                  - Source location is NOT available for BCL elements
                  - Present BCL dependencies in a **separate subsection** under each target code element
                  - Label clearly as ".NET Framework Dependencies"
                  - Show only the full qualified name.
                  - Keep application code dependencies in the main listing.
                  
                  ### 4. Dependency Count
                  
                  For each target, state the total number of dependencies found.
                  - Example: "Found 15 dependencies for `UserService.authenticate`"
                  
                  ### 5. Dependency Direction Clarity
                  
                  For each dependency, explicitly indicate:
                  - **Incoming (caller)**: Code that calls/uses the target
                  - **Outgoing (callee)**: Code that the target calls/uses
                  - **Both Incoming and Outgoing (caller and callee)**: Code that uses and used by the target
                  - Use clear labels: `→ Calls` or `← Called by` or `⇄ Entagled`
                  
                  ### 6. Depth for Indirect Dependencies
                  
                  When including indirect dependencies, show the depth level:
                  - Direct: depth 1, omit depth
                  - Indirect: depth 2, 3, etc.
                  - Format: `[depth 2]`
                  
                  ### 7. Indexed List
                  
                  Number all dependencies with 1-based indexing for easy reference.
                  - Enables user follow-up: "Show me more about dependency #5"
                  
                  **Default**: Retrieve direct callers and callees unless otherwise specified.
                  """)]
    public static async Task<ListDependenciesPaginatedResult> ListDependenciesTool(
                INDependService service,
                ILogger<DependencyToolsLog> logger,

                [Description(
                     """
                     An opaque token representing the pagination position after the last returned result. Set to null to start from the beginning.
                     """)]
                string? cursor,

                [Description(
                     """
                     Maximum number of dependencies to include per page. Must not exceed 500 to prevent LLM prompt overflow.
                     """)]
                int pageSize,

                [Description(
                     $"""
                      Specify whether to search for dependencies from the current analysis or from the baseline snapshot.
                      Value can be either `{CurrentOrBaselineHelpers.CURRENT}` per default, or `{CurrentOrBaselineHelpers.BASELINE}`.
                      """)]
                string currentOrBaseline,

                [Description(
                    $"""
                     Filters code elements dependency target by change status.
                     Valid values are `{CodeChangeStatusSinceBaselineHelpers.STATUS_DEFAULT}`, `{CodeChangeStatusSinceBaselineHelpers.STATUS_NEW}`, `{CodeChangeStatusSinceBaselineHelpers.STATUS_MODIFIED}`, `{CodeChangeStatusSinceBaselineHelpers.STATUS_UNCHANGED}`,`{CodeChangeStatusSinceBaselineHelpers.STATUS_REMOVED}`.
                     A null value means `{CodeChangeStatusSinceBaselineHelpers.STATUS_DEFAULT}`.
                     `{CodeChangeStatusSinceBaselineHelpers.STATUS_REMOVED}` can only work if the parameter currentOrBaseline is set to `{CurrentOrBaselineHelpers.BASELINE}`.
                     """)]
                string? filterCodeElementChangeStatus,

                [Description(
                    $"""
                     A list of kinds of code elements dependency target to search for.
                     Possible values are `{CodeElementKindHelpers.KIND_ALL}`, '${CodeElementKindHelpers.KIND_ASSEMBLY}`, `{CodeElementKindHelpers.KIND_NAMESPACE}`, `{CodeElementKindHelpers.KIND_TYPE}` and `{CodeElementKindHelpers.KIND_METHOD}`.
                     """)]
                string[] filterCodeElementKinds,

                [Description(
                     """
                     Specify the source file name that contains the code elements dependency target, including its extension (e.g., .cs for C# files).
                     If multiple source files share the same name, all of them will be included.
                     Set this value to null to search for code elements across all source files.
                     """)]
                string? filterFileName,

                [Description(
                    $"""
                     A substring used to filter by code element dependency target simple names.
                     {CodeElementKindHelpers.SIMPLE_NAME_EXPLANATION}
                     """)]
                string? filterSimpleNamePattern,

                [Description(
                    $"""
                     A substring used to filter dependencies by the parent’s name of the target code element.
                     {CodeElementKindHelpers.PARENT_NAME_EXPLANATION}
                     """)]
                string? filterParentNamePattern,

                [Description("Filter by parent project name.")]
                string? filterProjectName,

                [Description(
                    $"""
                     A list of dependency kinds to retrieve.
                     By default, return all three direct dependency kinds, unless the user explicitly requests only incoming or only outgoing dependencies.
                     Only include indirect dependency kinds if the user explicitly requests them; otherwise, restrict the response to direct dependency kinds only.
                     Possible values are `{DependencyKindHelpers.KIND_DIRECT_CALLER}`, `{DependencyKindHelpers.KIND_DIRECT_CALLEE}`, `{DependencyKindHelpers.KIND_DIRECT_CALLER_AND_CALLEE}`, `{DependencyKindHelpers.KIND_INDIRECT_CALLER}`, `{DependencyKindHelpers.KIND_INDIRECT_CALLEE}`, `{DependencyKindHelpers.KIND_INDIRECT_CALLER_AND_CALLEE}`, `{DependencyKindHelpers.KIND_ALL}`.
                     """)]
                string[] dependencyKindsStrings,


                [Description(
                    """
                     Set to true by default to export the graph, unless the user specify otherwise.
                     """)]
                bool exportGraph,

                [Description("A cancellation token for interrupting and canceling the operation.")]
                CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
            {LogHelpers.TOOL_LOG_SEPARATOR}
            Invoking {TOOL_LIST_DEPENDENCIES_NAME} with arguments: 
               cursor=`{cursor ?? "0"}`
               pageSize=`{pageSize}`
               currentOrBaseline=`{currentOrBaseline}`
               
               filterCodeElementChangeStatus=`{filterCodeElementChangeStatus ?? "<default>"}`
               filterCodeElementKinds: `{filterCodeElementKinds.Aggregate("', '")}`
               filterFileName: `{filterFileName ?? "<any>"}`
               filterSimpleNamePattern: `{filterSimpleNamePattern ?? "<any>"}`
               filterParentNamePattern: `{filterParentNamePattern ?? "<any>"}`
               filterProjectName: `{filterProjectName ?? "<any>"}`
               
               dependencyKindsStrings: `{dependencyKindsStrings.Aggregate("', '")}`
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
            dependencyKinds.HasFlag(DependencyKind.IndirectCallerAndCallee) ?
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
