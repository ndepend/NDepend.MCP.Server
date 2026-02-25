using NDepend.CodeModel;
using NDepend.Helpers;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;
using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;


namespace NDepend.Mcp.Tools.Metric;

[McpServerToolType]
public static class MetricTools {

    internal const string TOOL_SEARCH_CODE_METRICS_NAME = Constants.TOOL_NAME_PREFIX + "search-code-metrics";

    private const string GREATER_THAN_OR_EQUALS = ">=";
    private const string LOWER_THAN_OR_EQUALS = "<=";
    private const string MAX_PAGE_SIZE = "100";


    [McpServerTool(Name = TOOL_SEARCH_CODE_METRICS_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                    {Constants.PROMPT_CALL_INITIALIZE}
                    
                    # Code Metrics Collection
                    
                    Collect code metrics to assess quality, maintainability, and test coverage.
                    
                    Call this tool:
                    - **REQUIRED:** when metrics are requested
                    - **OPTIONAL:** to search code elements by name. Fill filters as needed
                    
                    ## Use-Cases
                    
                    ### Quality
                    - Which code is poorly maintainable?  
                    - Which methods are too long or complex?  
                    
                    ### Refactoring
                    - What should I refactor first?  
                    
                    ### Test Coverage
                    - Which methods need more tests?  
                    - What's the coverage for UserController?  
                    - Which classes are untested?  
                    
                    ### Size & Complexity
                    - Lines of code per class?  
                    - Show complexity metrics  
                    
                    ### Comparison & Prioritization
                    - Compare metrics across services  
                    - Show worst-performing code  
                    
                    ### Code Review
                    - Are changes adding complexity?  
                    - Did refactoring improve maintainability?  
                    
                    ### Code Search
                    - Which types and methods are related to `Login`
                    
                    ## Response Formatting
                    
                    ### 1. Clickable Locations (MANDATORY)
                    Always show: **Full qualified name** `(file.ext:line)` for each code element displayed
                    Example: `method ClientLog() (UserService.cs:45)`
                    
                    ### 2. Number All Rows (1-based) (MANDATORY)
                    Format: `1. `, `2. `, `3. `
                    Enables: `Show me more about #5`
  
                    ### 3. Clear Metric Presentation
                      
                    For multiple metrics per code element:
                    - Show in a structured, scannable format  
                    - Highlight values above thresholds  
                    - Group related metrics and prefix with metric acronyms (`{CodeMetricHelpers.METRIC_LOC_ACRONYM}`, `{CodeMetricHelpers.METRIC_CC_ACRONYM}`, `{CodeMetricHelpers.METRIC_MI_ACRONYM}`, `{CodeMetricHelpers.METRIC_HV_ACRONYM}`, `{CodeMetricHelpers.METRIC_PERCENT_COVERAGE_ACRONYM}`)
                    """)]
    public static async Task<ListMetricsPaginatedResult> SearchCodeMetricsTool(
                INDependService service,
                ILogger<MetricToolsLog> logger,

                [Description(PaginatedResult.PAGINATION_CURSOR_DESC)]
                int cursor,

                [Description($"Max number of code elements per page (≤ {MAX_PAGE_SIZE}) to avoid LLM prompt overflow.")]
                int pageSize,

                [Description($"""
                    Search code elements {CodeElementApplyFilter.FROM_CURRENT_OR_BASELINE_ENUM}.
                    """)]
                string currentOrBaseline,

                [Description(
                    $"""
                    Filter code elements by change status: {CodeElementApplyFilter.CHANGE_STATUS_ENUM}
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
                    $$"""
                      List of metric names to retrieve.
                      You MUST provide multiple related metrics when possible for a fuller view of code quality.
                      Include `{CodeMetricHelpers.METRIC_PERCENT_COVERAGE}` only if requested.
                      Possible values are `{{CodeMetricHelpers.METRIC_LOC}}`, `{{CodeMetricHelpers.METRIC_CC}}`, `{{CodeMetricHelpers.METRIC_MI}}`, `{{CodeMetricHelpers.METRIC_HV}}`, `{{CodeMetricHelpers.METRIC_COMMENT}}` and `{{CodeMetricHelpers.METRIC_PERCENT_COVERAGE}}`.
                      """)]
                string[] metricNames,

