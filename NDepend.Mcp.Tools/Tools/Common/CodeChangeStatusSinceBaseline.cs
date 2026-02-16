using System.ComponentModel;

namespace NDepend.Mcp.Tools.Common;

[Flags]
[Description("Specifies the change status of a code element when comparing the current snapshot to the baseline snapshot.")]
internal enum CodeChangeStatusSinceBaseline {
    [Description("The code element is not in the baseline; it has been recently introduced.")]
    New = 0x01,

    [Description("The code element exists in both snapshots but has been modified since the baseline.")]
    Modified = 0x02,

    [Description("The code element exists in both snapshots and has not changed since the baseline.")]
    Unchanged = 0x04,

    [Description("The code element was present in the baseline but is missing from the current snapshot.")]
    Removed = 0x08,

    [Description("Default includes all code elements in the current snapshot.")]
    Default = New | Modified | Unchanged
}
