namespace NDepend.Mcp.Tools.CodeQuery {
    internal partial class CodeQueryFeature {
        internal const string CONSTRUCTOR_INSTANTIATION_PROMPT =
        """
        # CONSTRUCTOR INSTANTIATION ANALYSIS PROMPT (CQLinq)
        
        ## OBJECTIVE
        Write CQLinq queries and use NDepend.API to analyze object instantiation patterns, detect tight coupling, identify dependency injection violations, and find performance issues related to object creation.
        
        ## APIS
        
        ### IMethod Interface (for Constructors)
        Constructors are IMethods objects with special properties.
        
        ```csharp
        interface IMethod : IMember {
            // Constructor identification
            bool IsConstructor { get; }              // Instance constructor (.ctor)
            bool IsClassConstructor { get; }         // Class constructor (.cctor)
            
            // Method calls
            IEnumerable<IMethod> MethodsCalled { get; }      // Methods called by this constructor
            IEnumerable<IMethod> MethodsCallingMe { get; }   // Who calls this constructor
            
            bool CreateA(IType typeCreated)  // Gets a value indicating whether this method is calling a constructor of the type typeCreated
            
            // Parameters
            int NbParameters { get; }
        }
        ```
        
        ### EXTENSION METHODS FOR OBJECT CREATION
        
        ```csharp
        // Returns true if method calls a constructor of a type whose FullName equals createdTypeFullName.
        bool CreateA(this IMethod method, string createdTypeFullName);
        
        // Returns a non-null depth value if method creates, directly or indirectly,
        // an instance of a type whose FullName equals createdTypeFullName.
        ushort? DepthOfCreateA(this IMethod method, string createdTypeFullName);
        
        // Returns true if method calls, directly or indirectly,
        // a constructor of a type whose FullName equals createdTypeFullName.
        bool IndirectlyCreateA(this IMethod method, string createdTypeFullName);
        ```
        
        ## COMMON QUERY PATTERNS
        
        ### CONSTRUCTOR ACCESS
        
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
        
        ### DEPENDENCY TRACKING
        
        ```csharp
        // Types a constructor depends on (creates or receives)
        IMethod constructor;
        var createdTypes = constructor.MethodsCalled.Where(m => m.IsConstructor).ParentTypes(); // new Xyz()
        var calledTypes = constructor.MethodsCalled.ParentTypes()   // Used types
        
        // Who instantiates a type
        IType type;
        var instantiators = type.Constructors.SelectMany(c => c.MethodsCallingMe).Distinct();  // Methods that call 'new Type()'
        ```
        
        ### SINGLETON CLASS DETECTION
        
        ```csharp
        // Lists Singleton classes (anti-pattern).
        // Singleton behaves like a global variable and reduces testability.
        from t in Application.Types
        where !t.IsStatic && 
               t.Constructors.Count() == 1 &&
               t.Constructors.Single().IsPrivate 
        let staticProc = t.Properties.FirstOrDefault(p => p.IsReadOnly && p.IsStatic && p.Name == "Instance")
        where staticProc != null
        select new { 
           t,
           staticProc 
        }
        ```
        
        ### PATTERN 1: SERVICE CLASSES INSTANTIATED DIRECTLY (DI VIOLATION)
        
        ```csharp
        // Find service/repository classes created with 'new'
        warnif count > 0
                
        from t in Application.Types
        where t.IsClass &&
              t.SimpleName.EndsWithAny("Service", "Repository", "Manager", "Controller")        
        
        // Who creates these services directly
        let directInstantiators = 
            t.Constructors.SelectMany(c => c.MethodsCallingMe).Distinct()
             .Where(m => !m.ParentType.SimpleName.ContainsAny("Factory", "Builder") &&
                         !m.SimpleName.Contains("CreateInstance")).ToArray()
                
        where directInstantiators.Any()
                
        select new {
           t,
           directInstantiators 
        }
        ```
        
        ### PATTERN 2: GOD CONSTRUCTORS (TOO MANY PARAMETERS)
        
        ```csharp
        // Constructors with too many parameters
        warnif count > 0
                
        from m in Methods
        where m.IsConstructor && 
              m.NbParameters > 5
        orderby m.NbParameters descending
        select new {
            m,
            m.ParentType
        }
        ```
        
        ### PATTERN 3: CONSTRUCTORS CREATING DEPENDENCIES (HIDDEN DEPENDENCIES)
        
        ```csharp
        // Constructors that create their own dependencies instead of receiving them
        // Use dependency injection - pass dependencies as constructor parameters
        warnif count > 0
             
        from m in Application.Methods
        where m.IsConstructor
             
        // Types instantiated in constructor
        let instantiatedTypes = m.MethodsCalled.Where(
                   c => c.IsConstructor && 
                       !c.IsThirdParty)   // Ignore framework types
                   .Select(c => c.ParentType)
                   .Distinct()
                   .ToArray()
        where instantiatedTypes.Any()
             
        select new {
            m,
            m.ParentType,
            instantiatedTypes 
        }
        
        ### PATTERN 4: COMPLEX CONSTRUCTORS (DOING TOO MUCH)
        ```csharp
        // Constructors with high complexity
        // Move logic to Initialize() method or factory
        warnif count > 0
           
        from m in Methods
        where m.IsConstructor &&
              (m.CyclomaticComplexity > 5 || m.NbLinesOfCode > 20)
           
        select new {
            m,
            m.CyclomaticComplexity,
            m.NbLinesOfCode,
            m.MethodsCalled
        }
        ```
        
        ### PATTERN 5: OBJECT CONSTRUCTION CALL GRAPH
        ```csharp
        from m in Methods 
        let depth0 = m.DepthOfCreateA("MyProduct.Type")
        where depth0  >= 0 // or == 1 for direct creation only
        orderby depth0   
        select new { m, depth0 }
        ```
        
        ### PATTERN 6: INSTANTIATION HOTSPOTS
        
        ```csharp
        from t in Types
        where t.IsClass
                
        let totalInstantiations = 
           t.Constructors
              .SelectMany(m => m.MethodsCallingMe)
              .Distinct().ToArray()
        
        where totalInstantiations.Length > 20
        orderby totalInstantiations.Length descending
        select new { t, totalInstantiations }
        ```
        
        ## ANTI-PATTERNS
        
        1. **God Constructor** - Too many dependencies (>5 parameters)
        2. **Hidden Dependencies** - Creating dependencies inside constructor
        3. **Constructor Over-Work** - Complex logic in constructor
        4. **Singleton Abuse** - Static constructors creating singletons
        5. **Circular Dependencies** - A creates B, B creates A
        
        ## CONSTRUCTOR DESIGN GUIDELINES
        1. ✅ **Keep constructors simple** - Assignment only, minimal logic
        2. ✅ **Inject dependencies** - Don’t create them
        3. ✅ **Limit parameters** - Max 3-5 parameters
        4. ✅ **Use constructor chaining** - Avoid duplicate code
        5. ✅ **Validate parameters** - Fail fast with clear exceptions
        6. ❌ **Avoid complex logic** - No calculations, API calls, I/O
        7. ❌ **Avoid static dependencies** - Makes testing hard
        """;
    }
}