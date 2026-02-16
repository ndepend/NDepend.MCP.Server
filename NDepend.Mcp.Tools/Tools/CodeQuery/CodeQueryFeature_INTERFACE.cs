namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string INTERFACE_PROMPT =
         """
         # INTERFACE IMPLEMENTATIONS AND CONTRACTS
         
         Your task is to generate accurate, efficient CQLinq queries that analyze interfaces, interface implementations, contracts, and interface usage patterns in .NET codebases.
         
         ## APIS
         
         The NDepend.API interface 'IType' presents these properties and methods related to interface:
          
          ```csharp
         bool IsInterface { get; }  // Gets a value indicating whether this type is an interface.
         IEnumerable<IType> InterfacesImplemented { get; } // Gets a sequence of interfaces implemented by this type. The sequence returned is empty if this type doesn’t implement any interface.
         IEnumerable<IType> TypesThatImplementMe { get; } // If this type is an interface, gets a sequence of types implementing this interface, otherwise gets an empty sequence.
         bool Implement(IType @interface); // Returns a value indicating whether this type implements 'interface'.
         bool Implement("interface full name"); // Returns a value indicating whether this type implements the 'interface' specified by the full name.
         ushort NbInterfacesImplemented { get; } // Gets a numeric value counting this class's number of implemented interfaces, or this interface number of extended interfaces.
         ```
         
         The NDepend.API interfaces 'IMethod', 'IProperty', 'IEvent' present these properties and methods related to interface:
         ```csharp
         bool IsExplicitInterfaceImpl { get; } // Gets a value indicating whether this method is an explicit interface method implementation.
         bool IsAbstract { get; } // Gets a value indicating whether this method is an abstract method, or a method declared on an interface.
         IEnumerable<IMethod> OverriddensBase { get; } // Gets a sequence of methods declared in this method’ 'ParentType.BaseClasses' and 'ParentType.InterfacesImplemented', overridden by this method. If this method doesn't override any method, gets an empty sequence. (same for IProperty and IEvent)
         IEnumerable<IMethod> OverridesDirectDerived { get; } IEnumerable<IMethod> OverridesDerived { get; } // For an interface method, both properties are equivalent and return the sequence of methods that implement it. (same for IProperty and IEvent)
         ```
         
         ## CORE CONCEPTS
         
         ### INTERFACE FUNDAMENTALS
         - **Interface Definition**: Contract specifying what a type must implement
         - **Implementation**: Types (classes/structs) that implement interfaces
         - **Explicit Implementation**: Interface members implemented explicitly
         - **Implicit Implementation**: Interface members implemented as public members
         - **Interface Segregation**: Principle that clients shouldn’t depend on interfaces they don’t use
         - **Abstraction**: Using interfaces to decouple implementation from contract
         
         ### INTERFACE PATTERNS
         - **Marker Interfaces**: Empty interfaces used for tagging (e.g., ISerializable)
         - **Single Method Interfaces**: Focused contracts (e.g., IDisposable, IComparable)
         - **Role Interfaces**: Define specific roles or capabilities
         - **Service Interfaces**: Define service contracts
         - **Repository Interfaces**: Data access abstractions
         - **Fat Interfaces**: Interfaces with too many members (Interface Segregation Principle
         (ISP) violation)
         
         ## COMMON QUERY PATTERNS
         
         ### BASIC INTERFACE QUERIES
         ```csharp
         // All interfaces in the codebase
         from t in Types where t.IsInterface select t
         
         // Public interfaces in library assemblies consummed by the application
         from i in ThirdParty.Types 
         where i.IsInterface
         select i
         
         // All types implementing a specific interface
         from t in Types 
         where t.Implement("System.IDisposable") 
         select t
         
         // Types implementing multiple interfaces
         from t in Types 
         where t.NbInterfacesImplemented > 3
         select new { t, t.InterfacesImplemented }
         
         // Interfaces with no implementations
         from i in Types 
         where i.IsInterface && !i.TypesThatImplementMe.Any()
         select i
         
         // Empty Interfaces
         from i in Types 
         where i.IsInterface && !i.Members.Any()
         select i
         ```
         
         ### IMPLEMENTATION ANALYSIS
         ```csharp
         // Classes implementing specific interface
         from t in Types 
         where t.IsClass && t.Implement("System.IDisposable")
         select t
         
         // Types implementing generic interfaces
         from t in Types 
         let genericInterfaces = t.InterfacesImplemented.Where(i => i.IsGeneric)
         where genericInterfaces.Any()
         select new { t, genericInterfaces }
         
         // Interfaces extending other interfaces
         from i in Types 
         where i.IsInterface 
         let ii = i.InterfacesImplemented
         where ii.Any()
         orderby ii.Count() descending
         select new { i, BaseInterfaces = ii }
         
         // Types with explicit interface implementations
         from t in Types 
         let eii = t.Methods.Where(m => m.IsExplicitInterfaceImpl)
         where eii.Any()
         select new { t, eii}
         
         // List implementations per interface
         from i in Types 
         where i.IsInterface
         select new { i, i.TypesThatImplementMe }
         
         // Methods that implement interface methods
         from m in Methods
         where m.ParentType.IsInterface
         select new { m, m.OverridesDerived } // m.OverridesDirectDerived returns same sequence
         ```
         
         ### INTERFACE SEGREGATION PRINCIPLE (ISP)
         ```csharp
         // Fat interfaces (too many members)
         from i in Types 
         where i.IsInterface && i.Members.Count() > 10
         select new { i, i.Members }
         ```
         
         ### ABSTRACTION PATTERNS
         ```csharp
         // Types depending on interfaces (good abstraction)
         from t in Types 
         let interfaceDeps = t.TypesUsed.Where(tu => tu.IsInterface).ToArray()
         let concreteDeps = t.TypesUsed.Where(tu => tu.IsClass || tu.IsStructure).ToArray()
         where interfaceDeps.Length > 0
         orderby interfaceDeps.Length descending
         select new { t, InterfaceDeps = interfaceDeps, ConcreteDeps = concreteDeps }
         
         // Methods returning interfaces
         from m in Methods 
         where m.ReturnType != null && m.ReturnType.IsInterface
         select m
         
         // Fields/Properties typed as interfaces
         from f in Fields 
         where f.FieldType != null && f.FieldType.IsInterface
         select f
         
         // Changing interfaces (anti-pattern low stability, risk change)
         from i in Types 
         where i.IsInterface && 
               i.WasChanged()
         let newMembers = i.Members.Where(m => m.WasAdded())
         let removedMembers = i.OlderVersion().Members.Where(m => m.WasRemoved())
         select new { i, 
            TypesUsers = i.TypesUsingMe, 
            newMembers,
            removedMembers }
         ```
         
         ## QUERY CATEGORIES
         
         ### 1. Interface Discovery
         - List all interfaces
         - Find interfaces by namespace/assembly
         - Identify marker interfaces
         - Locate service contracts
         
         ### 2. Implementation Analysis
         - Count implementations per interface
         - Find unused interfaces
         - Detect explicit vs implicit implementations
         - Analyze implementation patterns
         
         ### 3. Interface Segregation
         - Identify fat interfaces
         - Find ISP violations
         - Detect partial interface usage
         - Suggest interface splits
         
         ### 4. Abstraction Quality
         - Measure dependency on abstractions
         - Find concrete dependencies that should be interfaces
         - Analyze abstraction levels
         - Validate DIP (Dependency Inversion Principle)
         
         ### 5. Contract Completeness
         - Find missing implementations
         - Detect incomplete implementations
         - Validate interface contracts
         - Check interface inheritance
         
         ### 6. Design Patterns
         - Repository pattern usage
         - Strategy pattern interfaces
         - Factory method contracts
         - Observer/event patterns
         
         ### 7. API Design
         - Public interface contracts
         - Versioning considerations
         - Breaking change detection
         - Documentation coverage
         
         ## COMMON ANALYSIS SCENARIOS
         
         ### Scenario 1: Interface Segregation Audit
         Identify interfaces that are too large and should be split into smaller, focused contracts.
         
         ### Scenario 2: Abstraction Analysis
         Measure how well the codebase depends on abstractions rather than concrete implementations.
         
         ### Scenario 3: Unused Interface Detection
         Find interfaces with no implementations or implementations that aren’t used.
         
         ### Scenario 4: Contract Validation
         Ensure all types properly implement their interface contracts without missing members.
         
         ### Scenario 5: Dependency Inversion
         Verify that high-level modules depend on abstractions, not concrete implementations.
         
         ### Scenario 6: Interface Reuse
         Analyze which interfaces are heavily reused vs. single-use interfaces.
         
         ### Scenario 7: Breaking Change Risk
         Identify public interfaces where changes would impact many implementations.
         
         ## EXAMPLE REQUEST PATTERNS
         
         Users may ask queries like:
         - "Find all types implementing IRepository"
         - "Show me fat interfaces violating ISP"
         - "Which interfaces have no implementations?"
         - "Find concrete dependencies that should use interfaces"
         - "Show me explicit interface implementations"
         - "Which types implement too many interfaces?"
         - "Find marker interfaces in the codebase"
         - "Show dependency on abstractions vs. concrete types"
         - "Which interfaces are used most?"
         - "Find breaking change risks in public interfaces"
         
         ## INTERFACE DESIGN BEST PRACTICES
         
         - **ISP Compliance**: Interfaces should be small and focused
         - **Single Responsibility**: Each interface should have one reason to change
         - **Prefer Composition**: Use multiple small interfaces over one large interface
         - **Dependency Inversion**: Depend on abstractions, not concretions
         - **Explicit vs Implicit**: Use explicit implementation for conflict resolution
         - **Documentation**: All public interfaces should be well-documented
         - **Generic Interfaces**: Use for type-safe reusable contracts
         """;
}
