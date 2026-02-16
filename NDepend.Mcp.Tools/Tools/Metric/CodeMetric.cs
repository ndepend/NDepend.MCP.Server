using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.Metric {
    [Flags]
    [Description("Represents the set of code metrics that can be computed for code elements.")]
    public enum CodeMetric {
        [Description(
            $"""
            Number of lines of code.
            A `{CodeElementKindHelpers.KIND_METHOD}` with `{CodeMetricHelpers.METRIC_LOC}` above 50 or 
            a `{CodeElementKindHelpers.KIND_TYPE}` with `{CodeMetricHelpers.METRIC_LOC}` above 400
            is considered problematic.
            """)]
        LinesOfCode = 0x01,
        [Description(
            $"""
            Cyclomatic complexity. 
            A `{CodeElementKindHelpers.KIND_METHOD}` with `{CodeMetricHelpers.METRIC_CC}` above 20 or 
            a `{CodeElementKindHelpers.KIND_TYPE}` with `{CodeMetricHelpers.METRIC_CC}` above 200
            is considered problematic.
            """)]
        CyclomaticComplexity = 0x02,

        [Description(
           $"""
            Maintainability index.
            A code element with `{CodeMetricHelpers.METRIC_MI}` below 50 is considered problematic.
            """)]
        MaintainabilityIndex = 0x04,

        [Description(
           $"""
            Halstead volume.
            A `{CodeElementKindHelpers.KIND_METHOD}` with `{CodeMetricHelpers.METRIC_HV}` above 600 or 
            a `{CodeElementKindHelpers.KIND_TYPE}` with `{CodeMetricHelpers.METRIC_HV}` above 8000
            is considered problematic.
            """)]
        HalsteadVolume = 0x08,

        [Description("Number of lines of comment.")]
        NbLinesOfComment = 0x10,

        [Description(
           $"""
            Percentage coverage.
            A code element with `{CodeMetricHelpers.METRIC_PERCENT_COVERAGE}` below 90% is considered problematic.
            """)]
        PercentageCoverage = 0x20,

        [Description("All code metrics.")]
        All = LinesOfCode | CyclomaticComplexity | MaintainabilityIndex | HalsteadVolume | NbLinesOfComment | PercentageCoverage,

        [Description("No code metrics.")]
        None = 0
    }
}