
namespace NDepend.Mcp.Tools.CodeQuery; 
internal static partial class CodeQueryKind {

    internal const string CODE_QUERY_LIST = "code-query-list";
    internal const string CODE_QUERY_LIST_EXPL =
        "C# LINQ query returning a collection of code elements with properties, dependencies, and metrics. " +
        "Each row contains one element plus computed fields.";
    // There is no special prompt for list code query since it is the default one and covered by feature 'essential'.

    internal const string CODE_RULE = "code-rule";
    internal const string CODE_RULE_EXPL =
         "Code rule identifying violations. " +
         "Must start with 'warnif count > 0' and return elements violating quality criteria.";

    internal const string QUALITY_GATE = "quality-gate";
    internal const string QUALITY_GATE_EXPL =
        "Quality criterion with warning and failure thresholds. " +
        "Produces PASS, WARN, or FAIL based on measured value.";

    internal const string QUERYING_ISSUE_AND_RULE = "querying-issue-rule";
    internal const string QUERYING_ISSUE_AND_RULE_EXPL =
        "Query selecting issues and rules by conditions. " +
        "Filters by severity, scope, or custom logic for targeted checks and reporting.";

    internal const string TREND_METRIC = "trend-metric";
    internal const string TREND_METRIC_EXPL =
        "Query returning a scalar value stored over time to plot trend charts. " +
        "Can return records (scalar = record count).";

    internal const string CODE_QUERY_SCALAR = "code-query-scalar";
    internal const string CODE_QUERY_SCALAR_EXPL =
        "Query returning ONE numeric value using aggregation (Count, Sum, Average, Max, Min) or calculations. " +
        "Result is a number, not a collection.";

}
