
namespace NDepend.Mcp.Tools.Rule {

    [Description("Represents an NDepend rule, Roslyn analyzer, or ReSharper code inspection.")]
    public record RuleSummaryInfo {
        [Description("Gets or sets the rule identifier.")]
        public string Id { get; set; } = "";
        [Description("Gets or sets the rule name.")]
        public string Name { get; set; } = "";
        [Description("Gets or sets a flag indicating if the rule is critical or not.")]
        public bool IsCritical { get; set; }
        [Description("Gets or sets the number of issues found for this rule.")]
        public int NbIssues { get; set; }
    }
}
