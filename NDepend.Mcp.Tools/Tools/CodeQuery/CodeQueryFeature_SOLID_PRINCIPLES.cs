namespace NDepend.Mcp.Tools.CodeQuery {
    internal partial class CodeQueryFeature {
        internal const string SOLID_PRINCIPLES_PROMPT =
     """
     # SOLID Principles Analysis with CQLinq

     ## Overview

     ### S - Single Responsibility Principle (SRP)
     **Indicators**: High method count (> 20), high field count (> 15), many type dependencies (> 20),
     classes named "Manager"/"Helper"/"Util", low cohesion, high average cyclomatic complexity.

     ### O - Open/Closed Principle (OCP)
     **Indicators**: Base class depending on derived types, type-checking with GetType(), sealed classes
     with many methods, non-sealed classes with no virtual/abstract extension points.

     ### L - Liskov Substitution Principle (LSP)
     **Indicators**: NotImplementedException in overrides, empty override bodies, overrides with
     significantly different complexity from base.

     ### I - Interface Segregation Principle (ISP)
     **Indicators**: Fat interfaces (> 10 members), NotImplementedException in interface implementations,
     types using only a subset of an implemented interface.

     ### D - Dependency Inversion Principle (DIP)
     **Indicators**: High-level modules depending on concrete low-level types instead of abstractions,
     direct instantiation of dependencies (new keyword in business logic), namespace-level layer violations.

     ## Query Patterns by Principle

     ### Single Responsibility Principle (SRP) Queries

     ```csharp
     // God classes (too many responsibilities)
     from t in Types
     where t.IsClass && (t.InstanceMethods.Count() > 20 || t.InstanceFields.Count() > 15)
     select new { t, t.InstanceMethods, t.InstanceFields, Issue = "SRP violation: Too many responsibilities" }

     // Classes with many dependencies (too many reasons to change)
     from t in JustMyCode.Types
     where t.IsClass && t.TypesUsed.Count() > 30
     let namespacesUsed = t.TypesUsed.ParentNamespaces().ToArray()
     where namespacesUsed.Length > 15
     orderby namespacesUsed.Length descending
     select new { t, t.TypesUsed, namespacesUsed, t.NbLinesOfCode, Issue = "SRP violation: Too many dependencies" }

     // Manager/Helper/Util classes (often SRP violations)
     from t in JustMyCode.Types
     where t.IsClass && t.SimpleName.EndsWithAny("Manager", "Helper", "Util")
     select new { t, Warning = "Potential SRP violation: Generic naming" }

     // Classes with low cohesion (unrelated methods)
     from t in JustMyCode.Types
     where t.IsClass && !t.IsAbstract && t.NbMethods > 10
     let methodCalls = t.Methods.SelectMany(m => m.MethodsCalled).Distinct().ToArray()
     let internalCalls = methodCalls.Where(mc => mc.ParentType == t).ToArray()
     let cohesion = (double)internalCalls.Length / t.NbMethods
     where cohesion < 0.2
     orderby cohesion descending
     select new { t, Cohesion = cohesion, Issue = "SRP violation: Low cohesion" }
     ```

     ### Open/Closed Principle (OCP) Queries

     ```csharp
     // Base class using derived classes (knows too much)
     from t in JustMyCode.Types
     where t.IsClass && t.NbChildren > 0
     let derivedClassesUsed = t.DerivedTypes.UsedBy(t).ToArray()
     where derivedClassesUsed.Any()
     orderby derivedClassesUsed.Length descending
     select new { t, derivedClassesUsed, Warning = "OCP: Base class depending on derived" }

     // Type-checking patterns instead of polymorphism
     from m in JustMyCode.Methods
     let getTypeMethods = m.MethodsCalled.Where(mc => mc.SimpleName == "GetType").ToArray()
     where getTypeMethods.Length > 0
     select new { m, getTypeMethods, Issue = "OCP violation: Type checking instead of polymorphism" }

     // Non-sealed classes with no virtual/abstract methods (no extension points)
     from t in JustMyCode.Types
     where t.IsClass && !t.IsSealed && !t.IsStatic && t.InstanceMethods.Count() > 5 &&
           !t.InstanceMethods.Any(m => m.IsVirtual || m.IsAbstract)
     select new { t, Warning = "OCP: No extension points defined" }
     ```

     ### Liskov Substitution Principle (LSP) Queries

     ```csharp
     // NotImplementedException in overrides
     from m in JustMyCode.Methods
     where m.OverriddensBase.Any() && m.CreateA("System.NotImplementedException")
     select new { m, BaseMethod = m.OverriddensBase.First(), Issue = "LSP violation: Refusing inherited behavior" }

     // Empty override implementations
     from m in JustMyCode.Methods
     where m.OverriddensBase.Any() && (m.NbLinesOfCode ?? 0) == 0
     select new { m, BaseMethod = m.OverriddensBase.First(), Issue = "LSP violation: Empty override" }

     // Overrides with significantly higher complexity than base
     from m in JustMyCode.Methods
     where m.OverriddensBase.Any() && !m.OverriddensBase.First().IsAbstract
     let baseCC = m.OverriddensBase.First().CyclomaticComplexity ?? 0
     let derivedCC = m.CyclomaticComplexity ?? 0
     where derivedCC > baseCC * 3 && derivedCC > 10
     select new { m, BaseCC = baseCC, DerivedCC = derivedCC, Issue = "LSP: Override much more complex than base" }
     ```

     ### Interface Segregation Principle (ISP) Queries

     ```csharp
     // Fat interfaces (too many members)
     from i in Types
     where i.IsInterface && i.Members.Count() > 10
     select new { i, i.Members, Issue = "ISP violation: Fat interface" }

     // Interfaces with unrelated methods (low cohesion)
     from i in Types
     where i.IsInterface && i.Members.Count() > 5
     let namespaces = i.Methods
                       .SelectMany(m => m.MethodsCalled).ParentNamespaces().Distinct().ToArray()
     where namespaces.Length > 3
     select new { i, relatedNamespaces = namespaces, Warning = "ISP: Possibly low cohesion interface" }
     ```

     ### Dependency Inversion Principle (DIP) Queries

     ```csharp
     // High-level modules depending on concrete implementations
     from t in JustMyCode.Types
     where t.IsClass
     let concreteDeps = t.TypesUsed
         .Where(tu => tu.IsClass && !tu.IsAbstract && tu.ParentAssembly != t.ParentAssembly)
         .ToArray()
     let interfaceDeps = t.TypesUsed.Where(tu => tu.IsInterface).ToArray()
     where concreteDeps.Length > interfaceDeps.Length && concreteDeps.Length > 5
     orderby concreteDeps.Length descending
     select new { t, concreteDeps, interfaceDeps, Warning = "DIP: More concrete deps than interface deps" }

     // Direct instantiation of dependencies in business logic
     from m in JustMyCode.Methods
     where m.ParentType.IsClass
     let newInstances = m.MethodsCalled.ParentTypes().Distinct()
         .Where(tu => m.CreateA(tu) && tu.IsClass && !tu.IsThirdParty && 
                      tu.ParentAssembly != m.ParentType.ParentAssembly)
         .ToArray()
     where newInstances.Length > 3
     select new { m, newInstances, Warning = "DIP: Many external concrete instantiations" }
     ```

     ## Combined SOLID Analysis

     ```csharp
     // Classes with multiple SOLID violations
     from t in JustMyCode.Types
     where t.IsClass && !t.IsAbstract
     let srpScore  = (t.InstanceMethods.Count() > 20 ? 1 : 0) + (t.TypesUsed.Count() > 30 ? 1 : 0)
     let dipScore  = t.TypesUsed.Count(tu => tu.IsClass && !tu.IsAbstract && tu.ParentAssembly != t.ParentAssembly)
     let lspScore  = t.Methods.Count(m => m.OverriddensBase.Any() && m.CreateA("System.NotImplementedException"))
     let totalRisk = srpScore + (dipScore > 5 ? 1 : 0) + (lspScore > 0 ? 1 : 0)
     where totalRisk >= 2
     orderby totalRisk descending
     select new { t, srpScore, DIPConcreteDeps = dipScore, LSPViolations = lspScore, totalRisk }
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
     """;
    }
}

