using NDepend.CodeModel;
using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.Common {
    [Description("Code element with kind, full name, and source info.")]
    public sealed class CodeElementInfo {

        [Description("Initialize a new CodeElementInfo instance.")]
        public CodeElementInfo(ICodeElement codeElement, string? sourceFileName = null) {

            this.CodeElementKind = codeElement.GetKind();

            this.CodeElementFullName = codeElement.FullName;

            codeElement.ExtractSourceDecl(sourceFileName, out string? filePath, out uint? line);
            this.DeclarationSourceFilePath = filePath;
            this.DeclarationSourceFileLine = line;
        }

        [Description(
            $"""
             The kind of the code element.
             It can be either '${CodeElementKindHelpers.KIND_ASSEMBLY}`, `{CodeElementKindHelpers.KIND_NAMESPACE}`, `{CodeElementKindHelpers.KIND_TYPE}`, `{CodeElementKindHelpers.KIND_METHOD}`, `{CodeElementKindHelpers.KIND_METHOD}`, `{CodeElementKindHelpers.KIND_FIELD}`, `{CodeElementKindHelpers.KIND_PROPERTY}`, or `{CodeElementKindHelpers.KIND_EVENT}`.
             """)]
        public string CodeElementKind { get; set; }

        [Description(
             """
             Full name of the code element:
             - Methods: 'namespace.type.method<T>(param types)'
             - Types: 'namespace.type'
             - Namespaces: full namespace path
             """)]
        public string CodeElementFullName { get; set; }

        [Description("File path of the code element, if available.")]
        public string? DeclarationSourceFilePath { get; set; }

        [Description("Line number where the code element is declared, if available.")]
        public uint? DeclarationSourceFileLine { get; set; }


    }
}
