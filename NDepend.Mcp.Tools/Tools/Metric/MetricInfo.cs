using NDepend.CodeModel;
using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.Metric {

    [Description(
      $"""
      Code metrics for an '${CodeElementKindHelpers.KIND_ASSEMBLY}`, a `{CodeElementKindHelpers.KIND_NAMESPACE}`, a `{CodeElementKindHelpers.KIND_TYPE}` or a `{CodeElementKindHelpers.KIND_METHOD}`.
      """)]
    public sealed class MetricInfo {

        [Description("Creates MetricInfo with code element and metrics")]
        public MetricInfo(ICodeContainer codeContainer, CodeMetric metrics, string? sourceFileName = null) {

            this.CodeElement = new CodeElementInfo(codeContainer);

            if (metrics.HasFlag(CodeMetric.LinesOfCode)) {
                this.LinesOfCode = codeContainer.GetVal(CodeMetric.LinesOfCode);
            }
            if (metrics.HasFlag(CodeMetric.CyclomaticComplexity)) {
                this.CyclomaticComplexity = codeContainer.GetVal(CodeMetric.CyclomaticComplexity);
            }
            if (metrics.HasFlag(CodeMetric.MaintainabilityIndex)) {
                this.MaintainabilityIndex = codeContainer.GetVal(CodeMetric.MaintainabilityIndex);
            }
            if (metrics.HasFlag(CodeMetric.HalsteadVolume)) {
                this.HalsteadVolume = codeContainer.GetVal(CodeMetric.HalsteadVolume);
            }
            if (metrics.HasFlag(CodeMetric.NbLinesOfComment)) {
                this.NbLinesOfComment = codeContainer.GetVal(CodeMetric.NbLinesOfComment);
            }
            if (metrics.HasFlag(CodeMetric.PercentageCoverage)) {
                this.PercentageCoverage = codeContainer.GetVal(CodeMetric.PercentageCoverage);
            }
        }

        [Description("Code element")]
        public CodeElementInfo CodeElement { get; set; }

        [Description("Lines of code, if requested")]
        public ulong? LinesOfCode { get; set; }
        [Description("Cyclomatic complexity, if requested")]
        public ulong? CyclomaticComplexity { get; set; }
        [Description("Maintainability index, if requested")]
        public ulong? MaintainabilityIndex { get; set; }
        [Description("Halstead volume, if requested")]
        public ulong? HalsteadVolume { get; set; }
        [Description("Lines of comment, if requested")]
        public ulong? NbLinesOfComment { get; set; }
        [Description("Coverage percentage, if requested")]
        public ulong? PercentageCoverage { get; set; }
    }
}
