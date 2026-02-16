namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string USAGE_DEPENDENCY_PROMPT =
         """
         # DEPENDENCIES AND COUPLING ANALYSIS
         
         Your task is to generate accurate, efficient CQLinq queries that analyze dependencies, coupling metrics, architectural relationships, and dependency graphs in .NET codebases.
         
         ## APIS
         
         ### IType Dependency Properties
         IEnumerable<IType> TypesUsed { get; }        // Types this type depends on
         IEnumerable<IType> TypesUsingMe { get; }     // Types depending on this type
         uint? NbTypesUsed { get; }                   // Count of outbound type dependencies
         uint? NbTypesUsingMe { get; }                // Count of inbound type dependencies
         
         ### INamespace Dependency Properties
         IEnumerable<INamespace> NamespacesUsed { get; }      // Namespaces this namespace depends on
         IEnumerable<INamespace> NamespacesUsingMe { get; }   // Namespaces depending on this namespace
         uint? NbNamespacesUsed { get; }                      // Outbound namespace coupling
         uint? NbNamespacesUsingMe { get; }                   // Inbound namespace coupling
         
         ### IAssembly Dependency Properties
         IEnumerable<IAssembly> AssembliesUsed { get; }       // Referenced assemblies
         IEnumerable<IAssembly> AssembliesUsingMe { get; }    // Assemblies referencing this one
         uint? NbAssembliesUsed { get; }                      // Count of outbound assembly references
         uint? NbAssembliesUsingMe { get; }                   // Count of inbound assembly references
         
         ### IMethod Dependency Properties
         IEnumerable<IMethod> MethodsCalled { get; }         // Methods this method calls
         IEnumerable<IMethod> MethodsCallingMe { get; }      // Methods calling this method
         uint? NbMethodsCalled { get; }                      // Count of called methods
         uint? NbMethodsCallingMe { get; }                   // Count of callers methods
         
         ### IField Dependency Properties
         IEnumerable<IMethod> MethodsUsingMe { get; }        // Methods reading or assigning this field
         IEnumerable<IMethod> MethodsAssigningMe { get; }    // Methods assigning this field
         IEnumerable<IMethod> MethodsReadingMeButNotAssigningMe { get; }  // Methods reading but not assigning
         IType FieldType { get; }                             // Type of this field
         
         ### IProperty Dependency Properties
         IEnumerable<IMethod> MethodsUsingMe { get; }        // Methods reading or writing this property
         IEnumerable<IMethod> MethodsReadingMe { get; }      // Methods reading this property
         IEnumerable<IMethod> MethodsWritingMe { get; }      // Methods writing this property
         IEnumerable<IMethod> MethodsCalled { get; }         // Methods called by this property (if any)
         IEnumerable<IMember> MembersUsed { get; }           // Members used by this property
         IEnumerable<IField> FieldsUsed { get; }             // Fields used by this property
         IEnumerable<IField> FieldsAssigned { get; }         // Fields assigned by this property
         
         ### IUSER AND IUSED INTERFACES
         
         IUser and IUsed are interfaces implemented by all NDepend code element types that can participate in a usage relationship.
         
         They are implemented by:
         IAssembly
         INamespace
         IType
         IMethod
         IField   (notice it doesn’t implement IUser since a field don’t call any code element)
         IProperty
         IEvent
         
         ```csharp
         //
         // IUsed
         //
         /// <summary>Represents a code element that can be used from another code element. This interface is implemented by <see cref="IMethod"/>, <see cref="IField"/>, <see cref="IProperty" />, <see cref="IEvent" />, <see cref="IType"/>, <see cref="INamespace"/> and <see cref="IAssembly"/>.</summary>
         public interface IUsed {
         
            /// <summary>Returns true if this code element is used by <paramref name="codeElementUser"/>, otherwise returns false.</summary>
            bool IsUsedBy(ICodeElement codeElementUser);
         
            /// <summary>Returns true if this code element is used by <paramref name="assemblyUser"/>, otherwise returns false.</summary>
            bool IsUsedByAssembly(IAssembly assemblyUser);
         
            /// <summary>Returns true if this code element is used by <paramref name="namespaceUser"/>, otherwise returns false.</summary>
            bool IsUsedByNamespace(INamespace namespaceUser);
         
            /// <summary>Returns true if this code element is used by <paramref name="typeUser"/>, otherwise returns false.</summary>
            bool IsUsedByType(IType typeUser);
         
            /// <summary>Returns true if this code element is used by <paramref name="methodUser"/>, otherwise returns false.</summary>
            bool IsUsedByMethod(IMethod methodUser);
         
            /// <summary>Returns true if this code element is used by <paramref name="propertyUser"/>, otherwise returns false.</summary>
            bool IsUsedByProperty(IProperty propertyUser);
         
            /// <summary>Returns true if this code element is used by <paramref name="eventUser"/>, otherwise returns false.</summary>
            bool IsUsedByEvent(IEvent eventUser);
         }
         
         //
         // IUser
         //
         /// <summary>Represents a code element that can use another code element. This interface is implemented by <see cref="IMethod"/>, <see cref="IProperty" />, <see cref="IEvent" />, <see cref="IType"/>, <see cref="INamespace"/> and <see cref="IAssembly"/>.</summary>
         public interface IUser : ICodeElement, IUsed {
         
            /// <summary>Returns true if this code element is used by <paramref name="codeElementUsed"/>, otherwise returns false.</summary>
            bool IsUsing(ICodeElement codeElementUsed);
         
            /// <summary>Returns true if this code element is used by <paramref name="assemblyUsed"/>, otherwise returns false.</summary>
            bool IsUsingAssembly(IAssembly assemblyUsed);
         
            /// <summary>Returns true if this code element is used by <paramref name="namespaceUsed"/>, otherwise returns false.</summary>
            bool IsUsingNamespace(INamespace namespaceUsed);
         
            /// <summary>Returns true if this code element is used by <paramref name="typeUsed"/>, otherwise returns false.</summary>
            bool IsUsingType(IType typeUsed);
         
            /// <summary>Returns true if this code element is used by <paramref name="methodUsed"/>, otherwise returns false.</summary>
            bool IsUsingMethod(IMethod methodUsed);
         
            /// <summary>Returns true if this code element is used by <paramref name="fieldUsed"/>, otherwise returns false.</summary>
            bool IsUsingField(IField fieldUsed);
         
            /// <summary>Returns true if this code element is used by <paramref name="propertyUsed"/>, otherwise returns false.</summary>
            bool IsUsingProperty(IProperty propertyUsed);
         
            /// <summary>Returns true if this code element is used by <paramref name="eventUsed"/>, otherwise returns false.</summary>
            bool IsUsingEvent(IEvent eventUsed);
         
            /// <summary>Gets a nullable numeric value indicating whether this assembly, namespace, type, or method depends on a dependency cycle.</summary>
            ushort? Level { get; }
         }
         ```
         
         ### EXTENSION METHODS FOR DEPENDENCIES
         
         See below some extension methods related to dependencies. Use those when possible because they are optimized for performance.
         TUsed is a type that implements IUsed.
         TUser is a type that implements IUser.
         
         ```csharp
         //
         // Used By
         //
         IEnumerable<TUsed> UsedBy<TUsed, TUser>(
             this IEnumerable<TUsed> codeElementsUsed,
             TUser codeElementUser)
             // Returns a sub-sequence of codeElementsUsed containing only code elements
             // directly used by codeElementUser.
         
         IEnumerable<TUsed> UsedByAny<TUsed, TUser>(
             this IEnumerable<TUsed> codeElementsUsed,
             IEnumerable<TUser> codeElementsUser)
             // Returns a sub-sequence of codeElementsUsed containing only code elements
             // directly used by any code element in codeElementsUser.
         
         IEnumerable<TUsed> UsedByAll<TUsed, TUser>(
             this IEnumerable<TUsed> codeElementsUsed,
             IEnumerable<TUser> codeElementsUser)
             // Returns a sub-sequence of codeElementsUsed containing only code elements
             // directly used by all code elements in codeElementsUser.
         
         IEnumerable<TUsed> IndirectlyUsedBy<TUsed, TUser>(
             this IEnumerable<TUsed> codeElementsUsed,
             TUser codeElementUser)
             // Returns a sub-sequence of codeElementsUsed containing only code elements
             // directly or indirectly used by codeElementUser.
         
         IEnumerable<TUsed> IndirectlyUsedByAny<TUsed, TUser>(
             this IEnumerable<TUsed> codeElementsUsed,
             IEnumerable<TUser> codeElementsUser)
             // Returns a sub-sequence of codeElementsUsed containing only code elements
             // directly or indirectly used by any code element in codeElementsUser.
         
         ICodeMetric<TUsed, ushort> DepthOfIsUsedBy<TUsed, TUser>(
             this IEnumerable<TUsed> codeElementsUsed,
             TUser codeElementUser)
             // Returns a code metric whose DefinitionDomain is a sub-sequence of
             // codeElementsUsed containing only code elements directly or indirectly
             // used by codeElementUser. The metric value is the depth of usage.
         
         ICodeMetric<TUsed, ushort> DepthOfIsUsedByAny<TUsed, TUser>(
             this IEnumerable<TUsed> codeElementsUsed,
             IEnumerable<TUser> codeElementsUser)
             // Returns a code metric whose DefinitionDomain is a sub-sequence of
             // codeElementsUsed containing only code elements directly or indirectly
             // used by any code element in codeElementsUser. The metric value is the
             // depth of usage.
             
         
         //
         // Used By (user element specified by full name)
         //
         bool IsUsedBy(this IUsed usedCodeElement, string userCodeElementFullName)
             // Returns true if usedCodeElement is directly used by the code element
         
         bool IsUsedByAssembly(this IUsed usedCodeElement, string userAssemblyName)
            // Returns true if usedCodeElement is directly used by the assembly
            
         bool IsUsedByNamespace(this IUsed usedCodeElement, string userNamespaceName)
            // Returns true if usedCodeElement is directly used by the namespace
            
         bool IsUsedByType(this IUsed usedCodeElement, string userTypeFullName)
            // Returns true if usedCodeElement is directly used by the type 
            
         bool IsUsedByMethod(this IUsed usedCodeElement, string userMethodFullName)
             // Returns true if usedCodeElement is directly used by the method
             
         bool IsUsedByProperty(this IUsed usedCodeElement, string userPropertyFullName) {
            // Returns true if usedCodeElement is directly used by the property
            
         bool IsUsedByEvent(this IUsed usedCodeElement, string userEventFullName)
             // Returns true if usedCodeElement is directly used by the event    
             
         ushort? DepthOfIsUsedBy(this IUsed usedCodeElement, string userCodeElementFullName) 
             // Returns the depth of usage of usedCodeElement by the code element
             // whose FullName is userCodeElementFullName. Returns null if there is no
             // usage relationship.
             
         bool IsIndirectlyUsedBy(this IUsed usedCodeElement, string userCodeElementFullName)
             // Returns true if usedCodeElement is directly or indirectly used by the code element
             // whose FullName is userCodeElementFullName.
         
         //
         // Using
         //
         IEnumerable<TUser> Using<TUser, TUsed>(
             this IEnumerable<TUser> codeElementsUser,
             TUsed codeElementUsed)
             // Returns a sub-sequence of codeElementsUser containing only code elements
             // directly using codeElementUsed.
         
         IEnumerable<TUser> UsingAny<TUser, TUsed>(
             this IEnumerable<TUser> codeElementsUser,
             IEnumerable<TUsed> codeElementsUsed)
             // Returns a sub-sequence of codeElementsUser containing only code elements
             // directly using any code element in codeElementsUsed.
         
         IEnumerable<TUser> UsingAll<TUser, TUsed>(
             this IEnumerable<TUser> codeElementsUser,
             IEnumerable<TUsed> codeElementsUsed)
             // Returns a sub-sequence of codeElementsUser containing only code elements
             // directly using all code elements in codeElementsUsed.
         
         IEnumerable<TUser> IndirectlyUsing<TUser, TUsed>(
             this IEnumerable<TUser> codeElementsUser,
             TUsed codeElementUsed)
             // Returns a sub-sequence of codeElementsUser containing only code elements
             // directly or indirectly using codeElementUsed.
         
         IEnumerable<TUser> IndirectlyUsingAny<TUser, TUsed>(
             this IEnumerable<TUser> codeElementsUser,
             IEnumerable<TUsed> codeElementsUsed)
             // Returns a sub-sequence of codeElementsUser containing only code elements
             // directly or indirectly using any code element in codeElementsUsed.
         
         ICodeMetric<TUser, ushort> DepthOfIsUsing<TUser, TUsed>(
             this IEnumerable<TUser> codeElementsUser,
             TUsed codeElementUsed)
             // Returns a code metric whose DefinitionDomain is a sub-sequence of
             // codeElementsUser containing only code elements directly or indirectly
             // using codeElementUsed. The metric value is the depth of usage.
         
         ICodeMetric<TUser, ushort> DepthOfIsUsingAny<TUser, TUsed>(
             this IEnumerable<TUser> codeElementsUser,
             IEnumerable<TUsed> codeElementsUsed)
             // Returns a code metric whose DefinitionDomain is a sub-sequence of
             // codeElementsUser containing only code elements directly or indirectly
             // using any code element in codeElementsUsed. The metric value is the
             // depth of usage.
             
         //
         // Using (used element specified by full name)
         //
         bool IsUsing(this IUser userCodeElement, string usedCodeElementFullName)
             // Returns true if userCodeElement is directly using the code element
             
         bool IsUsingAssembly(this IUser userCodeElement, string usedAssemblyName)
             // Returns true if userCodeElement is directly using the assembly

         bool IsUsingNamespace(this IUser userCodeElement, string usedNamespaceName)
             // Returns true if userCodeElement is directly using the namespace

         bool IsUsingType(this IUser userCodeElement, string usedTypeFullName)
             // Returns true if userCodeElement is directly using the type

         bool IsUsingMethod(this IUser userCodeElement, string usedMethodFullName)
             // Returns true if userCodeElement is directly using the method

         bool IsUsingField(this IUser userCodeElement, string usedFieldFullName)
             // Returns true if userCodeElement is directly using the field

         bool IsUsingProperty(this IUser userCodeElement, string usedPropertyFullName)
             // Returns true if userCodeElement is directly using the property

         bool IsUsingEvent(this IUser userCodeElement, string usedEventFullName)
             // Returns true if userCodeElement is directly using the event

         ushort? DepthOfIsUsing(this IUser userCodeElement, string usedCodeElementFullName)
             // Returns the depth of usage of userCodeElement of the code element

         bool IsIndirectlyUsing(this IUser userCodeElement, string usedCodeElementFullName)
             // Returns true if userCodeElement is directly or indirectly using the code element
         ```
         
         ## CORE CONCEPTS
         
         ### DEPENDENCY FUNDAMENTALS
         - **Dependency**: A relationship where one code element uses/references another
         - **Coupling**: The degree of interdependence between modules/types
         
         ### DEPENDENCY TYPES
         - **Type Dependencies**: Class/interface uses another type
         - **Namespace Dependencies**: One namespace depends on another
         - **Assembly Dependencies**: Assembly references another assembly
         - **Method Dependencies**: Method calls another method
         - **Field Dependencies**: Field typed as another type
         - **Circular Dependencies**: A depends on B, B depends on A (anti-pattern)
         
         ### COUPLING LEVELS
         - **Tight Coupling**: High interdependence, hard to change
         - **Loose Coupling**: Low interdependence, easy to change
         - **Temporal Coupling**: Dependencies on execution order
         - **Content Coupling**: Direct access to internal data
         - **Common Coupling**: Shared global state
         - **Control Coupling**: Passing control flags
         
         ### ARCHITECTURAL PATTERNS
         - **Layered Architecture**: Strict layer dependencies (UI → Business → Data)
         - **Clean Architecture**: Dependencies point inward
         - **Dependency Inversion**: High-level doesn’t depend on low-level
         - **Acyclic Dependencies**: No circular references
         - **Stable Dependencies**: Depend on more stable packages
         
         ## COMMON QUERY PATTERNS
         
         ### BASIC DEPENDENCY QUERIES
         ```csharp
         // All types used by a specific type
         from t in Types 
         where t.IsUsedBy("Product.OrderService")
         select new { t, t.NbLinesOfCode }
         
         // All types used directly or indirectly by a specific type
         from t in Types 
         let depth0 = t.DepthOfIsUsedBy("Product.OrderService")
         where depth0  >= 0 orderby depth0
         select new { t, depth0 }
         
         // All types using a specific type
         from t in Types 
         where t.IsUsing("Product.Customer")
         select new { t, t.NbLinesOfCode }
         
         // All types using directly or indirectly a specific type
         from t in Types 
         let depth0 = t.DepthOfIsUsedBy("Product.Customer")
         where depth0  >= 0 orderby depth0
         select new { t, depth0 }
         ```
         
         ### COUPLING METRICS
         ```csharp
         // Highly coupled types (many dependencies)
         from t in Types 
         where t.TypesUsed.Count() > 20
         select new { t, Dependencies = t.TypesUsed }
        
         // Instability metric
         from t in Types 
         where t.IsClass
         let ce = t.TypesUsed.Count()
         let ca = t.TypesUsingMe.Count()
         let instability = (ce + ca) > 0 ? (double)ce / (ce + ca) : 0
         select new { t, Ce = ce, Ca = ca, Instability = instability }
         
         // Highly unstable types (instability > 0.8)
         from t in Types 
         where t.IsClass
         let ce = t.TypesUsed.Count()
         let ca = t.TypesUsingMe.Count()
         let instability = (ce + ca) > 0 ? (double)ce / (ce + ca) : 0
         where instability > 0.8
         select new { t, Instability = instability }
         ```
         
         ### NAMESPACE DEPENDENCIES
         ```csharp
         // Namespace dependency graph
         from n in Application.Namespaces 
         where n.NbNamespacesUsed >= 1
         orderby n.NbNamespacesUsed descending 
         select new { 
            n,
            n.NamespacesUsed,
            n.NamespacesUsingMe
         }

         // Types involved in coupling between two namespaces
         from nCaller in Namespaces
         from nCalled in nCaller.NamespacesUsed
         select new { 
            nCaller , 
            nCalled , 
            TypesCallers = nCaller.ChildTypes.Where(t => t.TypesUsed.Any(tu => tu.ParentNamespace == nCalled)),
            TypesCalled = nCalled.ChildTypes.Where(t => t.TypesUsingMe.Any(tu => tu.ParentNamespace == nCaller)) 
         }
         ```
         
         ### ASSEMBLY DEPENDENCIES
         ```csharp
         // Assembly graph
         from a in Application.Assemblies 
         where a.AssembliesUsed.Count() >= 1
         orderby a.AssembliesUsed.Count() descending 
         select new { 
            a,
            a.AssembliesUsed,
            a.AssembliesUsingMe
         }
         
         // Assemblies with many dependencies
         from a in Assemblies 
         where a.NbAssembliesUsed > 15
         select new { a, DependencyCount = a.NbAssembliesUsed }
         
         // Non .NET BCL Third-party dependencies
         from a in Assemblies 
         let thirdParties = a.AssembliesUsed.Where(
            au => au.IsThirdParty && 
                 !au.Name.StartsWithAny("System","Microsoft")).ToArray()
         where thirdParties.Any()
         select new { a, thirdParties }
         ```
         
         ### CIRCULAR DEPENDENCY DETECTION
         ```csharp
         // Pairs of types mutually dependent
         from t1 in Types 
         from t2 in t1.TypesUsed
         where t2.TypesUsed.Contains(t1)
         select new { Type1 = t1, Type2 = t2, Issue = "Mutuall dependent" }
         
         // Pairs of namespaces mutually dependent
         from n in Application.Namespaces 
         from nUsed in n.NamespacesUsed
         where nUsed.NamespacesUsed.Contains(n)
         select new { Namespace1 = n, Namespace2 = nUsed, Issue = "Mutually dependent" }
         
         // Indirect circular dependencies between namespaces (A→B→***→A)
         let dico = Application.Namespaces.ToDictionary(
          n => n,
          n => n.NamespacesUsed.FillIterative(
                    ns => ns.SelectMany(nx => nx.NamespacesUsed)
                ).DefinitionDomain
                .ToHashSetEx()) // because we call .Contains()
         from n1 in dico 
         from n2 in dico 
         where string.Compare(n1.Key.Name, n2.Key.Name) == 1
         && n1.Value.Contains(n2.Key)
         && n2.Value.Contains(n1.Key)
         select new { n1.Key, other = n2.Key, Issue = "Both using each other, directly or indirectly" }
         ```
         
         In the query above, notice the usage of the special FillIterative() extension method:
         
         ```csharp
         // Iteratively fills a sequence of code elements, until no new element can be added.
         // The first iteration starts with 'initialSeq', and the function 'func' is used to compute new elements of the iteration 'N+1' from new elements computed at iteration 'N'.
         ICodeMetric<TCodeElement, ushort> FillIterative<TCodeElement>(this IEnumerable<TCodeElement> initialSeq, Func<IEnumerable<TCodeElement>,IEnumerable<TCodeElement>> func)
         where TCodeElement : class, ICodeElement
         ```
         
         ### ARCHITECTURE VIOLATION DETECTION
         ```csharp
         // Layer violations (UI depending on Data)
         let uiTypes = Application.Types.Where(t => t.ParentNamespace.Name.Contains("UI"))
         let dbTypes = Application.Types.Where(t => t.ParentNamespace.Name.Contains("Data")).ToHashSetEx()
         from tUI in uiTypes.UsingAny(dbTypes)
         let dbTypesUsed = tUI.TypesUsed.Intersect(dbTypes)
         select new { tUI, dbTypesUsed , Violation = "Layer violation" }
         
         // Business logic depending on infrastructure
         let bizTypes = Application.Types.Where(t => t.ParentNamespace.Name.ContainsAny("Business", "Domain "))
         let infraTypes = Application.Types.Where(t => t.ParentNamespace.Name.ContainsAny("Data", "Infrastructure")).ToHashSetEx()
         from tBiz in bizTypes .UsingAny(infraTypes )
         let infraTypesUsed = tBiz.TypesUsed.Intersect(infraTypes )
         select new { tBiz, infraTypesUsed, Violation = "DIP violation" }
         ```
         
         ### DEPENDENCY ANALYSIS BY CATEGORY
         ```csharp
         // External library dependencies
         from t in Application.Types 
         where t.IsClass
         let externalDeps = t.TypesUsed.Where(tu => 
            !tu.ParentAssembly.Name.StartsWith("System") && 
             tu.ParentAssembly != t.ParentAssembly)
         where externalDeps.Any()
         select new { t, ExternalDependencies = externalDeps }
         
         // Internal coupling within namespace
         from n in Application.Namespaces 
         let internalCoupling = n.ChildTypes.Sum(t => t.TypesUsed.Count(tu => tu.ParentNamespace == n))
         where internalCoupling > 50
         select new { n, InternalCoupling = internalCoupling }
         
         // God types (used by many types)
         from t in Application.Types 
         where t.TypesUsingMe.Count() > 30
         select new { t, UsedBy = t.TypesUsingMe, Role = "God type / Hub" }
         ```
         
         ## QUERY CATEGORIES
         
         ### 1. Dependency Discovery
         - Map dependency graphs
         - Identify dependency chains
         - Find direct vs. transitive dependencies
         - Visualize relationships
         
         ### 2. Coupling Metrics
         - Calculate afferent/efferent coupling
         - Measure instability
         - Analyze abstractness
         - Compute distance from main sequence
         
         ### 3. Circular Dependencies
         - Detect direct cycles
         - Find indirect cycles
         - Identify strongly connected components
         - Break circular references
         
         ### 4. Architecture Validation
         - Enforce layer boundaries
         - Validate dependency direction
         - Check dependency inversion
         - Detect architecture violations
         
         ### 5. Stability Analysis
         - Find stable vs. unstable components
         - Identify fragile dependencies
         - Analyze change propagation
         - Assess architectural resilience
         
         ### 6. Namespace Organization
         - Analyze namespace structure
         - Detect namespace coupling
         - Find cross-cutting concerns
         - Validate namespace hierarchy
         
         ### 7. Assembly Management
         - Track third-party dependencies
         - Analyze assembly references
         - Detect unnecessary dependencies
         - Optimize assembly structure
         
         ## COMMON ANALYSIS SCENARIOS
         
         ### Scenario 1: Dependency Graph Mapping
         Understand the complete dependency structure to identify coupling hotspots.
         
         ### Scenario 2: Circular Dependency Detection
         Find and eliminate circular dependencies that harm maintainability.
         
         ### Scenario 3: Architecture Compliance
         Ensure code follows intended layered or clean architecture patterns.
         
         ### Scenario 4: Coupling Reduction
         Identify tightly coupled components that need refactoring.
         
         ### Scenario 5: Stability Assessment
         Measure component stability and identify fragile areas.
         
         ### Scenario 6: Third-Party Audit
         Track external dependencies and assess vendor lock-in risk.
         
         ### Scenario 7: Change Impact Analysis
         Understand which components are affected by changes to others.
         """;
}
