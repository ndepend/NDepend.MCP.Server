namespace NDepend.Mcp.Tools.CodeQuery {
    internal partial class CodeQueryFeature {
        internal const string CONSTRUCTOR_INSTANTIATION_PROMPT =
        """
        # Constructor Instantiation Analysis (CQLinq)

        ## Constructor APIs

        ### IMethod Interface (for Constructors)
        Constructors are IMethods objects with special properties.
        
        ```csharp
        // IMethod — key constructor properties
        bool IsConstructor { get; }         // Instance constructor (.ctor)
        bool IsClassConstructor { get; }    // Static constructor (.cctor)
        int NbParameters { get; }
        IEnumerable<IMethod> MethodsCalled { get; }
        IEnumerable<IMethod> MethodsCallingMe { get; }
        bool CreateA(IType typeCreated)     // true if this method calls a constructor of typeCreated

        // IType — constructor access
        IEnumerable<IMethod> Constructors { get; }

        // Extension Methods
        bool CreateA(this IMethod method, string createdTypeFullName)
        ushort? DepthOfCreateA(this IMethod method, string createdTypeFullName)
        bool IndirectlyCreateA(this IMethod method, string createdTypeFullName)
        ```
        
        ## Common Query Patterns

        ### Constructor Access
        
        ```csharp
        // Filter all constructors
        IEnumerable<IMethod> allConstructors = codeBase.Methods
            .Where(m => m.IsConstructor);
        
        // Constructors of a specific type
        IType type = codeBase.Types.WithFullName("MyApp.UserService").Single();
        IEnumerable<IMethod> typeConstructors = type.Constructors;
        IMethod defaultConstructor = type.Constructors.FirstOrDefault(c => c.NbParameters == 0);
        
        // Static constructors (also named Class Constructor)
        IEnumerable<IMethod> staticConstructors = codeBase.Methods.Where(m => m.IsClassConstructor);
        ```
        
        ### Dependency Tracking
        
        ```csharp
        // Types a constructor depends on (creates or receives)
        IMethod constructor;
        var createdTypes = constructor.MethodsCalled.Where(m => m.IsConstructor).ParentTypes(); // new Xyz()
        var calledTypes = constructor.MethodsCalled.ParentTypes()   // Used types
        
        // Who instantiates a type
        IType type;
        var instantiators = type.Constructors.SelectMany(c => c.MethodsCallingMe).Distinct();  // Methods that call 'new Type()'
        ```
        
        ### Singleton Class Detection
        
        ```csharp
        from t in Application.Types
        where !t.IsStatic &&
               t.Constructors.Count() == 1 &&
               t.Constructors.Single().IsPrivate
        let staticProc = t.Properties.FirstOrDefault(p => p.IsReadOnly && p.IsStatic && p.Name == "Instance")
        where staticProc != null
        select new { t, staticProc }
        ```

        ### Service Classes Instantiated Directly (DI Violation)
        ```csharp
        warnif count > 0
        from t in Application.Types
        where t.IsClass &&
              t.SimpleName.EndsWithAny("Service", "Repository", "Manager", "Controller")
        let directInstantiators =
            t.Constructors.SelectMany(c => c.MethodsCallingMe).Distinct()
             .Where(m => !m.ParentType.SimpleName.ContainsAny("Factory", "Builder") &&
                         !m.SimpleName.Contains("CreateInstance")).ToArray()
        where directInstantiators.Any()
        select new { t, directInstantiators }
        ```

        ### God Constructors (Too Many Parameters)
        ```csharp
        warnif count > 0
        from m in Methods
        where m.IsConstructor && m.NbParameters > 5
        orderby m.NbParameters descending
        select new { m, m.ParentType }
        ```

        ### Constructors Creating Their Own Dependencies
        ```csharp
        warnif count > 0
        from m in Application.Methods
        where m.IsConstructor
        let instantiatedTypes = m.MethodsCalled.Where(
                   c => c.IsConstructor && !c.IsThirdParty)
                   .Select(c => c.ParentType).Distinct().ToArray()
        where instantiatedTypes.Any()
        select new { m, m.ParentType, instantiatedTypes }
        ```

        ### Complex Constructors
        ```csharp
        warnif count > 0
        from m in Methods
        where m.IsConstructor &&
              (m.CyclomaticComplexity > 5 || m.NbLinesOfCode > 20)
        select new { m, m.CyclomaticComplexity, m.NbLinesOfCode, m.MethodsCalled }
        ```

        ### Object Construction Call Graph
        ```csharp
        from m in Methods
        let depth0 = m.DepthOfCreateA("MyProduct.Type")
        where depth0 >= 0
        orderby depth0
        select new { m, depth0 }
        ```

        ### Instantiation Hotspots
        ```csharp
        from t in Types
        where t.IsClass
        let totalInstantiations =
           t.Constructors.SelectMany(m => m.MethodsCallingMe).Distinct().ToArray()
        where totalInstantiations.Length > 20
        orderby totalInstantiations.Length descending
        select new { t, totalInstantiations }
        ```
        
        ## Anti-Patterns

        1. **God Constructor** - Too many dependencies (> 5 parameters)
        2. **Hidden Dependencies** - Creating dependencies inside constructor
        3. **Constructor Over-Work** - Complex logic in constructor
        4. **Singleton Abuse** - Static constructors creating singletons
        5. **Circular Dependencies** - A creates B, B creates A
        """;
    }
}