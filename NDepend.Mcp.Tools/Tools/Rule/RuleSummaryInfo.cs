
namespace NDepend.Mcp.Tools.Rule {

    [Description("Code rule summary")]
    public record RuleSummaryInfo {
        [Description("Rule ID")]
        public string Id { get; set; } = "";
        [Description("Rule name")]
        public string Name { get; set; } = "";
        [Description("Is critical rule")]
        public bool IsCritical { get; set; }
        [Description("Issue count")]
        public int NbIssues { get; set; }
    }
}
