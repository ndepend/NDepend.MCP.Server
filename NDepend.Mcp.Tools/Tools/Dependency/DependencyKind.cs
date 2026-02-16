
namespace NDepend.Mcp.Tools.Dependency {

    [Flags]
    [Description("Specifies the kind of dependency relationship between a code element target and a code element dependent")]
    public enum DependencyKind {
        [Description("The dependent code element is a direct caller of the target code element.")]
        DirectCaller = 0x01,
        [Description("The dependent code element is directly called by the target code element.")]
        DirectCallee = 0x02,
        [Description("The dependent code element is both a caller and called by the target code element.")]
        DirectCallerAndCallee = 0x04,

        [Description("The dependent code element is an indirect caller of the target code element.")]
        IndirectCaller = 0x08,
        [Description("The dependent code element is indirectly called by the target code element.")]
        IndirectCallee = 0x10,
        [Description("The dependent code element is both an indirect caller and indirectly called by the target code element.")]
        IndirectCallerAndCallee = 0x20
    }
}
