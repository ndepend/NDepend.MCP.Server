
using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.Issue;


[Description("Represents an issue detected by an NDepend rule, a Roslyn analyzer, or a ReSharper code inspection.")]
public record IssueInfo {

    [Description("The rule provider that detected this issue.")]
    public string RuleProvider { get; set; } = "";

    [Description("The identifier of the rule that reported this issue.")]
    public string RuleId { get; set; } = "";

    [Description("The name of the rule that reported this issue.")]
    public string RuleName { get; set; } = "";

    [Description("A plain-English explanation of the issue.")]
    public string Explanation { get; set; } = "";

    [Description($"""
                 The severity level of the issue.
                 It can take value in `{SeverityHelpers.SEVERITY_BLOCKER}`, `{SeverityHelpers.SEVERITY_CRITICAL}`, `{SeverityHelpers.SEVERITY_HIGH}`, `{SeverityHelpers.SEVERITY_MEDIUM}` or `{SeverityHelpers.SEVERITY_LOW}`.
                 """)]
    public string Severity { get; set; } = SeverityHelpers.SEVERITY_LOW;

    [Description("The fully qualified name of the code element involved in this issue.")]
    public string CodeElement { get; set; } = "";

    [Description("The path of the source file that contains this issue.")]
    public string SourceFilePath { get; set; } = "";

    [Description("The 1-based line number in the source file where the issue was detected.")]
    public uint SourceFileLine { get; set; }

    [Description(
        $"""
         The change status of the issue when comparing the current analysis with the baseline analysis.
         Values can be either `{IssueChangeStatusSinceBaselineHelpers.STATUS_DEFAULT}`, `{IssueChangeStatusSinceBaselineHelpers.STATUS_UNRESOLVED}`, `{IssueChangeStatusSinceBaselineHelpers.STATUS_NEW}` or `{IssueChangeStatusSinceBaselineHelpers.STATUS_FIXED}`.
         """)]
    public string IssueChangeStatus { get; set; } = "";

    [Description("The estimated time required to resolve the technical debt associated with this issue.")]
    public string Debt { get; set; } = "";
}

