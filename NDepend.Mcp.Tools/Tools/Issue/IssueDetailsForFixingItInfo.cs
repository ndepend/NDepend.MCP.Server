
namespace NDepend.Mcp.Tools.Issue; 


[Description("Provides fix-oriented details for an issue detected by an NDepend rule.")]
public record IssueDetailsForFixingItInfo : IssueInfo {
    [Description("A detailed description of the rule that reported this issue.")]
    public string RuleDescription { get; set; } = "";

    [Description("Describes how to resolve the issue, based on the corresponding rule.")]
    public string RuleHowToFix { get; set; } = "";

    [Description("Additional key/value information obtained from the NDepend rule related to this issue.")]
    public Dictionary<string, string> ExtraInfo { get; set; } = new();
}
