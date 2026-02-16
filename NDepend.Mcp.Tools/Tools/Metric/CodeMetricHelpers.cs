using NDepend.CodeModel;
using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.Metric;

internal static class CodeMetricHelpers {

    internal const string METRIC_LOC = "lines_of_code";
    internal const string METRIC_CC = "cyclomatic_complexity";
    internal const string METRIC_MI = "maintainability_index";
    internal const string METRIC_HV = "halstead_volume";
    internal const string METRIC_COMMENT = "nb_lines_of_comment";
    internal const string METRIC_PERCENT_COVERAGE = "percentage_coverage";
    internal const string METRIC_ALL = "all";

    internal const string METRIC_LOC_ACRONYM = "LOC";
    internal const string METRIC_CC_ACRONYM = "CC";
    internal const string METRIC_MI_ACRONYM = "MI";
    internal const string METRIC_HV_ACRONYM = "HV";
    internal const string METRIC_COMMENT_ACRONYM = "#Comment";
    internal const string METRIC_PERCENT_COVERAGE_ACRONYM = "%Cov";

    internal static CodeMetric GetCodeMetrics<C>(ILogger<C> logger, IEnumerable<string> arr) {
        CodeMetric result = CodeMetric.None;
        foreach (var str in arr) {
            result |= GetCodeMetric(logger, str);
        }
        return result;
    }

    internal static CodeMetric GetCodeMetric<C>(ILogger<C> logger, string str) {
        return str.ToLowerInvariant() switch {
            METRIC_ALL => CodeMetric.All,
            METRIC_LOC => CodeMetric.LinesOfCode,
            METRIC_CC => CodeMetric.CyclomaticComplexity,
            METRIC_MI => CodeMetric.MaintainabilityIndex,
            METRIC_HV => CodeMetric.HalsteadVolume,
            METRIC_COMMENT => CodeMetric.NbLinesOfComment,
            METRIC_PERCENT_COVERAGE => CodeMetric.PercentageCoverage,
            _ => throw logger.LogErrorAndGetException(
                $"""
                 Invalid code metric value: `{str}`
                 Valid values are `{METRIC_ALL}`, `{METRIC_LOC}`, `{METRIC_CC}`, `{METRIC_MI}`, `{METRIC_HV}`, `{METRIC_COMMENT}`, `{METRIC_PERCENT_COVERAGE}`.
                 """)
        };
    }



    internal static ulong? GetVal(this ICodeContainer codeContainer, CodeMetric metric) {
        if(metric.HasFlag(CodeMetric.LinesOfCode)) {
            return codeContainer.NbLinesOfCode;
        }
        if (metric.HasFlag(CodeMetric.CyclomaticComplexity)) {
            if (codeContainer.IsMethod) { return codeContainer.AsMethod.CyclomaticComplexity; }
            if (codeContainer.IsType) { return codeContainer.AsType.CyclomaticComplexity; }
            if (codeContainer.IsNamespace) {
                long ccn = codeContainer.AsNamespace.ChildTypes.Where(t => t.CyclomaticComplexity != null).Sum(t => (uint)t.CyclomaticComplexity!.Value);
                return ccn ==0 ? null : (ulong) ccn;
            }
            long cca = codeContainer.AsAssembly.ChildTypes.Where(t => t.CyclomaticComplexity != null).Sum(t => (uint)t.CyclomaticComplexity!.Value);
            return cca == 0 ? null : (ulong)cca;
        }
        if (metric.HasFlag(CodeMetric.NbLinesOfComment)) {
            return codeContainer.NbLinesOfComment;
        }
        if (metric.HasFlag(CodeMetric.MaintainabilityIndex)) {
            return codeContainer.MaintainabilityIndex;
        }
        if (metric.HasFlag(CodeMetric.HalsteadVolume)) {
            return codeContainer.HalsteadVolume;
        }
        if (metric.HasFlag(CodeMetric.PercentageCoverage)) {
            return codeContainer.PercentageCoverage  != null ? (uint)codeContainer.PercentageCoverage : null;
        }
        return null;
    }
}
