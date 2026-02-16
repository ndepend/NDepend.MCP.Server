using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.Common {
    internal static class CodeChangeStatusSinceBaselineHelpers {
        internal const string STATUS_NEW = "new";
        internal const string STATUS_MODIFIED = "modified";
        internal const string STATUS_UNCHANGED = "unchanged";
        internal const string STATUS_REMOVED = "removed";
        internal const string STATUS_DEFAULT = "default";


        internal static CodeChangeStatusSinceBaseline GetCodeChangeStatusVal<C>(ILogger<C> logger, string str) {
            return str.ToLowerInvariant() switch {
                STATUS_NEW => CodeChangeStatusSinceBaseline.New,
                STATUS_MODIFIED => CodeChangeStatusSinceBaseline.Modified,
                STATUS_UNCHANGED => CodeChangeStatusSinceBaseline.Unchanged,
                STATUS_REMOVED => CodeChangeStatusSinceBaseline.Removed,
                STATUS_DEFAULT => CodeChangeStatusSinceBaseline.Default,
                _ => throw logger.LogErrorAndGetException(
                    $"""
                     Invalid code change status: `{str}`.
                     Valid values are `{STATUS_NEW}`, `{STATUS_MODIFIED}`, `{STATUS_UNCHANGED}`, `{STATUS_REMOVED}`, `{STATUS_DEFAULT}`.
                     """)
            };
        }

    }
}
