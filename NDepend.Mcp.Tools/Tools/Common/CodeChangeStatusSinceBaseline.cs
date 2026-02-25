using System.ComponentModel;

namespace NDepend.Mcp.Tools.Common;

[Flags]
[Description("Code element change status vs baseline")]
internal enum CodeChangeStatusSinceBaseline {

    // The code element is not in the baseline; it has been recently introduced
    [Description("New")] 
    New = 0x01,

    // The code element exists in both snapshots but has been modified since the baseline.
    [Description("Modified")]
    Modified = 0x02,

    // The code element exists in both snapshots and has not changed since the baseline.
    [Description("Unchanged")]
    Unchanged = 0x04,

    // The code element was present in the baseline but is missing from the current snapshot.
    [Description("Removed from current")]
    Removed = 0x08,

    [Description("Default (New/Modified/Unchanged)")]
    Default = New | Modified | Unchanged
}
