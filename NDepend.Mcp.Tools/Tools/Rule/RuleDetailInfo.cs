
namespace NDepend.Mcp.Tools.Rule;


[Description("Represents an NDepend rule, Roslyn analyzer, or ReSharper code inspection, with a detailed description and fix guidance.")]
public record RuleDetailInfo : RuleSummaryInfo {
    [Description("Gets or sets the provider of this rule.")]
    public string Provider { get; set; } = "";
    [Description("Gets or sets a detailed description of this rule.")]
    public string Description { get; set; } = "";

    [Description("Gets or sets how to resolve an issue of this rule.")]
    public string RuleHowToFix { get; set; } = "";
    [Description("Gets or sets the rule category.")]
    public string Category { get; set; } = "";
}
