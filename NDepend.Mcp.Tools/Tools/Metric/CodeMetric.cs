using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.Metric {
    [Flags]
    [Description("Code metrics for analysis")]
    public enum CodeMetric {
        [Description(
            $"""
            Lines of code ({CodeMetricHelpers.METRIC_LOC_ACRONYM})
            Problematic: `{CodeElementKindHelpers.KIND_METHOD}`>50, `{CodeElementKindHelpers.KIND_TYPE}`>400
            """)]
        LinesOfCode = 0x01,
        [Description(
            $"""
            Cyclomatic complexity ({CodeMetricHelpers.METRIC_CC_ACRONYM}). 
            Problematic: `{CodeElementKindHelpers.KIND_METHOD}`>20, `{CodeElementKindHelpers.KIND_TYPE}`>200
            """)]
        CyclomaticComplexity = 0x02,

        [Description(
           $"""
            Maintainability index ({CodeMetricHelpers.METRIC_MI_ACRONYM})
            Problematic: <50
            """)]
        MaintainabilityIndex = 0x04,

        [Description(
           $"""
            Halstead ({CodeMetricHelpers.METRIC_HV_ACRONYM})
            Problematic: `{CodeElementKindHelpers.KIND_METHOD}`>600, `{CodeElementKindHelpers.KIND_TYPE}`>8000
            """)]
        HalsteadVolume = 0x08,

        [Description($"Lines of comment ({CodeMetricHelpers.METRIC_COMMENT_ACRONYM})")]
        NbLinesOfComment = 0x10,

        [Description(
           $"""
            Coverage percentage ({CodeMetricHelpers.METRIC_PERCENT_COVERAGE_ACRONYM})
            Problematic: <90%
            """)]
        PercentageCoverage = 0x20,

        [Description("All metrics")]
        All = LinesOfCode | CyclomaticComplexity | MaintainabilityIndex | HalsteadVolume | NbLinesOfComment | PercentageCoverage,

        [Description("No metrics.")]
        None = 0
    }
}