using System.Diagnostics;
using NDepend.CodeModel;
using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.Dependency {
    [Description("Represents a dependency relationship between two code elements, including kind and depth.")]
    [method: Description("Create a new instance of the DependencyInfo class with the specified code element.")]
    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed class DependencyInfo(
            ICodeElement codeElementTarget, 
            ICodeElement codeElementDependent, 
            DependencyKind dependencyKind, 
            uint indirectDepth) {           

        [Description("The target code element in the dependency relationship.")]
        public CodeElementInfo CodeElementTarget { get; set;    } = new(codeElementTarget);

        [Description("The dependent code element in the dependency relationship.")]
        public CodeElementInfo CodeElementDependent { get; set; } = new(codeElementDependent);

        [Description("The kind of dependency (direct/indirect, caller/callee).")]
        public DependencyKind DependencyKind { get; set; } = dependencyKind;

        [Description("The depth of the dependency (1 for direct, >1 for indirect).")]
        public uint IndirectDepth { get; set; } = indirectDepth;

        private string DebuggerDisplay => $"Target: {CodeElementTarget.CodeElementFullName}, Dependent: {CodeElementDependent.CodeElementFullName}, Kind: {DependencyKind}, Depth: {IndirectDepth}";
    }
}
