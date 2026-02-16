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

    private const string GREATER_THAN = "greater-than-or-equals";
    private const string LOWER_THAN = "lower-than-or-equals";
    private const string MAX_PAGE_SIZE = "500";

    [McpServerTool(Name = TOOL_SEARCH_CODE_METRICS_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                    {Constants.PROMPT_CALL_INITIALIZE}
                    
                    # Code Metrics Collection
                    
                    Collect and analyze code metrics for one or more code elements to assess code quality, maintainability, and test coverage.
                    When the user ask for metrics, call this tool.
                    
                    # Purpose and Use-Cases
                    
                    **Refactoring Candidates:**
                    - "Which code is poorly maintainable in the namespace XYZ?"
                    - "Which methods are too long?"
                    - "Find complex methods that need refactoring"
                    - "Show me the most complex classes"
                    - "What should I refactor first?"
                    
                    **Quality Assessment:**
                    - "How complex is the UserService class?"
                    - "Is this code too complex?"
                    - "What's the maintainability index of the Authenticate method?"
                    
                    **Test Coverage Analysis:**
                    - "Which methods need more tests?"
                    - "What's the test coverage for UserController?"
                    - "Show me coverage metrics"
                    - "Is the PaymentService well-tested?"
                    - "What's our test coverage percentage?"

                    **Size & Complexity:**
                    - "How many lines of code in this class?"
                    - "What's the Halstead volume?"
                    - "Show me complexity metrics"
                    - "Is this method too big?"
                    
                    **Comparison & Prioritization:**
                    - "Compare metrics across services"
                    - "Rank methods by maintainability"
                    - "Show worst-performing code"
                    
                    **Code Review Support:**
                    - "Are these changes introducing complexity?"
                    - "Check metrics for new code"
                    - "Did this refactoring improve maintainability?"
                    - "Validate code quality standards"
                    
                    # Metric Thresholds
                    
                    The following thresholds indicate potentially problematic code that may warrant attention:
                    
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
                    
                    **Filtering:** You can optionally filter results to show only code elements exceeding these thresholds.
                  
                    # Response Structure
                    
                    ## 1. Enable Single-Click Navigation
                    
                    Every code element MUST include:
                    - **Full qualified name** (e.g., `UserService.Authenticate`, `CustomerRepository`)
                    - **Clickable source location**: `ElementName (file.ext:line)`
                    
                    ## 2. Clear Metric Presentation
                    
                    When multiple metrics are collected for a code element:
                    - Present them in a structured, scannable format
                    - Highlight values that exceed thresholds
                    - Group related metrics together (e.g., size metrics, complexity metrics)
                    - Prefix each value with its metric name acronym for clarity (`{CodeMetricHelpers.METRIC_LOC_ACRONYM}`, `{CodeMetricHelpers.METRIC_CC_ACRONYM}`, `{CodeMetricHelpers.METRIC_MI_ACRONYM}`, `{CodeMetricHelpers.METRIC_HV_ACRONYM}`, `{CodeMetricHelpers.METRIC_PERCENT_COVERAGE_ACRONYM}`)
                      
                    ## 3. Indexed List
                    Number all dependencies with 1-based indexing for easy reference.
                    - Enables user follow-up: "Show me more about metric #5"
                    """)]
    public static async Task<ListMetricsPaginatedResult> SearchCodeMetricsTool(
                INDependService service,
                ILogger<MetricToolsLog> logger,

                [Description(
                    "An opaque token representing the pagination position after the last returned result. Set to null to start from the beginning.")]
                string? cursor,

                [Description(
                    $"Maximum number of code elements to include per page. Must not exceed {MAX_PAGE_SIZE} to prevent LLM prompt overflow.")]
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
                     Specify the source file name that contains the code elements, including its extension (e.g., .cs for C# files).
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

                [Description(
                    $"""
                     A list of metric names to retrieve.
                     When it makes sense, you MUST provide multiple related code metrics — or even all metrics — to give a more complete view of code quality.
                     Possible values are `{CodeMetricHelpers.METRIC_LOC}`, `{CodeMetricHelpers.METRIC_CC}`, `{CodeMetricHelpers.METRIC_MI}`, `{CodeMetricHelpers.METRIC_HV}`, `{CodeMetricHelpers.METRIC_COMMENT}` and `{CodeMetricHelpers.METRIC_PERCENT_COVERAGE}`.
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
                              Possible values are null, `{GREATER_THAN}`, or `{LOWER_THAN}`.
                              """)]
                string? thresholdGreaterLower,

                [Description("The threshold value applied to filter code elements based on the selected metric.")]
                ulong? thresholdValue,

                [Description("A cancellation token for interrupting and canceling the operation.")] 
                CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
               {LogHelpers.TOOL_LOG_SEPARATOR}
               Invoking {TOOL_SEARCH_CODE_METRICS_NAME} with arguments: 
                  cursor=`{cursor ?? "0"}`
                  pageSize=`{pageSize}`
                  currentOrBaseline=`{currentOrBaseline}`
                  filterCodeElementChangeStatus=`{filterCodeElementChangeStatus ?? "<default>"}`
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
                bool isGreaterThan = thresholdGreaterLower.Equals(GREATER_THAN, StringComparison.OrdinalIgnoreCase);
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

            var paginatedResult = PaginatedResult.Build(logger, metricsInfo, cursor, pageSize, out var paginatedMetricsInfo);
            return new ListMetricsPaginatedResult(paginatedMetricsInfo, paginatedResult);

   
        }, cancellationToken);

    }
}
