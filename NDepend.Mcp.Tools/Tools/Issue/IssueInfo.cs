
using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.Issue;


[Description("Code issue from NDepend, Roslyn or ReSharper")]
public record IssueInfo {

    [Description("Rule provider")]
    public string RuleProvider { get; set; } = "";
    [Description("Rule ID")]
    public string RuleId { get; set; } = "";
    [Description("Rule name")]
    public string RuleName { get; set; } = "";
    [Description("Plain-English explanation of the issue.")]
    public string Explanation { get; set; } = "";

    [Description(
         $"""
         Severity: `{SeverityHelpers.SEVERITY_BLOCKER}`, `{SeverityHelpers.SEVERITY_CRITICAL}`, `{SeverityHelpers.SEVERITY_HIGH}`, `{SeverityHelpers.SEVERITY_MEDIUM}` or `{SeverityHelpers.SEVERITY_LOW}`
         """)]
    public string Severity { get; set; } = SeverityHelpers.SEVERITY_LOW;

    [Description("Code element fully qualified name")]
    public string CodeElement { get; set; } = "";

    [Description("Source file path")]
    public string SourceFilePath { get; set; } = "";

    [Description("Line number (1-based)")]
    public uint SourceFileLine { get; set; }

    [Description(
        $"""
         Change status vs baseline: `{IssueChangeStatusSinceBaselineHelpers.STATUS_DEFAULT}`, `{IssueChangeStatusSinceBaselineHelpers.STATUS_UNRESOLVED}`, `{IssueChangeStatusSinceBaselineHelpers.STATUS_NEW}` or `{IssueChangeStatusSinceBaselineHelpers.STATUS_FIXED}`
         """)]
    public string IssueChangeStatus { get; set; } = "";

    [Description("Estimated fix time")]
    public string Debt { get; set; } = "";
}

