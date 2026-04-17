namespace NDepend.Mcp.Tools.CodeQuery {
    internal partial class CodeQueryFeature {
        internal const string CLEAN_ARCHITECTURE_PROMPT =
        """
        # .NET Clean Architecture with CQLinq

        ## Objective
        Use CQLinq queries and NDepend.API to generate query and rule to enforce Clean Architecture principles in .NET applications, detect layer violations, validate dependency rules, and ensure architectural boundaries are maintained.

        ## APIs

        ### Core Architecture APIs
        
        #### Useful IAssembly Members
        ```csharp
        interface IAssembly : ICodeElement {
            // Assembly metadata
            string Name { get; }
            string FullName { get; }
            Version Version { get; }
            
            // Structural elements
            IEnumerable<INamespace> Namespaces { get; }
            IEnumerable<IType> Types { get; }
            
            // Dependencies
            IEnumerable<IAssembly> AssembliesUsed { get; }          // Assemblies this depends on
            IEnumerable<IAssembly> AssembliesUsingMe { get; }       // Assemblies depending on this
            
            // Metrics
            int NbLinesOfCode { get; }
            int NbTypes { get; }
            int NbNamespaces { get; }
            
            // Layer identification helpers
            bool NameLike(string pattern) { get; }
        }
        ```
        
        #### Useful INamespace Members
        ```csharp
        interface INamespace : ICodeElementParent {
            // Namespace metadata
            string Name { get; }
            string FullName { get; }
            
            // Hierarchy
            INamespace ParentNamespace { get; }
            IEnumerable<INamespace> ChildNamespaces { get; }
            IAssembly ParentAssembly { get; }
            
            // Types
            IEnumerable<IType> Types { get; }
            
            // Dependencies
            IEnumerable<INamespace> NamespacesUsed { get; }
            IEnumerable<INamespace> NamespacesUsingMe { get; }
            
            // Metrics
            int NbLinesOfCode { get; }
            int NbTypes { get; }
            int NbNamespaces { get; }
        }
        ```
        
        #### Useful IType Members
        ```csharp
        interface IType : ICodeElement {
            // Type metadata
            string FullName { get; }
            string SimpleName { get; }
            INamespace ParentNamespace { get; }
            IAssembly ParentAssembly { get; }
            
            // Type classification
            bool IsClass { get; }
            bool IsInterface { get; }
            bool IsAbstract { get; }
            bool IsSealed { get; }
            bool IsPublic { get; }
            
            // Dependencies
            IEnumerable<IType> TypesUsed { get; }                   // Types this depends on
            IEnumerable<IType> TypesUsingMe { get; }                // Types depending on this
            IEnumerable<IType> DerivedTypes { get; }                // Subtypes
            IEnumerable<IType> DirectDerivedTypes { get; }          // Direct subtypes
            IType BaseClass { get; }                                // Supertype
            IEnumerable<IType> BaseClasses { get; }                 // Supertypes
            
            // Members
            IEnumerable<IMethod> Methods { get; }
            IEnumerable<IField> Fields { get; }
            IEnumerable<IProperty> Properties { get; }
            
            // Interface implementation
            IEnumerable<IType> InterfacesImplemented { get; }
            bool Implement(string interfaceFullName)
            bool Implement(IType @interface)
            ushort NbInterfacesImplemented { get; }
        }
        ```
        
        
        ## Core Concepts

        ### Clean Architecture Layers
        
        ```
        ┌─────────────────────────────────────────────────┐
        │          PRESENTATION / UI / API                │
        │  (Controllers, Views, API Endpoints)            │
        └────────────────┬────────────────────────────────┘
                         │ depends on ↓
        ┌────────────────▼────────────────────────────────┐
        │        INFRASTRUCTURE / PERSISTENCE             │
        │  (Repositories, External Services, Database)    │
        └────────────────┬────────────────────────────────┘
                         │ depends on ↓
        ┌────────────────▼────────────────────────────────┐
        │         APPLICATION / USE CASES                 │
        │  (Business Logic, Orchestration, DTOs)          │
        └────────────────┬────────────────────────────────┘
                         │ depends on ↓
        ┌────────────────▼────────────────────────────────┐
        │            DOMAIN / ENTITIES                    │
        │  (Domain Models, Business Rules, Interfaces)    │
        │         *** NO DEPENDENCIES ***                 │
        └─────────────────────────────────────────────────┘
        ```
        
        ### The Dependency Rules
        
        ```
        Presentation    →  Application  ✅ OK
        Presentation    →  Domain       ✅ OK
        Infrastructure  →  Application  ✅ OK
        Infrastructure  →  Domain       ✅ OK
        Application     →  Domain       ✅ OK
        
        Domain          →  Application    ❌ VIOLATION
        Domain          →  Infrastructure ❌ VIOLATION
        Application     →  Infrastructure ❌ VIOLATION
        Application     →  Presentation   ❌ VIOLATION
        ```

        ## Enforcing Clean Architecture

        ```csharp
        // <Name>Enforcing Clean Architecture</Name>
        warnif count > 0
        let getLayerTypes = new Func<string, HashSet<IType>>(
          names => Application.Types.Where(t => names.Split('|').Any(name =>
            t.ParentNamespace.Name.EndsWith("." + name) ||
            t.ParentNamespace.Name.Contains("." + name + ".") ||
            t.ParentAssembly.Name.EndsWith("." + name)
          )).ToHashSetEx()
        )

        // Define layers 
        let layers = new[] {
            new { Name = "Domain", 
                  Types = getLayerTypes("Domain|Entities") },
            new { Name = "Application", 
                  Types = getLayerTypes("Application|UseCases") },
            new { Name = "Infrastructure", 
                  Types = getLayerTypes("Infrastructure|Persistence") },
            new { Name = "Presentation", 
                  Types = getLayerTypes("Presentation|UI|API|Web") }
        }

        // Define forbidden dependencies as anonymous types
        let forbiddenDeps = new[] {
            new { From = "Domain",         To = "Application" },
            new { From = "Domain",         To = "Infrastructure" },
            new { From = "Domain",         To = "Presentation" },
            new { From = "Application",    To = "Infrastructure" },
            new { From = "Application",    To = "Presentation" },
            new { From = "Infrastructure", To = "Presentation" }
        }

        // Search for forbidden dependencies
        from dep in forbiddenDeps
        let sourceLayer = layers.First(l => l.Name == dep.From)
        let targetLayer = layers.First(l => l.Name == dep.To)
        from tUser  in sourceLayer.Types.UsingAny(targetLayer.Types)
        let typesUsed = targetLayer.Types.Intersect(tUser.TypesUsed)
        from tUsed in typesUsed
        select new {
            tUser,
            tUsed,
            error = dep.From + " must not use " + dep.To,
            Debt = 7.ToMinutes().ToDebt(),
            Severity = Severity.High
        }
        ```
        """;
    }
}
