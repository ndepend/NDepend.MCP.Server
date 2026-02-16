
using NDepend.CodeModel;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Services;

namespace NDepend.Mcp.Tools.Common {
    internal static class CodeElementApplyFilter {

        internal static ICodeBaseView GetApplicationCurrentOrBaseline<T>(
                this Session session,
                ILogger<T> logger,
                string currentOrBaseline,
                out CurrentOrBaseline currentOrBaselineVal) {
            currentOrBaselineVal = CurrentOrBaselineHelpers.GetCurrentOrBaselineVal(logger, currentOrBaseline);
            ICodeBaseView codeBase = session.AnalysisResult.CodeBase.Application;
            if (currentOrBaselineVal == CurrentOrBaseline.Baseline) {
                codeBase = session.CompareContext.OlderCodeBase.Application;
            }
            return codeBase;
        }


        extension<TElem>(List<TElem> codeElements) where TElem : ICodeElement {

            internal void FilterByChangeStatus<C>(ILogger<C> logger, string? filterChangeStatus, CurrentOrBaseline currentOrBaselineVal, ICompareContext compareContext) {
                // First obtain typed changeStatus
                bool isCurrent = currentOrBaselineVal == CurrentOrBaseline.Current;
                CodeChangeStatusSinceBaseline changeStatus = isCurrent ? CodeChangeStatusSinceBaseline.Default : CodeChangeStatusSinceBaseline.Removed;
                if (filterChangeStatus.IsValid()) {
                    changeStatus = CodeChangeStatusSinceBaselineHelpers.GetCodeChangeStatusVal(logger, filterChangeStatus);
                    // Remove incompatible value
                    if(isCurrent) { changeStatus &= ~CodeChangeStatusSinceBaseline.Removed; } 
                             else { changeStatus &= ~CodeChangeStatusSinceBaseline.New; }
                    if(changeStatus == 0) { changeStatus = isCurrent ? CodeChangeStatusSinceBaseline.Default : CodeChangeStatusSinceBaseline.Removed; }
                }

                // Most common case, no filtering needed
                if (isCurrent && changeStatus == CodeChangeStatusSinceBaseline.Default) { return; } 

                // Apply filtering
                if (isCurrent && !changeStatus.HasFlag(CodeChangeStatusSinceBaseline.New)) {
                    codeElements.RemoveAll(c => compareContext.WasAdded(c));
                }
                if (!isCurrent && !changeStatus.HasFlag(CodeChangeStatusSinceBaseline.Removed)) {
                    codeElements.RemoveAll(c => compareContext.WasRemoved(c));
                }
                if(!changeStatus.HasFlag(CodeChangeStatusSinceBaseline.Modified)) {
                    codeElements.RemoveAll(c => compareContext.WasChanged(c));
                }
                if (!changeStatus.HasFlag(CodeChangeStatusSinceBaseline.Unchanged)) {
                    codeElements.RemoveAll(c => !compareContext.WasChanged(c));
                }
            }


            internal void FilterByFileName(string? fileName) {
                if (fileName.IsValid()) {
                    codeElements.RemoveAll(
                        c => !c.SourceFileDeclAvailable ||
                             c.SourceDecls.All(d => !d.SourceFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)));
                }
            }

            internal void FilterBySimpleNamePattern(string? pattern) {
                if (pattern.IsValid()) {
                    codeElements.RemoveAll(c => !c.SimpleName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
                }
            }

            internal void FilterByParentNamePattern(string? pattern) {
                if (pattern.IsValid()) {
                    codeElements.RemoveAll(c => {
                        if(!TryGetParent(c, out ICodeElementParent? parent)) {
                            return false;
                        }
                        // Check for FullName in case of namespace 
                        return !(parent is INamespace ? parent.FullName : parent!.SimpleName)
                            .Contains(pattern, StringComparison.OrdinalIgnoreCase);
                    });
                }
            }
      

            internal void FilterByProjectName(string? pattern) {
                if (pattern.IsValid()) {
                    codeElements.RemoveAll(c => {
                        string parentAssemblyName = 
                             c.IsAssembly  ? c.Name :
                             c.IsNamespace ? c.AsNamespace.ParentAssembly.Name :
                             c.IsType      ? c.AsType.ParentAssembly.Name :
                                             c.AsMember.ParentAssembly.Name;
                        return !parentAssemblyName.Equals(pattern, StringComparison.OrdinalIgnoreCase);
                    });
                }
            }
             


            internal void AppendElementsOfKinds(CodeElementKind kind, ICodeBaseView codeBase) {
                if (kind.HasFlag(CodeElementKind.Assembly)) { codeElements.AddRange(codeBase.Assemblies.Cast<TElem>()); }
                if (kind.HasFlag(CodeElementKind.Namespace)) { codeElements.AddRange(codeBase.Namespaces.Cast<TElem>()); }
                if (kind.HasFlag(CodeElementKind.Type)) { codeElements.AddRange(codeBase.Types.Cast<TElem>()); }
                if (kind.HasFlag(CodeElementKind.Method)) { codeElements.AddRange(codeBase.Methods.Cast<TElem>()); }
                if (kind.HasFlag(CodeElementKind.Field)) { codeElements.AddRange(codeBase.Fields.Cast<TElem>()); }
                if (kind.HasFlag(CodeElementKind.Property)) { codeElements.AddRange(codeBase.Properties.Cast<TElem>()); }
                if (kind.HasFlag(CodeElementKind.Event)) { codeElements.AddRange(codeBase.Events.Cast<TElem>()); }
            }
        }

        private static bool TryGetParent(ICodeElement codeElement, out ICodeElementParent? parent) {
            switch (codeElement) {
                case IType type:
                    parent = type.ParentNamespace;
                    return true;
                case IMember member:
                    parent = member.ParentType;
                    return true;
                case INamespace @namespace:
                    parent = @namespace.ParentNamespace;
                    return true;
                default:
                    parent = null;
                    return false;
            }
        }
    }
}
