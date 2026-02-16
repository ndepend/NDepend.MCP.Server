namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string NAMING_PROMPT =
         """
         # CODE ELEMENT NAMING
         
         ## Name vs. SimpleName vs. FullName
         
         Code elements (assemblies, namespaces, types, methods, fields, etc.) have names that can be expressed at different levels of detail. 
         This why the ICodeElement interface has three distinct properties for naming: Name, SimpleName, and FullName.
         Understanding these distinctions is essential for writing accurate queries.
         
         Example:
         - Type: FullName="MyNamespace.OuterType+InnerType<T>", Name="OuterType+InnerType<T>", SimpleName="InnerType"
         - Method: FullName="MyNamespace.TypeName.MethodName<U>(Int32,String,List<T>)", Name="MethodName<U>(Int32,String,List<T>)", SimpleName="MethodName"
         - Constructor: FullName="MyNamespace.TypeName..ctor(Int32,String)", Name=".ctor(Int32,String)", SimpleName=".ctor"
         - Class Constructor: FullName="MyNamespace.TypeName..cctor()", Name=".cctor()", SimpleName=".cctor"
         - Field/Property/Event: FullName="MyNamespace.TypeName.MemberName", Name="MemberName", SimpleName="MemberName"
         - Namespace: FullName="N1.N2.N3", Name="N1.N2.N3", SimpleName="N3"
         - Anonymous Namespace: FullName="", Name="", SimpleName=""
         - Assembly: FullName="MyAssembly", Name="MyAssembly", SimpleName="MyAssembly"
         - Property accessors sipmple name start with "get_"/"set_" for properties and "add_"/"remove_" for events.
         
         ## NAME FILTERS:
         
         Many extension methods are available to filter code elements by their names.
         Instead of writing queries like:
         
         ```csharp
         from x in CodeElements          
         where x.Name.Contains("Customer")
         select x
         ```
         
         you can simplify it using:
         
         ```csharp
         CodeElements.WithNameWildcardMatch("*Customer*")
         ```
         
         Other useful extension methods targeting 'IEnumerable<TCodeElement> seq' for name filtering include:
         
         - WithNameLike(this IEnumerable<TCodeElement> seq, string regex)
           Supports standard regular expressions.
           Add "\i" at the end of the pattern to ignore case.
         
         - WithNameIn(this IEnumerable<TCodeElement> seq, params string[] names)
           Filters elements whose name equals any of the provided names.
         
         - WithNameNotIn(this IEnumerable<TCodeElement> seq, params string[] names)
           Filters elements whose name does not equal any of the provided names.
         
         - WithNameWildcardMatch(this IEnumerable<TCodeElement> seq, string name0, string pattern)
           Filters elements whose name matches any of the provided wildcard patterns ('*' supported).
           
         - WithNameWildcardMatchIn(this IEnumerable<TCodeElement> seq, params string[] patterns)
           Filters elements whose name matches any of the provided wildcard patterns ('*' supported).
         
         - WithNameWildcardMatchNotIn(this IEnumerable<TCodeElement> seq, params string[] patterns)
           Filters elements whose name does not match any of the provided wildcard patterns.
         
         All of these methods also exist with WithSimpleName* and WithFullName* variants, allowing filtering by SimpleName or FullName instead of just Name.
         
         ## USAGE EXAMPLES:
         
         ```csharp
         // <Name>Interface name must start with I</Name>
         from t in Application.Types
         where t.IsInterface && !t.SimpleName.StartsWith("I")
         select t
         ```
         
         ```csharp
         // <Name>Attribute class name must be suffixed with Attribute</Name>
         from t in Types
         where t.BaseClasses.WithFullName("System.Attribute").Any()
           && !t.SimpleName.EndsWith("Attribute")
         select t
         ```
         
         ```csharp
         // <Name>Select methods that has overloads (methods with the same SimpleName)</Name>
         from m in Methods
         where m.IsOverloaded
         let overloads = m.Overloads.ToArray()
         select new { m, overloads }
         ```
         
         
         ## STRING EXTENSION METHODS:
         
         Some useful string extension methods for name manipulation include:
         
         bool EqualsAny(this string , string[] array, StringComparison stringComparison)
         bool ContainsAny(this string , string[] array, StringComparison stringComparison)
         bool StartsWithAny(this string , string[] array, StringComparison stringComparison)
         bool EndsWithAny(this string , string[] array, StringComparison stringComparison)
         
         For example:
         ```csharp
         from m in Methods
         where m.SimpleName.ContainsAny("Client", "Customer", StringComparison.OrdinalIgnoreCase)
         select m
         ```
         """;
}
