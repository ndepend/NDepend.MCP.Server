
namespace NDepend.Mcp.Tools.Rule;


[Description("Code rule with description and fix guidance")]
public record RuleDetailInfo : RuleSummaryInfo {
    [Description("Rule provider")]
    public string Provider { get; set; } = "";
    [Description("Rule description")]
    public string Description { get; set; } = "";
    [Description("How to fix violations")]
    public string RuleHowToFix { get; set; } = "";
    [Description("Rule category")]
    public string Category { get; set; } = "";
}
