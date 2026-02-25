using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.Dependency;

internal static class DependencyKindHelpers {
    internal const string KIND_DIRECT_CALLER = "direct-caller";
    internal const string KIND_DIRECT_CALLEE = "direct-callee";
    internal const string KIND_ENTANGLED = "direct-entangled";
    internal const string KIND_INDIRECT_CALLER = "indirect-caller";
    internal const string KIND_INDIRECT_CALLEE = "indirect-callee";
    internal const string KIND_INDIRECT_ENTANGLED = "indirect-entangled";
    internal const string KIND_ALL_DIRECT = "all-direct";
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
            KIND_ENTANGLED => DependencyKind.DirectEntangled,
            KIND_INDIRECT_CALLER => DependencyKind.IndirectCaller,
            KIND_INDIRECT_CALLEE => DependencyKind.IndirectCallee,
            KIND_INDIRECT_ENTANGLED => DependencyKind.IndirectEntangled,
            KIND_ALL_DIRECT => DependencyKind.DirectCaller | DependencyKind.DirectCallee | DependencyKind.DirectEntangled,
            KIND_ALL => DependencyKind.DirectCaller | DependencyKind.DirectCallee | DependencyKind.DirectEntangled |
                        DependencyKind.IndirectCaller | DependencyKind.IndirectCallee | DependencyKind.IndirectEntangled,
            _ => throw logger.LogErrorAndGetException(
                $"""
                 Invalid dependency kind: `{str}`.
                 Valid values are `{KIND_ALL_DIRECT}`,`{KIND_DIRECT_CALLER}`, `{KIND_DIRECT_CALLEE}`, `{KIND_ENTANGLED}`, `{KIND_INDIRECT_CALLER}`, `{KIND_INDIRECT_CALLEE}`, `{KIND_INDIRECT_ENTANGLED}`, `{KIND_ALL}`.
                 """)
        };
    }



}
