namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string NAMING_PROMPT =
         """
         # Code Element Naming
         
         ## Name / SimpleName / FullName

         - FullName: fully qualified unique identifier
         - Name: human-readable signature
         - SimpleName: short identifier without qualification or signature
         
         Conventions:
         - Nested types use '+'
         - Generics use '<T>'
         - Constructors use '.ctor' (instance) and '.cctor' (static)
         - Property accessors prefix `get_`/`set_` 
         - Event accessors prefix `add_`/`remove_`
         
         Examples:
         - Element | FullName | Name | SimpleName 
         - Type | MyNS.Outer+Inner<T> | Outer+Inner<T> | Inner
         - Method | MyNS.Type.Method<U>(Int32,String) | Method<U>(Int32,String) | Method
         - Constructor | MyNS.Type..ctor(Int32) | .ctor(Int32) | .ctor
         - Static ctor | MyNS.Type..cctor() | .cctor() | .cctor
         - Field/Prop/Event | MyNS.Type.Member | Member | Member
         - Namespace | N1.N2.N3 | N1.N2.N3 | N3
         - Assembly | MyAssembly | MyAssembly | MyAssembly

         ### Name Filter Extensions (on `IEnumerable<TCodeElement>`)
         ```csharp
         WithNameWildcardMatch("*Customer*")    // * wildcard
         WithNameWildcardMatchIn(patterns)      // match any pattern
         WithNameWildcardMatchNotIn(patterns)   // exclude patterns
         WithNameLike("regex\i")                // regex; \i = ignore case
         WithNameIn("A","B")                    // exact match any
         WithNameNotIn("A","B")                 // exclude exact
         ```
         
         All exist as `WithSimpleName*` and `WithFullName*` variants.

         ### Examples
         
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
               
         ## String Extensions

         ```csharp
         str.EqualsAny(string[], StringComparison)
         str.ContainsAny(string[], StringComparison)
         str.StartsWithAny(string[], StringComparison)
         str.EndsWithAny(string[], StringComparison)
         ```

         ### Examples
         
         ```csharp
         from m in Methods
         where m.SimpleName.ContainsAny("Client", "Customer", StringComparison.OrdinalIgnoreCase)
         select m
         ```
         """;
}
