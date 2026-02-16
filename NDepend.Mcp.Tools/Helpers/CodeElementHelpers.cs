using System.Collections;
using NDepend.CodeModel;
using NDepend.CodeQuery;
using NDepend.Mcp.Tools.Common;
using NDepend.TechnicalDebt;

namespace NDepend.Mcp.Helpers; 
internal static class CodeElementHelpers {
    internal static void ExtractSourceDecl(
            this ICodeElement codeElement, 
            string? sourceFileName, 
            out string? filePath, 
            out uint? line) {
        // Find the relevant source declaration, for example some types, namespaces or assemblies have multiple source declarations.
        ISourceDecl? sourceDecl = !codeElement.SourceFileDeclAvailable ? null :
                                  !sourceFileName.IsValid() ? codeElement.SourceDecls.FirstOrDefault() :
                                   codeElement.SourceDecls.FirstOrDefault(s => s.SourceFile.FileName.Equals(sourceFileName, StringComparison.OrdinalIgnoreCase));
        filePath = sourceDecl?.SourceFile.FilePath.ToString();
        line = sourceDecl?.Line;
    }

    internal static string FullyQualifiedName(this ICodeElement? codeElement) {
        if(codeElement is null) {return Constants.NOT_AVAILABLE; }
        
        string prefix = codeElement switch {

            // method prefix
            IMethod { IsPropertyGetter: true } => "property-get",
            IMethod { IsPropertySetter: true } => "property-set",
            IMethod { IsEventAdder: true } => "event-add",
            IMethod { IsEventRemover: true } => "event-remove",
            IMethod { IsExtensionMethod: true } => "extension-method",
            IMethod => "method",

            // type prefix
            IType { IsDelegate: true } => "delegate-class",
            IType { IsAttributeClass: true } => "attribute-class",
            IType { IsExceptionClass: true } => "exception-class",
            IType { IsClass: true } => "class",
            IType { IsStructure: true } => "struct",
            IType { IsInterface: true } => "interface",
            IType { IsEnumeration: true } => "enum",
            IType => "type",

            // field prefix
            IField { IsLiteral: true } => "const-field",
            IField => "field",

            INamespace => "namespace",
            IProperty => "property",
            IEvent => "event",

            _ => "assembly"
        };

        return $"{prefix} {codeElement.FullName}";
    }


    internal static string GetRecordCellValueDescription(this RecordCellValue cellValue, IDebtFormatter debtFormatter) {
        string itemDesc = Constants.NOT_AVAILABLE;
        object untypedValue = cellValue.m_UntypedValue;
        if (untypedValue != null) {
            switch (cellValue.m_RecordCellType) {
                case RecordCellType.Members:
                case RecordCellType.Methods:
                case RecordCellType.Properties:
                case RecordCellType.Events:
                case RecordCellType.Fields:
                case RecordCellType.Types:
                case RecordCellType.Namespaces:
                case RecordCellType.Assemblies:
                case RecordCellType.CodeElements:
                case RecordCellType.CodeElementParents:
                case RecordCellType.CodeContainers:
                case RecordCellType.AttributeTargets:
                    if (untypedValue is IEnumerable seq) {
                        var arr = seq.Cast<ICodeElement>().ToArray();
                        if (arr.Length > 0) {
                            var sb = new StringBuilder(arr.Length.ToString());
                            sb.Append(" code element")
                                .Append(arr.Length > 1 ? "s" : "")
                                .Append(" fully qualified name")
                                .Append(arr.Length > 1 ? "s, dot coma separated" : "")
                                .Append(": ");
                            foreach (var codeElem in arr) {
                                sb.Append(codeElem.FullyQualifiedName());
                                sb.Append(";");
                            }
                            if (sb.Length > 0) {
                                sb.Length -= 1; // Remove last ;
                                itemDesc = sb.ToString();
                            }
                        }
                    }
                    break;

                case RecordCellType.Member:
                case RecordCellType.Method:
                case RecordCellType.Property:
                case RecordCellType.Event:
                case RecordCellType.Field:
                case RecordCellType.Type:
                case RecordCellType.Namespace:
                case RecordCellType.Assembly:
                case RecordCellType.CodeElement:
                case RecordCellType.CodeElementParent:
                case RecordCellType.CodeContainer:
                case RecordCellType.AttributeTarget:
                    if (untypedValue is ICodeElement codeElement) {
                        itemDesc = codeElement.FullyQualifiedName();
                    }
                    break;
                default:
                    itemDesc = cellValue.RecordCellValueToString(debtFormatter);
                    break;
            }
        }

        return itemDesc;
    }

}
