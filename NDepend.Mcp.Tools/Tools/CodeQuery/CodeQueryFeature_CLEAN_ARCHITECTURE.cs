namespace NDepend.Mcp.Tools.CodeQuery {
    internal partial class CodeQueryFeature {
        internal const string CLEAN_ARCHITECTURE_PROMPT =
        """
        # .NET CLEAN ARCHITECTURE WITH CQLINQ
        
        ## OBJECTIVE
        Use CQLinq queries and NDepend.API to generate query and rule to enforce Clean Architecture principles in .NET applications, detect layer violations, validate dependency rules, and ensure architectural boundaries are maintained.
        
        ## APIS
        
        ### CORE ARCHITECTURE APIS
        
        #### USEFUL IAssembly MEMBERS
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
        
        #### USEFUL INamespace MEMBERS
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
        
        #### USEFUL IType MEMBERS
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
        
        ### LAYER DETECTION EXTENSIONS
        
        ```csharp
        // Custom function for layer detection to declare before any 'from' 
        let isInDomainLayer = new Predicate<IType>(t =>
            t.ParentNamespace.NameLike(@"^.*\.(Domain|Entities)($|\.)") ||
            t.ParentAssembly.Name.EndsWith(".Domain")
        )
        let isInApplicationLayer= new Predicate<IType>(t =>
            t.ParentNamespace.NameLike(@"^.*\.(Application|UseCases)($|\.)") ||
            t.ParentAssembly.Name.EndsWith(".Application")
        )
        let isInInfrastructureLayer= new Predicate<IType>(t =>
            t.ParentNamespace.NameLike(@"^.*\.(Infrastructure|Persistence)($|\.)") ||
            t.ParentAssembly.Name.EndsWith(".Infrastructure")
        )
        let isInPresentationLayer= new Predicate<IType>(t =>
            t.ParentNamespace.NameLike(@"^.*\.(Presentation|UI|API|Web)($|\.)"))
        ```
        
        ### ACCESSING ARCHITECTURE ELEMENTS
        
        ```csharp
        // Get all assemblies in solution
        IEnumerable<IAssembly> allAssemblies = Application.Assemblies;
        
        // Find specific layer assemblies
        IAssembly domainAssembly = Application.Assemblies
            .FirstOrDefault(a => a.Name.EndsWith(".Domain"));
        
        IAssembly applicationAssembly = Application.Assemblies
            .FirstOrDefault(a => a.Name.EndsWith(".Application"));
        
        // Get types in specific namespace (layer)
        IEnumerable<IType> domainTypes = Application.Types.Where(t => isInDomainLayer(t))
        
        // Check dependencies between layers
        from t in domainTypes .UsingAny(infraTypes) 
        let infraTypesUsed = infraTypes.Intersect(t.TypesUsed)
        select new { t, infraTypesUsed }
        ```
        
        ---
        
        ## CORE CONCEPTS
        
        ### CLEAN ARCHITECTURE LAYERS
        
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
        
        ### THE DEPENDENCY RULE
        
        **CRITICAL PRINCIPLE**: Dependencies must point INWARD only.
        
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
        
        ### ENFORCING CLEAN ARCHITECTURE WITH A CQLINQ CODE RULE
        
        ```csharp
        // <Name>Enforcing Clean Architecture</Name>
        warnif count > 0
        
        let isInDomainLayer = new Predicate<IType>(t =>
            t.ParentNamespace.NameLike(@"^.*\.(Domain|Entities)($|\.)") ||
            t.ParentAssembly.Name.EndsWith(".Domain")
        )
        let isInApplicationLayer= new Predicate<IType>(t =>
            t.ParentNamespace.NameLike(@"^.*\.(Application|UseCases)($|\.)") ||
            t.ParentAssembly.Name.EndsWith(".Application")
        )
        let isInInfrastructureLayer= new Predicate<IType>(t =>
            t.ParentNamespace.NameLike(@"^.*\.(Infrastructure|Persistence)($|\.)") ||
            t.ParentAssembly.Name.EndsWith(".Infrastructure")
        )
        let isInPresentationLayer= new Predicate<IType>(t =>
            t.ParentNamespace.NameLike(@"^.*\.(Presentation|UI|API|Web)($|\.)"))
        
        let domainTypes = Application.Types.Where(t => isInDomainLayer(t))
        let appTypes = Application.Types.Where(t => isInApplicationLayer(t)).ToHashSetEx()  // ToHashSetEx() for fast Intersect() calls below
        let infraTypes = Application.Types.Where(t => isInInfrastructureLayer(t)).ToHashSetEx()
        let presTypes = Application.Types.Where(t => isInPresentationLayer(t)).ToHashSetEx()
        
        // Domain          →  Application    ❌ VIOLATION
        let a = from t in domainTypes.UsingAny(appTypes) 
                let typesUsed = appTypes.Intersect(t.TypesUsed)
                select new { type= t, typesUsed, error = "Domain must not use Application" }
        
        // Domain          →  Infrastructure ❌ VIOLATION
        let b = from t in domainTypes.UsingAny(infraTypes) 
                let typesUsed = infraTypes.Intersect(t.TypesUsed)
                select new { type= t, typesUsed, error = "Domain must not use Infrastructure" }
        
        // Domain          →  Presentation   ❌ VIOLATION
        let c = from t in domainTypes.UsingAny(presTypes) 
                let typesUsed = presTypes.Intersect(t.TypesUsed)
                select new { type= t, typesUsed, error = "Domain must not use Presentation" }
        
        // Application     →  Infrastructure ❌ VIOLATION
        let d = from t in appTypes.UsingAny(infraTypes) 
                let typesUsed = infraTypes.Intersect(t.TypesUsed)
                select new { type= t, typesUsed, error = "Application must not use Infrastructure" }
        
        // Application     →  Presentation   ❌ VIOLATION
        let e = from t in appTypes.UsingAny(presTypes) 
                let typesUsed = presTypes.Intersect(t.TypesUsed)
                select new { type= t, typesUsed, error = "Application must not use Presentation" }
        
        // Infrastructure  →  Presentation   ❌ VIOLATION
        let f = from t in infraTypes.UsingAny(presTypes) 
                let typesUsed = presTypes.Intersect(t.TypesUsed)
                select new { type= t, typesUsed, error = "Infrastructure must not use Presentation" }
        
        from x in a.Concat(b).Concat(c).Concat(d).Concat(e).Concat(f)
        select new { x.type, x.typesUsed, x.error }
        ```
        
        ### ENFORCING DOMAIN LAYER PURITY (NO EXTERNAL DEPENDENCIES)
        
        ```csharp
        // Domain layer must have ZERO external dependencies
        warnif count > 0
              
        let isInDomainLayer= new Predicate<IType>(t =>
            t.ParentNamespace.NameLike(@"^.*\.(Application|UseCases)($|\.)") ||
            t.ParentAssembly.Name.EndsWith(".Application")
        )
        let domainTypes = Application.Types.Where(t => isInDomainLayer(t)).ToHashSetEx()
          
        from t in domainTypes 
          
        // Check for forbidden dependencies
        from tUsed in t.TypesUsed
        where !tUsed.IsThirdParty &&  // Allow framework types like string, int
               tUsed.ParentNamespace != t.ParentNamespace &&  // External namespace or assembly
               !domainTypes.Contains(tUsed) // tUsed innnot in Domain
                            
        select new {
            t,
            domainTypes
        }
        ```
        
        
        ### DOMAIN ENTITIES POLLUTED WITH EF/ORM ATTRIBUTES
        
        ```csharp
        // Domain entities polluted with EF/ORM attributes
        warnif count > 0
              
        let isInDomainLayer= new Predicate<IType>(t =>
            t.ParentNamespace.NameLike(@"^.*\.(Application|UseCases)($|\.)") ||
            t.ParentAssembly.Name.EndsWith(".Application")
        )
        let domainTypes = Application.Types.Where(t => isInDomainLayer(t)).ToHashSetEx()
          
        from t in domainTypes 
        from tag in t.AttributeTagsOnMe
        where tag.AttributeType.FullName.ContainsAny(
           "EntityFramework", "Table", "Column",
           "Key", "Required", "DataAnnotations",
           "Json", "Xml"
        )            
        
        select new {
            t,
            tag.AttributeType,
            @params = tag.Parameters.Select(p =>p.Name +": "+p.Value.ToString()).Aggregate("    ")
        }
        ```
        """;
    }
}
