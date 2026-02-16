namespace NDepend.Mcp.Tools.Common {
    public enum CurrentOrBaseline {
        [Description("Get the result from the current analysis result.")]
        Current = 0,

        [Description("Get the result from the baseline analysis result.")]
        Baseline = 1,

        [Description("Default is Current.")]
        Default = Current
    }
}