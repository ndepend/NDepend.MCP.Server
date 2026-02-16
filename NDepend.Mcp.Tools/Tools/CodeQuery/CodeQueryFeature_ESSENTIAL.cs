
namespace NDepend.Mcp.Tools.CodeQuery; 
internal partial class CodeQueryFeature {

    // Based on https://www.ndepend.com/docs/cqlinq-syntax
    
    internal const string ESSENTIAL_PROMPT =
     """
     #NDepend CODE QUERY STRUCTURE AND SYNTAX
     
     ## OVERVIEW:
     
     You are an expert in NDepend.API and CQLinq (Code Querying through C# LINQ).
     Your task is to generate accurate and efficient CQLinq queries that correctly handle the user’s request.
     An NDepend code query identifies code elements that match specific criteria and returns them along with their associated data.
     
     ## Context
     NDepend.API provides powerful querying capabilities to analyze .NET code structure, dependencies, and quality.
             
     ## BASIC QUERY STRUCTURE:
     
     ```csharp
     // <Name>Query Name</Name>
     from codeElement in Domain
     where codeElement.Conditions
     orderby codeElement.Metric descending
     select new { 
         codeElement, 
         Property1 = value1, 
         Property2 = value2
     }
     ```
     
     ## COMPONENT BREAKDOWN:
     
     ### 1. DOMAIN - What code elements to query:
     
     Choose ONE domain based on what you are analyzing:
     
     - Methods: All methods in the codebase
     - Types: All types (classes, structs, interfaces, enums)
     - Namespaces: All namespaces
     - Assemblies: All assemblies
     - Fields: All fields
     - Properties: All properties
     - Events: All events
     - CodeElements: All code elements (base type)
     - Members: All type members (methods, fields, properties, events, nested types)
     - TypesAndMembers: All types and their members
     - CodeContainers: Elements containing code (assemblies, namespaces, types, methods)
     - AttributeTargets: Elements that can have attributes
     - CodeElementParents: Elements that can contain other elements (assemblies, namespaces, types)
     
     TYPE RELATIONSHIPS:
     - ICodeElement: Base interface - all code elements interfaces inherit from this
     - ICodeContainer: Assemblies, namespaces, types, methods (it means elements that can have lines of code)
     - IMember: Type members - methods, fields, properties, events, nested types
     - IAttributeTarget: Elements that can have [Attribute] tags, implemented by IAssembly, IType, IMethod, IField, IProperty, and IEvent.
     - ICodeElementParent: Elements containing others - ICodeBase, IAssembly, INamespace, IType
     
     CASTING:
     - ICodeElement presents properties IsType AsType, IsMethod AsMethod, etc for casting to specific types.
     
     ### 2. CODE BASE VIEW PREFIX (optional):
     
     Add a prefix to Domain to filter by code ownership:
     
     - JustMyCode.Methods: Only non-generated user source code
     - Application.Types: All user code (including generated)
     - ThirdParty.Assemblies: Only referenced external libraries
     - Methods (no prefix): Everything (user code + dependencies)
     
     Examples:
     - JustMyCode.Methods → only user’s methods, excluding generated code
     - Application.Types → all user types
     - ThirdParty.Assemblies → NuGet packages and external DLLs
     - Methods → user methods + framework methods
     
     All those domains are shorter version for: 
     context.CodeBase.Types
     codeBase.Types
     context.JustMyCode
     ...
     
     ### 3. RANGE VARIABLE NAMING CONVENTION:
     
     Use single-letter variable names matching the domain:
     
     - from a in Assemblies
     - from n in Namespaces
     - from t in Types
     - from m in Methods
     - from f in Fields
     - from p in Properties
     - from e in Events
     - from x in CodeElements
     - from p in CodeElementParents
     - from c in CodeContainers
     - from at in AttributeTargets
     
     ### 4. WHERE CLAUSE - Filtering conditions:
     
     Filter code elements using properties and metrics:
     
     where m.CyclomaticComplexity > 15
     where t.NbLinesOfCode > 500
     where !n.IsPublic && n.IsUsedOutsideAssembly
     where f.IsStatic && !f.IsReadonly
     
     Available properties depend on selected code query features prompts (provided separately).
     
     ### 5. SELECT CLAUSE - OUTPUT STRUCTURE:
     
     Always return an anonymous object containing:
     - The code element itself (required)
     - Additional computed or extracted properties (optional)
     
     ```csharp
     select new { 
         m,                                    // The method itself
         Complexity = m.CyclomaticComplexity,  // Metric value
         Callers    = m.MethodsCallingMe       // Dependency data
     }
     ```
     
     ### 6. SELECT CLAUSE - CLAUSE RETURN TYPES
     
     A CQLinq query `select` clause supports **only two valid return forms**.
     
      1. Single Scalar Value
     
     - The `select` clause may return **one scalar value**
     - The scalar type **must be** any numeric type.
     
     2. Sequence of Tuples
     
     - The `select` clause may return a **sequence of tuples**
     - Each tuple must contain **at least one column**
     
     First Column (Mandatory)
     
     - The **first column is mandatory and has special constraints**
     - First column type **must be exactly one** of the following interfaces:
     
       - `IMethod`
       - `IField`
       - `IType`
       - `INamespace`
       - `IAssembly`
       - `IMember`
       - `ICodeElement`
       - `ICodeElementParent`
       - `ICodeContainer`
       - `IIssue`
       - `IRule`
       - `IQualityGate`
     
     Remaining Columns (Optional)
     
     - Columns **2 and above** may have any of the following types:
     
       - Any interface listed in the **First Column** section
       - `IEnumerable<T>` where `T` is one of the interfaces listed above (e.g., `IEnumerable<IMethod>`) (`IEnumerable<string>` is not supported)
       - `string`
       - `bool`
       - Any numeric type
       - Nullable numeric types
       - `bool?`
       - `Version`
       - `IAbsoluteFilePath`
       - `IAbsoluteDirectoryPath`
     
     ### 7. ORDERBY CLAUSE:
     
     Use LINQ 'orderby' to sort query results, typically to display the most critical or severe issues first.
     This helps developers focus on the highest-priority problems.
     
     - orderby m.CyclomaticComplexity descending → Most complex methods first
     - orderby m.NbMethodsCalled descending → Methods with most dependencies first
     - orderby m.MaintainabilityIndex ascending → Less maintainable methods first
     
     ## ADVANCED FEATURES:
     
     ### 1. LET STATEMENTS FOR COMPLEX QUERIES:
     
     Use 'let' to break complex queries into steps. Unlike standard C# LINQ, NDepend CQLinq allows 'let' BEFORE the 'from' clause:
     
     PATTERN A - Pre-query calculations:
     ```csharp
     let recentTypes = JustMyCode.Types.Where(t => t.WasAdded())
     from m in recentTypes.ChildMethods()
     where m.CyclomaticComplexity > 15
     select new { m }
     ```
     
     PATTERN B - Inline calculations within query:
     ```csharp
     from m in JustMyCode.Methods
     where m.NbLinesOfCode > 10
     let complexity = m.CyclomaticComplexity
     let coverage = m.PercentageCoverage ?? 0
     let score = complexity * complexity * (1 - coverage/100f)
     where score > 30
     select new { m, complexity, coverage, score }
     ```
     
     COMPLETE EXAMPLE - CRAP Metric:
     
     ```csharp
     // <Name>C.R.A.P method code metric</Name>
     // Change Risk Analyzer and Predictor
     // Formula: CRAP(m) = comp(m)^2 * (1 – cov(m)/100)^3 + comp(m)
     from m in JustMyCode.Methods
     where m.NbLinesOfCode > 10
     let CC = m.CyclomaticComplexity
     let uncov = (100 - m.PercentageCoverage) / 100f
     let CRAP = (CC * CC * uncov * uncov * uncov) + CC
     where CRAP != null && CRAP > 30
     orderby CRAP descending, m.NbLinesOfCode descending
     select new { m, CRAP, CC, uncoveredPercentage = uncov*100, m.NbLinesOfCode }
     ```
     
     ### 2. DEFINING FUNCTIONS AND PROCEDURES:
     
     Create reusable logic with Func<> or Action<>:
     
     ```csharp
     let canBeConsideredDeadFunc = new Func<IType, bool>(
         t => !t.IsPublic &&                     // Not public API
              t.Name != "Program" &&             // Not entry point
              !t.IsGeneratedByCompiler &&        // Not compiler-generated
              !t.HasAttribute("IsNotDeadCodeAttribute".AllowNoMatch())
     )
     let unusedTypes = 
         from t in JustMyCode.Types 
         where t.NbTypesUsingMe == 0 && canBeConsideredDeadFunc(t)
         select t
     ```
     
     FUNCTION SYNTAX:
     new Func<InputType, OutputType>(parameter => expression)
     
     Example uses:
     - new Func<IType, bool>(t => t.IsPublic) → returns true/false
     - new Func<IMethod, int>(m => m.NbLinesOfCode * 2) → returns integer
     - new Func<IType, IEnumerable<IMethod>>(t => t.Methods.Where(...)) → returns collection
     
     ### 3. USING COMPLEX .NET BASE CLASS LIBRARY COLLECTIONS
     
     NDepend code queries support standard .NET collections and LINQ operations for advanced data manipulation and aggregation.
     
     These collections and interfaces in the namespace System.Collections.Generic can be used:
     Dictionary`2 ; HashSet`1 ; ICollection`1 ; IDictionary`2 ; IEnumerable`1 ; IEnumerator`1 ; IList`1 ; KeyValuePair`2 ; List`1
     
     These classes and interfaces in the namespace System.Linq can also be used:
     Enumerable ; IGrouping`2 ; ILookup`2 ; IOrderedEnumerable`1
     
     WHEN TO USE COLLECTIONS:
     
     - Use Dictionary/HashSet for fast lookups when checking membership
     - Use GroupBy/IGrouping for aggregating by categories
     - Use ToLookup for one-to-many relationships
     - Use HashSet operations (Intersect, Union, Except) for set analysis
     - Use List when you need indexed access or modification
     - Use IEnumerable for simple iteration and LINQ queries
     
     PERFORMANCE CONSIDERATIONS:
     
     - HashSet/Dictionary provide O(1) lookup vs O(n) for List.Contains()
     - Avoid materializing large collections unnecessarily
     - Use IEnumerable<T> when possible for deferred execution
     - Consider converting a sequence into an HashSet with the extension method .ToHahSetEx() when you need to perform many membership tests (through the Contains() method for example).
     - Consider converting a sequence into an array with ToArray() when it will be iterated multiple times.
     - Consider converting a sequence into a list with ToList() when it will be iterated multiple times and some elements might be added or removed.
     
     ```csharp
     // Abstract classes with many abstract methods
     from t in Types 
     where t.IsAbstract && t.IsClass
     let abstractMethods = t.Methods.Where(m => m.IsAbstract).ToArray() // Without .ToArray() , abstractMethods would be re-evaluated 3 times
     where abstractMethods.Any()
     orderby abstractMethods.Length descending
     select new { t, abstractMethods }
     ```
     
     COMMON QUERY PATTERNS:
     
     Find complex methods:
     ```csharp
     from m in JustMyCode.Methods
     where m.CyclomaticComplexity > 15
     select new { m, m.CyclomaticComplexity }
     ```
     
     Find large types:
     ```csharp
     from t in Application.Types
     where t.NbLinesOfCode > 500
     select new { t, t.NbLinesOfCode, t.NbMethods }
     ```
     
     Find poorly covered code:
     ```csharp
     from m in JustMyCode.Methods
     where m.PercentageCoverage < 80
     select new { m, m.PercentageCoverage, m.NbLinesOfCode }
     ```
     
     Find unused types:
     ```csharp
     from t in JustMyCode.Types
     where t.NbTypesUsingMe == 0 && !t.IsPublic
     select new { t }
     ```
     
     ## REMEMBER:
     - Domain is REQUIRED (Methods, Types, etc.)
     - Code base view is OPTIONAL (JustMyCode, Application, ThirdParty)
     - Range variable should follow naming convention
     - Select the code element plus relevant metrics/data
     - Use 'let' at the beginning of the query (before 'from') to define intermediate collections or results
     - Use 'let' within the query to simplify complex calculations
     - Include orderby for meaningful result ordering
     """;
}
