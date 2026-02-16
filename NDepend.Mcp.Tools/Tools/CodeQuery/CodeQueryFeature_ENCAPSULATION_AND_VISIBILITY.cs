namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string ENCAPSULATION_AND_VISIBILITY_PROMPT =
         """
         # ENCAPSULATION AND VISIBILITY
         
         Your task is to generate accurate, efficient CQLinq queries that analyze code encapsulation and visibility concerns in .NET codebases.
         
         ## CORE CONCEPTS
         
         ### VISIBILITY LEVELS
         - **Public**: Accessible from anywhere
         - **Internal**: Accessible only within the same assembly
         - **Protected**: Accessible within the class and derived classes
         - **Private**: Accessible only within the containing type
         - **Protected Internal**: Accessible within the same assembly OR from derived classes
         - **Private Protected**: Accessible within the same assembly AND only from derived classes
         
         ### KEY ELEMENTS TO QUERY
         
         The interface IMember presents properties Visibility and OptimalVisibility, both typed with the enumeration Visibility.
         
         ```csharp
         public enum Visibility {
            None,
            Public,
            ProtectedAndInternal,
            ProtectedOrInternal,
            Internal,
            Protected,
            Private,
         }
         ```
         
         OptimalVisibility is a Visibility value that represents the most restrictive visibility a member can have without breaking compilation.
         For example, an internal method that is used only within its declaring class has an OptimalVisibility of Private.
         
         The interface IMember also has these properties that can be use to simplfy a code query: `IsPublic`, `IsInternal`, `IsPrivate`, `IsProtected`, `IsProtectedOrInternal`, and `IsProtectedAndInternal`.
         
         The interface IMember also exposes the IsPubliclyVisible (bool) property, which indicates whether the member is accessible from outside the assembly.
         This is useful because for example, a method declared public in an internal type is not publicly visible.
         
         IMember is implemented by IType, IMethod, IField, IProperty, and IEvent.
         
         ## COMMON QUERY PATTERNS
         
         ### BASIC VISIBILITY QUERIES
         ```csharp
         // Public types
         from t in Types where t.IsPublic select t
         
         // Internal types
         from t in Types where t.IsInternal select t
         
         // Methods with a more restricted OptimalVisibility
         from m in Application.Methods 
         where m.Visibility != m.OptimalVisibility
         select new { m, m.Visibility, m.OptimalVisibility }
         
         // Fields should be private
         from f in Fields 
         where !f.IsPrivate && !f.IsLiteral
         select f
         ```
         
         ### ENCAPSULATION VIOLATIONS
         ```csharp
         // Public fields (typically bad practice)
         from f in Fields 
         where f.IsPublic && !f.IsLiteral && !f.IsInitOnly
         select f
         
         // Mutable public fields
         from f in Fields 
         where f.IsPublic && !f.IsReadOnly && !f.IsLiteral
         select f
         
         // Protected fields in sealed classes
         from f in Fields 
         where f.IsProtected && f.ParentType.IsSealed
         select f
         ```
         
         ### API SURFACE ANALYSIS
         ```csharp
         // All publicly visible types
         from t in Types 
         where t.IsPubliclyVisible 
         select t
         
         // Public API methods
         from m in Methods 
         where m.IsPubliclyVisible 
         select m
         
         // Internal types exposed through public API
         from t in Types 
         where t.IsInternal && t.IsUsedByPublicTypes
         select t
         ```
         
         ## COMMON ANALYSIS SCENARIOS
         
         ### Scenario 1: Find Encapsulation Violations
         Identify public fields, over-exposed members, and accessibility issues.
         
         ### Scenario 2: API Surface Audit
         Calculate and list the complete public API to understand what’s exposed.
         
         ### Scenario 3: Visibility Refinement
         Find members that could be more restrictive (public → internal → private).
         
         ### Scenario 4: Framework Design Review
         Ensure proper separation between public contracts and internal implementation.
         
         ### Scenario 5: Breaking Change Analysis
         Identify what visibility changes would be breaking vs. non-breaking.
         """;
}
