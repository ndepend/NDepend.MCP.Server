namespace NDepend.Mcp.Tools.Issue {
    [Description("Specifies the change status of an issue when comparing the current snapshot to the baseline snapshot.")]
    [Flags]
    public enum IssueChangeStatusSinceBaseline {

        [Description("The issue is not in the baseline, it has been recently introduced.")]
        New = 0x01,

        [Description("The issue was already in the baseline and is still unresolved.")]
        Unresolved = 0x02,

        [Description("The issue is in the baseline and has been fixed.")]
        Fixed = 0x04,

        [Description("Default is issue found in the current snapshot.")]
        Default = New | Unresolved
    }
}