using NDepend.CodeModel;

namespace NDepend.Mcp.Tools.Dependency; 
internal static class FillDependencies {

    const int INVALID = -1;
    // depth = -1 (INVALID) means no dependency with target code element,
    // depth = 0 means target code element,
    // depth = 1 means direct dependency
    // depth > 1 means indirect dependency
    private sealed class Depth(int callerDepth, int calleeDepth) { 
        internal int CallerDepth { get; set; } = callerDepth;
        internal int CalleeDepth { get; set; } = calleeDepth;
        internal bool JustAdded { get; set; } = true;
    }

    internal static void Go(List<DependencyInfo> dependencies, ICodeElement target, DependencyKind kinds) {

        // Fill the dico of dependencies with their depth, seeds it with the target code element
        var dico = new Dictionary<ICodeElement, Depth> { { target, new Depth(0, 0) } };
        bool newAdded = Fill(dico); // Fill direct dependencies

        // Eventually fill indirect dependencies
        bool fillIndirect = 0 != (kinds & (DependencyKind.IndirectCaller | DependencyKind.IndirectCallee | DependencyKind.IndirectEntangled));
        while (newAdded && fillIndirect) {
            newAdded = Fill(dico);
        }

        // Fill the dependencies list taking account of wanted DependencyKind
        var seen = new HashSet<ICodeElement>(); // Used to avoid duplicate dependent in dependencies
        foreach (var (dependent, depth) in dico) {
            if (seen.Contains(dependent)) continue;

            DependencyKind? finalKind = null;
            uint finalDepth = 1;

            // Direct dependencies
            if (kinds.HasFlag(DependencyKind.DirectEntangled) && depth is { CallerDepth: 1, CalleeDepth: 1 }) {
                finalKind = DependencyKind.DirectEntangled;

            } else if (kinds.HasFlag(DependencyKind.DirectCaller) && depth.CallerDepth == 1) {
                finalKind = DependencyKind.DirectCaller;

            } else if (kinds.HasFlag(DependencyKind.DirectCallee) && depth.CalleeDepth == 1) {
                finalKind = DependencyKind.DirectCallee;
            }

            // Indirect dependencies
            if (finalKind == null) {
                if (kinds.HasFlag(DependencyKind.IndirectEntangled) && depth is { CallerDepth: > 0, CalleeDepth: > 0 }) {
                    finalKind = (depth.CallerDepth == 1 || depth.CalleeDepth == 1) ? DependencyKind.DirectEntangled : DependencyKind.IndirectEntangled;
                    finalDepth = (uint)Math.Min(depth.CallerDepth, depth.CalleeDepth);

                } else if (kinds.HasFlag(DependencyKind.IndirectCaller) && depth.CallerDepth > 0) {
                    finalKind = depth.CallerDepth == 1 ? DependencyKind.DirectCaller : DependencyKind.IndirectCaller;
                    finalDepth = (uint)depth.CallerDepth;

                } else if (kinds.HasFlag(DependencyKind.IndirectCallee) && depth.CalleeDepth > 0) {
                    finalKind = depth.CalleeDepth == 1 ? DependencyKind.DirectCallee : DependencyKind.IndirectCallee;
                    finalDepth = (uint)depth.CalleeDepth;
                }
            }

            if (finalKind != null) {
                seen.Add(dependent);
                dependencies.Add(new DependencyInfo(target, dependent, finalKind.Value, finalDepth));
            }
        }
    }


    private static bool Fill(Dictionary<ICodeElement, Depth> dico) {
        int initialCount = dico.Count;
        foreach (var (elem, depth) in dico.ToArray()) { // Need .ToArray() to avoid modifying collection during enumeration
            if (!depth.JustAdded) continue;
            depth.JustAdded = false;
            FillWith(dico, elem.Callees, depth.CalleeDepth, isCallee: true);
            FillWith(dico, elem.Callers, depth.CallerDepth, isCallee: false);
        }
        return dico.Count > initialCount;
    }

    private static void FillWith(
                Dictionary<ICodeElement, Depth> dico,
                IEnumerable<ICodeElement> dependents,
                int depthLevel,
                bool isCallee) {
        if (depthLevel == INVALID) return;
        depthLevel++; // Add one level of depth

        foreach (ICodeElement dependent in dependents) {
            if (!dico.TryGetValue(dependent, out Depth? d)) {
                dico.Add(dependent, isCallee ? new Depth(INVALID, depthLevel) : new Depth(depthLevel, INVALID));
                continue;
            }

            // Update depth only if we found a shorter path
            int existingDepth = isCallee ? d.CalleeDepth : d.CallerDepth;
            if (existingDepth == INVALID || existingDepth > depthLevel) {
                if (isCallee) { d.CalleeDepth = depthLevel; } 
                else          { d.CallerDepth = depthLevel; }
            }
        }
    }


    extension(ICodeElement elem) {
        IEnumerable<ICodeElement> Callees => elem switch {
            IAssembly asm         => asm.AssembliesUsed,
            INamespace @namespace => @namespace.NamespacesUsed,
            IType type            => type.TypesUsed,
            IMethod method        => method.MethodsCalled,
            // field is not calling any members
            IProperty prop        => prop.MethodsCalled,
            IEvent @event         => @event.MethodsCalled,
            _ => []
        };

        IEnumerable<ICodeElement> Callers => elem switch {
            IAssembly asm         => asm.AssembliesUsingMe,
            INamespace @namespace => @namespace.NamespacesUsingMe,
            IType type            => type.TypesUsingMe,
            IMethod method        => method.MethodsCallingMe,
            IField @field         => @field.MethodsUsingMe,
            IProperty prop        => prop.MethodsUsingMe,
            IEvent @event         => @event.MethodsUsingMe,
            _ => []
        };
    }
}
