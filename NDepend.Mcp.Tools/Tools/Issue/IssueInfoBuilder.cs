using System.Diagnostics;
using NDepend.CodeQuery;
using NDepend.Helpers;
using NDepend.Issue;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Tools.Common;
using NDepend.TechnicalDebt;

namespace NDepend.Mcp.Tools.Issue;

/// <summary>
/// Single source of truth for turning an <see cref="IIssue"/> into the agent-facing
/// <see cref="IssueInfo"/> / <see cref="IssueDetailsForFixingItInfo"/> data. Used by the MCP tools
/// (list-issues / get-issue-details-to-fix-it) AND by the NDepend.AI fix-issues orchestrator when it
/// inlines issue descriptions into a coding-agent prompt — so the agent reads the same wording
/// whether an issue reaches it through the prompt or through an MCP call.
/// </summary>
public static class IssueInfoBuilder {

    /// <summary>Fills the <see cref="IssueInfo"/> part shared by list-issues and
    /// get-issue-details-to-fix-it. <see cref="IssueDetailsForFixingItInfo"/> derives from
    /// <see cref="IssueInfo"/> so this method serves both.</summary>
    public static void Fill(IIssue issue, IssueInfo issueInfo, IDebtFormatter debtFormatter, IssueChangeStatusSinceBaseline issueChangeStatus) {

        if (!issue.TryGetExplanation(StringFormattingKind.Readable, debtFormatter, out string explanation)) {
            explanation = Constants.NOT_AVAILABLE;
        }

        issueInfo.RuleProvider = issue.Rule.Provider.GetString();
        issueInfo.RuleId = issue.Rule.Id;
        issueInfo.RuleName = issue.Rule.Name;
        issueInfo.Explanation = explanation;
        issueInfo.Severity = issue.Severity.GetString();
        issueInfo.CodeElement = issue.CodeElement.FullyQualifiedName();
        issueInfo.IsTestCode = issue.CodeElement.IsTestCode;
        issueInfo.SourceFilePath = issue.SourceDecl?.SourceFile?.FilePathString ?? string.Empty;
        issueInfo.SourceFileLine = issue.SourceDecl?.Line ?? 0;
        issueInfo.IssueChangeStatus = issueChangeStatus.GetString();
        issueInfo.Debt = debtFormatter.ToString(issue.Debt.Value);
    }

    /// <summary>Builds the full fix-it view of an issue: the <see cref="IssueInfo"/> part plus the
    /// rule description / how-to-fix and the per-issue extra record values — the exact content the
    /// get-issue-details-to-fix-it MCP tool returns.</summary>
    public static IssueDetailsForFixingItInfo BuildDetailsToFixIt(IIssue issue, IDebtFormatter debtFormatter, IssueChangeStatusSinceBaseline issueChangeStatus) {
        var issueDetails = new IssueDetailsForFixingItInfo();
        Fill(issue, issueDetails, debtFormatter, issueChangeStatus);

        // Fill properties specific to IssueDetailsForFixingItInfo
        if (issue.Rule.TryGetDescription(StringFormattingKind.Readable, out string ruleDescription)) {
            issueDetails.RuleDescription = ruleDescription;
        }
        if (issue.Rule.TryGetHowToFix(StringFormattingKind.Readable, out string ruleHowToFix)) {
            issueDetails.RuleHowToFix = ruleHowToFix;
        }
        if (issue.Record is RecordBase record) {
            var columnNames = issue.ColumnsNames;
            Debug.Assert(columnNames.Count == record.RecordArity);

            var extraInfo = new Dictionary<string, string>();
            for (int i = 1; i < record.RecordArity; i++) { // First column is always the code element we already have in IssueInfo.CodeElement
                RecordCellValue cellValue = record[i];
                if (cellValue.m_RecordCellType == RecordCellType.Debt) { continue; } // Already available in IssueInfo.Debt
                string itemDesc = cellValue.GetRecordCellValueDescription(debtFormatter);
                extraInfo.Add(columnNames[i], itemDesc);
            }
            issueDetails.ExtraInfo = extraInfo;
        }
        return issueDetails;
    }
}
