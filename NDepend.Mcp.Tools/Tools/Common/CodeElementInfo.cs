using NDepend.CodeModel;
using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.Common {
    [Description("Represents a code element with its kind, full name, and source declaration information.")]
    public sealed class CodeElementInfo {

        [Description("Create a new instance of the CodeElementInfo class with the specified code element.")]
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
            $"""
             The full name of the code element.
             {CodeElementKindHelpers.FULL_NAME_EXPLANATION}
             """)]
        public string CodeElementFullName { get; set; }

        [Description("The file path where the code element is declared, if available.")]
        public string? DeclarationSourceFilePath { get; set; }

        [Description("The line number in the source file where the code element is declared, if available.")]
        public uint? DeclarationSourceFileLine { get; set; }


    }
}
