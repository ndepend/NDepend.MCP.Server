
namespace NDepend.Mcp.Tools.Issue;

[Description("Issue details with fix guidance")]
public record IssueDetailsForFixingItInfo : IssueInfo {
    [Description("Rule description")]
    public string RuleDescription { get; set; } = "";
    [Description("How to fix")]
    public string RuleHowToFix { get; set; } = "";
    [Description("Additional key/value info from rule")]
    public Dictionary<string, string> ExtraInfo { get; set; } = new();
}