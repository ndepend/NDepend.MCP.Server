using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.Common;

internal static class CurrentOrBaselineHelpers {
    internal const string CURRENT = "Current";
    internal const string BASELINE = "Baseline";

    internal static CurrentOrBaseline GetCurrentOrBaselineVal<C>(ILogger<C> logger, string str) {
        return str switch {
            CURRENT => CurrentOrBaseline.Current,
            BASELINE => CurrentOrBaseline.Baseline,
            _ => throw logger.LogErrorAndGetException(
                $"""
                 Invalid CurrentOrBaseline value: `{str}`.
                 Valid values are `{CURRENT}` or `{BASELINE}`.
                 """)
        };
    }

}
