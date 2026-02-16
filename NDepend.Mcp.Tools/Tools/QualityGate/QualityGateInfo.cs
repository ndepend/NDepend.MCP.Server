using NDepend.Issue;

namespace NDepend.Mcp.Tools.QualityGate;

[Description("Represents a quality gate status with details." +
             "A quality gate is a code-quality criterion that must be satisfied for the code to be considered release-ready.")]
public record QualityGateInfo {

    [Description("The quality gate status.")]
    public QualityGateStatus Status { get; set; }

    [Description("The quality gate name.")]
    public string Name { get; set; } = "";

    [Description("The quality gate description in plain english.")]
    public string Description { get; set; } = "";

    [Description("The quality gate unit string.")]
    public string Unit { get; set; } = "";

    [Description("Return the quality gate value.")]
    public double Value { get; set; } = 0;

    [Description("Return the quality gate value string formatted with unit.")]
    public string ValueString { get; set; } = "";

    [Description("Returns true if when the value of the quality gate is increasing, the quality is considered worst.")]
    public bool MoreIsBad { get; set; }

    [Description("Gets the quality gate fail threshold.")]
    public double FailThreshold { get; set; } = 0;

    [Description("Gets the quality gate warn threshold or 0 if the warn threshold is not defined.")]
    public double WarnThreshold { get; set; } = 0;
}
