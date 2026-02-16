
namespace NDepend.Mcp.Tools.CodeQuery; 
internal static partial class CodeQueryKind {

    internal const string CODE_QUERY_LIST = "code-query-list";
    internal const string CODE_QUERY_LIST_EXPL = 
        "Generate a standard NDepend LINQ query that returns a collection of code elements (methods, types, namespaces, etc.) with their associated properties, dependencies and metrics. " +
        "Each result row contains one code element plus computed data fields. " +
        "Use this for queries like 'find all methods with high complexity' or 'list types with low coverage'. " +
        "The result is enumerable, not a single number.";
    // Nos special prompt for list code query since it is the default one.
    
    internal const string CODE_RULE = "code-rule";
    internal const string CODE_RULE_EXPL =
        "Generate an NDepend code rule (quality rule) that identifies code violations. " +
        "Must start with 'warnif count > 0' and return code elements that violate specific quality criteria. " +
        "Each returned element represents a potential issue requiring developer attention.";

    internal const string QUALITY_GATE = "quality-gate";
    internal const string QUALITY_GATE_EXPL =
        "Defines an NDepend quality gate, a code quality criterion that must be enforced. " +
        "A quality gate specifies a warning threshold and a failure threshold. " +
        "Based on the measured value, it produces a result: PASS, WARN, or FAIL.";

    internal const string QUERYING_ISSUE_AND_RULE = "querying-issue-rule";
    internal const string QUERYING_ISSUE_AND_RULE_EXPL =
        "Generates an NDepend query that selects issues and rules matching specific conditions. " +
        "Allows filtering, combining, or correlating issues and rules based on severity, scope, or custom logic. " +
        "Useful to build targeted quality checks and advanced reporting scenarios.";

    internal const string TREND_METRIC = "trend-metric";
    internal const string TREND_METRIC_EXPL =
        "Defines an NDepend trend metric, a special code query that returns a scalar value. " +
        "This value is stored over time and used to plot trend charts showing evolution.";

    internal const string CODE_QUERY_SCALAR = "code-query-scalar";
    internal const string CODE_QUERY_SCALAR_EXPL =
        "Generate an NDepend query that returns exactly ONE numeric value (int, double, or float). " +
        "Use aggregation functions (Count, Sum, Average, Max, Min) or calculations to produce a single scalar result. " +
        "Examples: 'count of methods exceeding complexity threshold', 'average coverage percentage', 'total lines of code'. " +
        "The result is a number, not a collection.";

}
