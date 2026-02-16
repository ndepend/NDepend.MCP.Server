using NDepend.CodeModel;
using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.Metric {

    [Description(
      $"""
      Represents code metrics information for an '${CodeElementKindHelpers.KIND_ASSEMBLY}`, a `{CodeElementKindHelpers.KIND_NAMESPACE}`, a `{CodeElementKindHelpers.KIND_TYPE}` or a `{CodeElementKindHelpers.KIND_METHOD}`.")]
      """)]
    public sealed class MetricInfo {

        [Description("Create a new instance of the MetricInfo class with the specified code element and its code metrics.")]
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

        [Description("The code element for which metrics were collected.")]
        public CodeElementInfo CodeElement { get; set; }
        

        [Description("The number of lines of code for the code element, if requested.")]
        public ulong? LinesOfCode { get; set; }
        [Description("The cyclomatic complexity for the code element, if requested.")]
        public ulong? CyclomaticComplexity { get; set; }
        [Description("The maintainability index for the code element, if requested.")]
        public ulong? MaintainabilityIndex { get; set; }
        [Description("The Halstead volume for the code element, if requested.")]
        public ulong? HalsteadVolume { get; set; }
        [Description("The number of lines of comment for the code element, if requested.")]
        public ulong? NbLinesOfComment { get; set; }
        [Description("The percentage coverage for the code element, if requested.")]
        public ulong? PercentageCoverage { get; set; }
    }
}