                [Description(
                     $"""
                      The name of the metric used to filter code elements by their metric values.
                      Possible values are `{CodeMetricHelpers.METRIC_LOC}`, `{CodeMetricHelpers.METRIC_CC}`, `{CodeMetricHelpers.METRIC_MI}`, `{CodeMetricHelpers.METRIC_HV}`, `{CodeMetricHelpers.METRIC_COMMENT}` and `{CodeMetricHelpers.METRIC_PERCENT_COVERAGE}`.
                      """)]
                string? thresholdMetric,

                [Description($"""
                              The direction of the threshold comparison.
                              Possible values are null, `{GREATER_THAN_OR_EQUALS}`, or `{LOWER_THAN_OR_EQUALS}`.
                              """)]
                string? thresholdGreaterLower,

                [Description(
                  $"""
                   The threshold used to flag code elements for a selected metric.  
                   Here are values indicating potentially problematic code.
                   
                   **All Code Elements:**
                   - `{CodeMetricHelpers.METRIC_MI}` (`{CodeMetricHelpers.METRIC_MI_ACRONYM}`) < 50 → Poor maintainability
                   - `{CodeMetricHelpers.METRIC_PERCENT_COVERAGE}` (`{CodeMetricHelpers.METRIC_PERCENT_COVERAGE_ACRONYM}`) < 90% → Insufficient testing
                   
                   **Methods (`{CodeElementKindHelpers.KIND_METHOD}`):**
                   - `{CodeMetricHelpers.METRIC_LOC}` (`{CodeMetricHelpers.METRIC_LOC_ACRONYM}`) > 50 → Too long, consider splitting
                   - `{CodeMetricHelpers.METRIC_CC}` (`{CodeMetricHelpers.METRIC_CC_ACRONYM}`) > 20 → Too complex, hard to test
                   - `{CodeMetricHelpers.METRIC_HV}` (`{CodeMetricHelpers.METRIC_HV_ACRONYM}`) > 600 → High cognitive load
                   
                   **Types/Classes (`{CodeElementKindHelpers.KIND_TYPE}`):**
                   - `{CodeMetricHelpers.METRIC_LOC}` (`{CodeMetricHelpers.METRIC_LOC_ACRONYM}`) > 400 → Too large, violates SRP
                   - `{CodeMetricHelpers.METRIC_CC}` (`{CodeMetricHelpers.METRIC_CC_ACRONYM}`) > 200 → Too many branches
                   - `{CodeMetricHelpers.METRIC_HV}` (`{CodeMetricHelpers.METRIC_HV_ACRONYM}`) > 8000 → Overly complex
                   """)]
                ulong? thresholdValue,

                CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
               {LogHelpers.TOOL_LOG_SEPARATOR}
               Invoking {TOOL_SEARCH_CODE_METRICS_NAME} with arguments: 
                  cursor= `{cursor}`
                  pageSize= `{pageSize}`
                  currentOrBaseline= `{currentOrBaseline}`
                  filterCodeElementChangeStatus= `{filterCodeElementChangeStatus ?? "<default>"}`
                  filterCodeElementKinds: `{filterCodeElementKinds.Aggregate("', '")}`
                  filterFileName: `{filterFileName ?? "<any>"}`
                  filterSimpleNamePattern: `{filterSimpleNamePattern ?? "<any>"}`
                  filterParentNamePattern: `{filterParentNamePattern ?? "<any>"}`
                  filterProjectName: `{filterProjectName ?? "<any>"}`
                  metricNames: `{metricNames.Aggregate("', '")}`
                  thresholdMetric: `{thresholdMetric ?? "<any>"}`
                  thresholdGreaterLower: `{thresholdGreaterLower ?? "<any>"}`
                  thresholdValue: `{(thresholdValue != null ? thresholdValue.Value.ToString() : "<any>")}`
               """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {

            // Apply currentOrBaseline
            ICodeBaseView codeBase = session.GetApplicationCurrentOrBaseline(logger, currentOrBaseline, out CurrentOrBaseline currentOrBaselineVal);

            // If the user ask about code coverage but code coverage hasen't been imported
            // show the user the import coverage doc link: https://www.ndepend.com/docs/code-coverage
            if ((metricNames.Contains(CodeMetricHelpers.METRIC_PERCENT_COVERAGE) ||
                (thresholdMetric.IsValid() && thresholdMetric == CodeMetricHelpers.METRIC_PERCENT_COVERAGE)
                  ) && codeBase.CodeBase.NbLinesOfCodeCovered == null) {
                logger.LogErrorAndThrow(
                    $"""
                    The LLM detected the user is asking for code coverage metrics and invoked the tool '{TOOL_SEARCH_CODE_METRICS_NAME}' with the metric '{CodeMetricHelpers.METRIC_PERCENT_COVERAGE}'.
                    However code coverage data hasn't been imported in this analysis result.
                    Read the doc to import code coverage data here: https://www.ndepend.com/docs/code-coverage
                    """);
            }

            // Fill codeContainers
            List<ICodeContainer> codeContainers = [];
            var kinds = CodeElementKindHelpers.GetKindOfCodeElementVal(logger, filterCodeElementKinds);
            // Make sure we get only kinds compatible with CodeContainer 
            kinds &= CodeElementKind.Assembly | CodeElementKind.Namespace | CodeElementKind.Type | CodeElementKind.Method; 
            codeContainers.AppendElementsOfKinds(kinds, codeBase);
            // Don't match ctor and cctor whose multiple initialization definitions can be noisy
            codeContainers.RemoveAll(c => c.IsMethod && (c.AsMethod.IsConstructor || c.AsMethod.IsClassConstructor));
            
            // Apply filters
            codeContainers.FilterByChangeStatus(logger, filterCodeElementChangeStatus, currentOrBaselineVal, session.CompareContext);
            codeContainers.FilterByFileName(filterFileName);
            codeContainers.FilterBySimpleNamePattern(filterSimpleNamePattern);
            codeContainers.FilterByParentNamePattern(filterParentNamePattern);
            codeContainers.FilterByProjectName(filterProjectName);

            // Apply threshold filter
            if (thresholdMetric.IsValid() && thresholdGreaterLower.IsValid() && thresholdValue != null) {
                CodeMetric metric = CodeMetricHelpers.GetCodeMetric(logger, thresholdMetric);

                bool isGreaterThan = true; // greater by default
                switch (thresholdGreaterLower) {
                    case GREATER_THAN_OR_EQUALS: break;
                    case LOWER_THAN_OR_EQUALS: isGreaterThan = false; break;
                    default:
                        if(thresholdGreaterLower.ContainsAny("<", "lower", StringComparison.OrdinalIgnoreCase)) {
                            isGreaterThan = false;
                        }
                        break;
                }

                ulong threshold = thresholdValue.Value;

                codeContainers.RemoveAll(c => {
                    ulong? metricVal = c.GetVal(metric);
                    if(metricVal == null) { return true; }
                    return isGreaterThan ? metricVal <= threshold : metricVal >= threshold;
                });
            }

            // Order by threshold metric or first metric
            string? orderByMetricStr = thresholdMetric.IsValid() ? thresholdMetric : metricNames.Any() ? metricNames.First() : null;
            if (orderByMetricStr.IsValid()) {
                static Func<ICodeContainer, ulong> GetMetricVal(CodeMetric orderByMetric) {
                    return c => {
                        ulong? val = c.GetVal(orderByMetric);
                        return val ?? ulong.MinValue;
                    };
                }
                CodeMetric orderByMetric = CodeMetricHelpers.GetCodeMetric(logger, orderByMetricStr);
                if (orderByMetric.EqualsAny(CodeMetric.LinesOfCode, CodeMetric.CyclomaticComplexity, CodeMetric.HalsteadVolume)) {
                    // For LOC, CC and HV we want the highest values first, more is worst
                    codeContainers = codeContainers
                        .OrderByDescending(GetMetricVal(orderByMetric))
                        .ToList();
                } else {
                    // For the MaintainabilityIndex, NbLinesOfComment, PercentageCoverage we want the lowest values first, less is worst
                    codeContainers = codeContainers
                        .OrderBy(GetMetricVal(orderByMetric))
                        .ToList();
                }
            }

            // Select metricsInfo
            CodeMetric metrics = CodeMetricHelpers.GetCodeMetrics(logger, metricNames);
            var metricsInfo = codeContainers.Select(cc => new MetricInfo(cc, metrics, filterFileName)).ToList();

            var paginatedResult = PaginatedResult.Build(logger, metricsInfo, cursor, pageSize, MAX_PAGE_SIZE, out var paginatedMetricsInfo);
            return new ListMetricsPaginatedResult(paginatedMetricsInfo, paginatedResult);

   
        }, cancellationToken);

    }
}
