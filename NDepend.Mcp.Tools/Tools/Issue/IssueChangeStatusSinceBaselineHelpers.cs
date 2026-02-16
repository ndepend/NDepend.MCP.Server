
namespace NDepend.Mcp.Tools.Issue;

internal static class IssueChangeStatusSinceBaselineHelpers {
    internal const string STATUS_NEW = "new";
    internal const string STATUS_UNRESOLVED = "unresolved";
    internal const string STATUS_FIXED = "fixed";
    internal const string STATUS_DEFAULT = "default";


    internal static IssueChangeStatusSinceBaseline GetIssueChangeStatusVal(string str) {
        return str.ToLowerInvariant() switch {
            STATUS_NEW => IssueChangeStatusSinceBaseline.New,
            STATUS_UNRESOLVED => IssueChangeStatusSinceBaseline.Unresolved,
            STATUS_FIXED => IssueChangeStatusSinceBaseline.Fixed,
            _ => IssueChangeStatusSinceBaseline.Default
        };
    }

    internal static string GetString(this IssueChangeStatusSinceBaseline status) {
        if (status.HasFlag(IssueChangeStatusSinceBaseline.Default)) {
            return STATUS_DEFAULT;
        }
        if (status.HasFlag(IssueChangeStatusSinceBaseline.New)) {
            return STATUS_NEW;
        }
        if (status.HasFlag(IssueChangeStatusSinceBaseline.Unresolved)) {
            return STATUS_UNRESOLVED;
        }
        // Assume status.HasFlag(IssueChangeStatusSinceBaseline.Fixed
        return STATUS_FIXED;
    }
}

