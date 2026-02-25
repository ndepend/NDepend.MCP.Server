namespace NDepend.Mcp.Tools.Common {

    [Description("Specifies current or baseline snapshot")]
    public enum CurrentOrBaseline {
        [Description("Current snapshot")]
        Current = 0,
        [Description("Baseline snapshot")]
        Baseline = 1,
        [Description("Default=Current")]
        Default = Current
    }
}