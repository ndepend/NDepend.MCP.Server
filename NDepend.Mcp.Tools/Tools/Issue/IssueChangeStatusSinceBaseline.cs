namespace NDepend.Mcp.Tools.Issue {
    [Description("Issue change status vs baseline")]
    [Flags]
    public enum IssueChangeStatusSinceBaseline {
        [Description("New issue")]
        New = 0x01,
        [Description("Still unresolved")]
        Unresolved = 0x02,
        [Description("Fixed")]
        Fixed = 0x04,
        [Description("Default (new or unresolved)")]
        Default = New | Unresolved
    }
}