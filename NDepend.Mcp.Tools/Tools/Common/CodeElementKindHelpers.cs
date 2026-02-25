using NDepend.CodeModel;
using NDepend.Mcp.Helpers;


namespace NDepend.Mcp.Tools.Common; 
internal static class CodeElementKindHelpers {
    internal const string KIND_ALL = "all";
    internal const string KIND_MEMBER = "member";

    internal const string KIND_ASSEMBLY = "assembly";
    internal const string KIND_NAMESPACE = "namespace";
    internal const string KIND_TYPE = "type";
    internal const string KIND_METHOD = "method";
    internal const string KIND_FIELD = "field";
    internal const string KIND_PROPERTY = "property";
    internal const string KIND_EVENT = "event";



    internal static CodeElementKind GetKindOfCodeElementVal<C>(ILogger<C> logger, IEnumerable<string> arr) {
        CodeElementKind result = CodeElementKind.None;
        foreach (var str in arr) {
            result |= GetKindOfCodeElementVal(logger, str);
        }
        return result;
    }
    private static CodeElementKind GetKindOfCodeElementVal<C>(ILogger<C> logger, string str) {
        return str.ToLowerInvariant() switch {
            KIND_ALL => CodeElementKind.All,
            KIND_MEMBER => CodeElementKind.Member,
            KIND_ASSEMBLY => CodeElementKind.Assembly,
            KIND_NAMESPACE => CodeElementKind.Namespace,
            KIND_TYPE => CodeElementKind.Type,
            KIND_METHOD => CodeElementKind.Method,
            KIND_FIELD => CodeElementKind.Field,
            KIND_PROPERTY => CodeElementKind.Property,
            KIND_EVENT => CodeElementKind.Event,
            _ => throw logger.LogErrorAndGetException(
                $"""
                 Invalid kind of code element: `{str}`.
                 Valid values are `{KIND_ALL}`, `{KIND_MEMBER}`, `{KIND_ASSEMBLY}`, `{KIND_NAMESPACE}`, `{KIND_TYPE}`, `{KIND_METHOD}`, `{KIND_FIELD}`, `{KIND_PROPERTY}`, `{KIND_EVENT}`.
                 """)
        };
    }


    internal static string GetKind(this ICodeElement codeElement) {
        return codeElement.IsMethod ?     KIND_METHOD :
               codeElement.IsType ?       KIND_TYPE :
               codeElement.IsNamespace ?  KIND_NAMESPACE :
               codeElement.IsField ?      KIND_FIELD :
               codeElement.IsProperty ?   KIND_PROPERTY :
               codeElement.IsEvent ?      KIND_EVENT :
                                          KIND_ASSEMBLY;
    }
}
