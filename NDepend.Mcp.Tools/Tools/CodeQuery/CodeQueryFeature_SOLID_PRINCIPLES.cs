namespace NDepend.Mcp.Tools.CodeQuery {
    internal partial class CodeQueryFeature {
        internal const string SOLID_PRINCIPLES_PROMPT =
     """
     # SOLID PRINCIPLES ANALYSIS WITH CQLINQ
     
     You are an expert in NDepend.API and CQLinq (Code Query LINQ). Your task is to generate accurate, efficient CQLinq queries that analyze adherence to SOLID principles in .NET codebases and identify violations of these fundamental object-oriented design principles.
     
     ## CONTEXT
     NDepend.API provides powerful querying capabilities to analyze .NET code structure and quality. This prompt focuses specifically on detecting violations and measuring compliance with the five SOLID principles: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion.
     
     ## SOLID PRINCIPLES OVERVIEW
     
     ### S - Single Responsibility Principle (SRP)
     **Principle**: A class should have one, and only one, reason to change.
     
     **Indicators of Violations**:
     - High number of methods (> 20)
     - High number of fields (> 15)
     - Many dependencies (> 20)
     - Multiple unrelated responsibilities (classes used are declared in multiple unrelated namespaces)
     - High cyclomatic complexity across methods
     - Names like "Manager", "Helper", "Util"
     
     **Detection Strategies**:
     - Count methods, fields, dependencies
     - Analyze method cohesion
     - Detect god classes
     - Identify lack of focused responsibility
     
     ### O - Open/Closed Principle (OCP)
     **Principle**: Software entities should be open for extension but closed for modification.
     
     **Indicators of Violations**:
     - Switch statements on type codes
     - Conditional logic based on type checking
     - Lack of polymorphism
     - Concrete dependencies instead of abstractions
     - No virtual/abstract methods for extension points
     
     **Detection Strategies**:
     - High cyclomatic complexity across methods
     - Identify sealed classes that should be extensible
     - Find missing abstraction opportunities
     
     ### L - Liskov Substitution Principle (LSP)
     **Principle**: Derived classes must be substitutable for their base classes.
     
     **Indicators of Violations**:
     - Overrides that throw NotImplementedException
     - Overrides that weaken preconditions or strengthen postconditions
     - Empty override implementations
     - Refusing inherited behavior
     - Changing expected behavior in derived classes
     
     **Detection Strategies**:
     - Find NotImplementedException in overrides
     - Detect empty override methods
     - Identify overrides with significantly different complexity
     - Find base class methods not used by derived classes
     
     ### I - Interface Segregation Principle (ISP)
     **Principle**: Clients should not be forced to depend on interfaces they don’t use.
     
     **Indicators of Violations**:
     - Large interfaces (> 10 members)
     - Implementations with empty/stub methods
     - Implementations using only subset of interface
     - NotImplementedException in interface implementations
     - Fat interfaces with unrelated methods
     
     **Detection Strategies**:
     - Count interface members
     - Find partial interface implementations
     - Detect NotImplementedException in interface methods
     - Identify interfaces with low cohesion
     
     ### D - Dependency Inversion Principle (DIP)
     **Principle**: High-level modules should not depend on low-level modules. Both should depend on abstractions.
     
     **Indicators of Violations**:
     - Business logic depending on infrastructure/data
     - Concrete class dependencies instead of interfaces
     - UI depending directly on database
     - No abstraction layers
     - Direct instantiation of dependencies (new keyword)
     
     **Detection Strategies**:
     - Find concrete dependencies in high-level modules
     - Detect layer violations
     - Identify missing abstractions
     - Find new keyword usage in business logic
     
     ## QUERY PATTERNS BY PRINCIPLE
     
     ### Single Responsibility Principle (SRP) Queries
     
     ```csharp
     // God classes (too many responsibilities)
     from t in Types 
     where t.IsClass && 
           (t.InstanceMethods.Count() > 20 || t.InstanceFields.Count() > 15)
     select new { t, 
        t.InstanceMethods, 
        t.InstanceFields, 
        Issue = "SRP violation: Too many responsibilities" }
          
     // Classes with high complexity (doing too much)
     from t in JustMyCode.Types 
     where t.IsClass
     let methodWithCC = t.Methods.Where(m => m.CyclomaticComplexity > 0)
     where methodWithCC.Any()
     let avgComplexity = 
           methodWithCC 
          .Average(m => m.CyclomaticComplexity ?? 0)
     where avgComplexity > 10
     select new { 
        t, 
        AvgComplexity = avgComplexity, 
        Issue = "SRP violation: High complexity"
     }
          
     // Classes with many dependencies (too many reasons to change)
     from t in JustMyCode.Types 
     where t.IsClass && 
        t.TypesUsed.Count() > 30
     let namespacesUsed = t.TypesUsed.ParentNamespaces().ToArray()
     where namespacesUsed.Length > 15  
     orderby namespacesUsed.Length descending
     select new { 
        t, 
        t.TypesUsed, 
        namespacesUsed,
        t.NbLinesOfCode,
        Issue = "SRP violation: Too many dependencies"
     }
          
     // Manager/Helper/Util classes (often SRP violations)
     from t in JustMyCode.Types
     where t.IsClass &&
           t.SimpleName.EndsWithAny("Manager", "Helper", "Util")
     select new { t, Warning = "Potential SRP violation: Generic naming" }
     
     // Classes with low cohesion (unrelated methods)
     from t in JustMyCode.Types 
     where t.IsClass && !t.IsAbstract && t.NbMethods > 10
     let methodCalls = t.Methods.SelectMany(m => m.MethodsCalled).Distinct().ToArray()
     let internalCalls = methodCalls.Where(mc => mc.ParentType == t).ToArray()
     let cohesion = (double)internalCalls.Length / t.NbMethods
     where cohesion < 0.2
     orderby cohesion descending
     select new { t, 
        Cohesion = cohesion, 
        methodCalls,
        internalCalls,
        Issue = "SRP violation: Low cohesion" }
     ```
     
     ### Open/Closed Principle (OCP) Queries
     
     ```csharp
     // Base class using derived classes
     from t in JustMyCode.Types 
     where t.IsClass && t.NbChildren > 0 
     let derivedClassesUsed = t.DerivedTypes.UsedBy(t).ToArray()
     where derivedClassesUsed.Any()
     orderby derivedClassesUsed.Length descending
     select new { t, derivedClassesUsed, Warning = "OCP: Base class knowing about derived" }
     
     // Type checking patterns (GetType() calls)
     from m in JustMyCode.Methods 
     let getTypeMethods = m.MethodsCalled.Where(mc => mc.SimpleName == "GetType").ToArray()
     where getTypeMethods.Length > 0
     select new { m, getTypeMethods, Issue = "OCP violation: Type checking instead of polymorphism" }
     
     // Sealed classes with derived types needed
     from t in JustMyCode.Types 
     where t.IsSealed && t.IsClass && !t.IsStatic &&
           t.InstanceMethods.Count() > 20
     orderby t.InstanceMethods.Count() descending
     select new { t, t.InstanceMethods, Warning = "OCP: Sealed class may need extension" }
     
     // Non-Sealed Classes without virtual methods (not extensible)
     from t in JustMyCode.Types 
     where t.IsClass && !t.IsSealed && !t.IsStatic &&
           t.InstanceMethods.Count() > 5 &&
           !t.InstanceMethods.Any(m => m.IsVirtual || m.IsAbstract)
     select new { t, Warning = "OCP: No extension points" }
     ```
     
     ### Liskov Substitution Principle (LSP) Queries
     
     ```csharp
     // NotImplementedException in overrides
     from m in JustMyCode.Methods 
     where m.OverriddensBase.Any() && 
           m.CreateA("System.NotImplementedException")
     select new { m, BaseMethod = m.OverriddensBase.First(), Issue = "LSP violation: Refusing inherited behavior" }
     
     // Empty override implementations
     from m in JustMyCode.Methods 
     where m.OverriddensBase.Any() && 
           m.NbLinesOfCode == 0
     select new { m, BaseMethod = m.OverriddensBase.First(), Issue = "LSP violation: Refusing to implement behavior" }
     
     // Overrides with significantly different complexity
     from m in JustMyCode.Methods 
     let baseMethod = m.OverriddensBase.FirstOrDefault()
     where baseMethod != null && !baseMethod.IsAbstract
     let complexityDiff = Math.Abs((m.CyclomaticComplexity ?? 0) - (baseMethod.CyclomaticComplexity ?? 0))
     where complexityDiff > 10
     select new { m, BaseMethod = baseMethod, ComplexityDiff = complexityDiff, Warning = "LSP: Behavior divergence" }
     
     // Overrides not calling base when they should
     from m in JustMyCode.Methods 
     let baseMethod = m.OverriddensBase.FirstOrDefault()
     where baseMethod != null && !baseMethod.IsAbstract &&
           !m.MethodsCalled.Any(mc => mc.ParentType == baseMethod.Parent)
     select new { m, baseMethod, baseMethod.Parent, Warning = "LSP: Override doesn't call base implementation" }
     ```
     
     ### Interface Segregation Principle (ISP) Queries
     
     ```csharp
     // Fat interfaces (too many members)
     from i in JustMyCode.Types 
     where i.IsInterface
     let methods = i.Methods.Where(m => m.ParentProperty == null).ToArray()
     where methods.Length + i.Properties.Count() > 15
     select new { Interface = i, methods, i.Properties, Issue = "ISP violation: Fat interface" }
     
     // NotImplementedException in interface implementations
     from t in JustMyCode.Types 
     where t.InterfacesImplemented.Any()
     from m in t.InstanceMethods
     where m.OverriddensBase.Where(mc => mc.ParentType.IsInterface).Any()
        && m.CreateA("System.NotImplementedException")
     select new { m, Issue = "ISP violation: Forced implementation" }
     
     // Types implementing interface but not using all methods
     from t in JustMyCode.Types
     where t.IsClass
     from i in t.InterfacesImplemented
     where i.NbMethods > 5
     let interfaceMethods = i.Methods.ToHashSetEx()
     let usedMethods = t.Methods.SelectMany(mc => mc.MethodsCalled.Intersect(interfaceMethods)).Distinct().ToArray()
     where usedMethods.Length < i.NbMethods * 0.2
     select new { t, i, interfaceMethods, usedMethods, Issue = "ISP violation: Partial interface usage" }
     
     // Interfaces with unrelated methods (low cohesion)
     from i in JustMyCode.Types 
     where i.IsInterface && i.NbMethods > 5
     let methodNames = i.Methods.Select(m => m.SimpleName).ToList()
     let prefixes = methodNames.Select(n => n.Substring(0, Math.Min(3, n.Length))) 
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
     where prefixes.Length > 10
     select new { i, prefixes = prefixes.Aggregate(", "), Issue = "ISP violation: Interface with unrelated methods" }
     
     // Empty/stub implementations
     from m in JustMyCode.Methods 
     where m.ParentEvent == null
     let baseMethod = m.OverriddensBase.FirstOrDefault(mb => mb.ParentType.IsInterface)
     where baseMethod != null && m.NbLinesOfCode == 0
     select new { m, baseMethod.ParentType, Warning = "ISP: Stub implementation" }
     ```
     
     ### Dependency Inversion Principle (DIP) Queries
     
     ```csharp
     // High-level depending on low-level (concrete implementations)
     let lowLevel = JustMyCode.Types
        .Where(t => !t.IsInterface && 
                    t.ParentNamespace.Name.ContainsAny(".Data", ".Infrastructure"))
        .ToHashSetEx()
     let highLevel = JustMyCode.Types
        .Where(t => t.ParentNamespace.Name.ContainsAny(".Business", ".Domain"))
     from t in highLevel.UsingAny(lowLevel)
     let used = t.TypesUsed.Intersect(lowLevel)
     select new { t, used, Issue = "DIP violation: Depending on concrete low-level module" }
  
     // Direct instantiation in business logic (new keyword)
     let bizType = JustMyCode.Types
        .Where(t => t.ParentNamespace.Name.ContainsAny(".Business", ".Domain"))
     from t in bizType 
     let createdTypes = t.Methods
        .SelectMany(m => m.MethodsCalled.Where(mc => mc.IsConstructor))
        .ParentTypes().ToArray()
     where createdTypes.Any()
     select new { t, createdTypes, 
        Issue = "DIP violation: Direct instantiation in business logic" }
     
     // Fields typed as concrete classes instead of interfaces
     from t in JustMyCode.Types
               .Where(t => t.ParentNamespace.Name.ContainsAny(".Business", ".Domain"))
     from f in t.Fields
     let fieldType = f.FieldType
     where fieldType != null &&
           !fieldType.IsInterface && 
           !fieldType.IsAbstract && 
           !fieldType.IsStructure &&
           !fieldType.IsEnumeration &&
           !fieldType.IsThirdParty
     select new { f, fieldType , Issue = "DIP violation: Field typed as concrete class" }
     
     // Dependencies flowing wrong direction (low-level to high-level is OK)
     let lowLevel = JustMyCode.Types
        .Where(t => !t.IsInterface && 
                    t.ParentNamespace.Name.ContainsAny(".Data", ".Infrastructure"))
     let highLevel = JustMyCode.Types
        .Where(t => t.ParentNamespace.Name.ContainsAny(".Business", ".Domain"))
        .ToHashSetEx()
     from t in lowLevel.UsingAny(highLevel)
     let used = t.TypesUsed.Intersect(highLevel)
     select new { t, used, Issue = "DIP violation: Low-level depending on high-level (should use abstraction)" }
     
     // Ratio of abstract to concrete dependencies
     from t in JustMyCode.Types 
     where t.IsClass && t.TypesUsed.Count() > 5
     let abstractDeps = t.TypesUsed.Where(tu => tu.IsInterface || tu.IsAbstract).ToArray()
     let concreteDeps = t.TypesUsed.Where(tu => !tu.IsInterface && !tu.IsAbstract && !tu.IsStructure && !tu.IsEnumeration).ToArray()
     let dipRatio = (abstractDeps.Length + concreteDeps.Length) > 0 ? (double)abstractDeps.Length / (abstractDeps.Length + concreteDeps.Length) : 0
     where dipRatio < 0.3
     select new { t, abstractDeps, concreteDeps, DIPRatio = dipRatio, Issue = "DIP: Low abstraction ratio" }
     ```
     
     ## Combined SOLID Analysis
     
     ### Comprehensive SOLID Scorecard
     ```csharp
     // Generate SOLID compliance score for each type
     from t in JustMyCode.Types 
     where (t.IsClass && !t.IsStatic) || t.IsInterface
          
     // SRP Score
     let srpScore = (t.NbMethods <= 20 && t.NbFields <= 15 && t.TypesUsed.Count() <= 20) ? 1 : 0
          
     // OCP Score
     let ocpScore = (t.Methods.Any(m => m.IsVirtual || m.IsAbstract) || t.IsAbstract) ? 1 : 0
          
     // LSP Score (no NotImplementedException)
     let lspScore = t.Methods.Where(m => m.OverriddensBase.Any()).Any(m => 
         !m.CreateA("System.NotImplementedException")) ? 1 : 0
          
     // ISP Score (interfaces have reasonable size)
     let ispScore = t.IsInterface && t.NbMethods <= 10 ? 1 : 0
          
     // DIP Score (depends on abstractions)
     let abstractDeps = t.TypesUsed.Count(tu => tu.IsInterface || tu.IsAbstract)
     let totalDeps = t.TypesUsed.Count()
     let dipScore = totalDeps > 0 && (double)abstractDeps / totalDeps >= 0.5 ? 1 : 0
      
     let solidScore = srpScore + ocpScore + lspScore + ispScore + dipScore
     orderby solidScore descending
     select new { 
         Type = t, 
         SRPScore = srpScore,
         OCPScore = ocpScore,
         LSPScore = lspScore,
         ISPScore = ispScore,
         DIPScore = dipScore,
         TotalSOLIDScore = solidScore,
         //Percentage = (solidScore * 20) + "%"
     }
     ```
     
     ## RESPONSE FORMAT
     
     When generating queries or rules, provide:
     
     1. **Query Purpose**: Which SOLID principle is being validated
     2. **CQLinq Code**: The actual query with proper syntax
     3. **Explanation**: How the query detects violations
     4. **Principle Background**: Brief explanation of the SOLID principle
     5. **Violation Impact**: Why this violation matters
     6. **Refactoring Guidance**: How to fix violations
     7. **Examples**: Before/after code examples when helpful
     8. **Thresholds**: Recommended limits for metrics

     ## COMMON ANALYSIS SCENARIOS
     
     ### Scenario 1: SOLID Compliance Audit
     Generate a comprehensive report showing SOLID principle adherence across the codebase.
     
     ### Scenario 2: Refactoring Prioritization
     Identify the most critical SOLID violations that need immediate attention.
     
     ### Scenario 3: Architecture Review
     Validate that architectural decisions align with SOLID principles.
     
     ### Scenario 4: Code Review Guidelines
     Generate SOLID-based code review checklist items.
     
     ### Scenario 5: Technical Debt Assessment
     Quantify SOLID violations as technical debt metrics.
     
     ### Scenario 6: Design Pattern Validation
     Ensure design patterns are properly implemented per SOLID.
     
     ### Scenario 7: Training and Education
     Identify examples of violations to teach SOLID principles.
     
     ## BEST PRACTICES AND THRESHOLDS
     
     ### SRP Thresholds
     - Methods per class: ≤ 20
     - Fields per class: ≤ 15
     - Dependencies per class: ≤ 20
     - Average cyclomatic complexity: ≤ 5
     
     ### OCP Indicators
     - At least one virtual/abstract method for extensible classes
     - Avoid switch statements on type codes
     - Prefer polymorphism over conditionals
     
     ### LSP Requirements
     - No NotImplementedException in overrides
     - Overrides maintain base method contracts
     - Derived classes don’t weaken postconditions
     
     ### ISP Thresholds
     - Interface methods: ≤ 10
     - Interface members (methods + properties): ≤ 15
     - No forced stub implementations
     
     ### DIP Targets
     - Abstraction ratio: ≥ 50% (interfaces/abstractions vs concrete)
     - No business logic depending on infrastructure
     - Constructor injection of abstractions
     """;
    }
}

