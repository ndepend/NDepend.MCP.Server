using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.Dependency;

internal static class DependencyKindHelpers {
    internal const string KIND_DIRECT_CALLER = "direct_caller";
    internal const string KIND_DIRECT_CALLEE = "direct_callee";
    internal const string KIND_DIRECT_CALLER_AND_CALLEE = "direct_caller_and_callee";
    internal const string KIND_INDIRECT_CALLER = "indirect_caller";
    internal const string KIND_INDIRECT_CALLEE = "indirect_callee";
    internal const string KIND_INDIRECT_CALLER_AND_CALLEE = "indirect_caller_and_callee";
    internal const string KIND_ALL = "all";

    internal static DependencyKind GetDependencyKinds<C>(ILogger<C> logger, IEnumerable<string> arr) {
        DependencyKind result = 0;
        foreach (var str in arr) {
            result |= GetDependencyKind(logger, str);
        }
        return result;
    }
    private static DependencyKind GetDependencyKind<C>(ILogger<C> logger, string str) {
        return str.ToLowerInvariant() switch {
            KIND_DIRECT_CALLER => DependencyKind.DirectCaller,
            KIND_DIRECT_CALLEE => DependencyKind.DirectCallee,
            KIND_DIRECT_CALLER_AND_CALLEE => DependencyKind.DirectCallerAndCallee,
            KIND_INDIRECT_CALLER => DependencyKind.IndirectCaller,
            KIND_INDIRECT_CALLEE => DependencyKind.IndirectCallee,
            KIND_INDIRECT_CALLER_AND_CALLEE => DependencyKind.IndirectCallerAndCallee,
            KIND_ALL => DependencyKind.DirectCaller | DependencyKind.DirectCallee | DependencyKind.DirectCallerAndCallee |
                        DependencyKind.IndirectCaller | DependencyKind.IndirectCallee | DependencyKind.IndirectCallerAndCallee,
            _ => throw logger.LogErrorAndGetException(
                $"""
                 Invalid dependency kind: `{str}`.
                 Valid values are `{KIND_DIRECT_CALLER}`, `{KIND_DIRECT_CALLEE}`, `{KIND_DIRECT_CALLER_AND_CALLEE}`, `{KIND_INDIRECT_CALLER}`, `{KIND_INDIRECT_CALLEE}`, `{KIND_INDIRECT_CALLER_AND_CALLEE}`, `{KIND_ALL}`.
                 """)
        };
    }



}
