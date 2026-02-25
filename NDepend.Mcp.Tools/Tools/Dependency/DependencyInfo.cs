using System.Diagnostics;
using NDepend.CodeModel;
using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.Dependency {
    [Description("Represents a dependency between two code elements with kind and depth.")]
    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed class DependencyInfo(
            ICodeElement codeElementTarget, 
            ICodeElement codeElementDependent, 
            DependencyKind dependencyKind, 
            uint indirectDepth) {

        [Description("Target code element.")]
        public CodeElementInfo CodeElementTarget { get; set; } = new(codeElementTarget);

        [Description("Dependent code element.")]
        public CodeElementInfo CodeElementDependent { get; set; } = new(codeElementDependent);

        [Description("Dependency kind (direct/indirect, caller/callee).")]
        public DependencyKind DependencyKind { get; set; } = dependencyKind;

        [Description("Dependency depth (1 = direct, >1 = indirect).")]
        public uint IndirectDepth { get; set; } = indirectDepth;

        private string DebuggerDisplay => $"Target: {CodeElementTarget.CodeElementFullName}, Dependent: {CodeElementDependent.CodeElementFullName}, Kind: {DependencyKind}, Depth: {IndirectDepth}";
    }
}
