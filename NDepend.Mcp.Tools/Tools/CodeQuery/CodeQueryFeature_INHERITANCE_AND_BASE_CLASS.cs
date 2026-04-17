namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string INHERITANCE_AND_BASE_CLASS_PROMPT =
         """
         # Base class and inheritance hierarchy

         ## IType APIs
         ```csharp
         bool IsAbstract { get; }               // abstract class or interface
         bool IsSealed { get; }                 // sealed class
         IType BaseClass { get; }               // direct base class, null if none
         IEnumerable<IType> BaseClasses { get; }        // all ancestor classes
         IEnumerable<IType> DerivedTypes { get; }       // all derived types (recursive)
         IEnumerable<IType> DirectDerivedTypes { get; } // direct derived types only
         ushort? NbChildren { get; }                    // derived class count / implementor count
         bool DeriveFrom(IType baseClass)               // true if derives directly or indirectly
         bool DeriveFrom("full name")
         ushort? DepthOfDeriveFrom(IType baseClass)     // null if not derived
         ushort? DepthOfDeriveFrom("full name")
         ```

         ## IMethod / IProperty / IEvent APIs
         ```csharp
         bool IsAbstract { get; }
         bool IsVirtual { get; }
         bool IsNewSlot { get; }
         bool IsFinal { get; }                                      // sealed override
         IEnumerable<IMethod> OverriddensBase { get; }              // base methods overridden by this (same for IProperty/IEvent)
         IEnumerable<IMethod> OverridesDerived { get; }             // derived overrides of this
         IEnumerable<IMethod> OverridesDirectDerived { get; }       // direct-derived overrides only
         ```

         ## Query Patterns

         ### Basic
         ```csharp
         from t in Types where t.IsAbstract && t.IsClass select new { t, t.DerivedTypes }
         from t in Types where t.IsClass && t.BaseClass != null && t.BaseClass.Name != "Object" select new { t, t.BaseClass }
         from t in Types where t.DeriveFrom("My.Namespace.BaseClass") select t
         from t in Types where t.IsClass && t.NbChildren >= 5 select new { t, t.DerivedTypes }
         ```

         ### Depth Analysis
         ```csharp
         from t in Types where t.DepthOfInheritance > 5
         orderby t.DepthOfInheritance descending
         select new { t, t.DepthOfInheritance, Chain = t.BaseClasses }

         let avgDepth = Types.Where(t => t.IsClass).Average(t => t.DepthOfInheritance)
         from t in Types where t.IsClass && t.DepthOfInheritance > avgDepth
         select new { t, t.DepthOfInheritance, Average = avgDepth }
         ```

         ### Abstract / Virtual / Override
         ```csharp
         // Abstract with no derived
         from t in Types where t.IsAbstract && t.IsClass && t.NbChildren == 0 select t

         // Abstract classes with many abstract methods
         from t in Types where t.IsAbstract && t.IsClass
         let abstractMethods = t.Methods.Where(m => m.IsAbstract).ToArray()
         where abstractMethods.Any() orderby abstractMethods.Length descending
         select new { t, abstractMethods }

         // Abstract methods with many overrides
         from m in Methods where m.IsAbstract
         let overrides = m.OverridesDerived.ToArray()
         where overrides.Length >= 5 orderby overrides.Length descending
         select new { m, m.ParentType, overrides }

         // Virtual never overridden
         from m in Methods where m.IsVirtual && !m.IsFinal && !m.OverridesDerived.Any() select m

         // Overrides that call base
         from m in Methods
         let baseMethods = m.OverriddensBase.ToArray()
         where baseMethods.Length > 0
         let baseMethodsCalled = m.MethodsCalled.Intersect(baseMethods).ToArray()
         orderby baseMethodsCalled.Length descending
         select new { m, baseMethodsCalled }
         ```

         ### Sealed
         ```csharp
         // Candidates to seal (non-public, no children)
         from t in Types where t.IsClass && !t.IsSealed && !t.IsAbstract && t.NbChildren == 0 && !t.IsPubliclyVisible select t

         // Virtual in sealed (unnecessary)
         from m in Methods where m.IsVirtual && m.ParentType.IsSealed select new { m, SealedParent = m.ParentType }
         ```

         ### Fragile Base / Polymorphism / Hierarchy Map
         ```csharp
         // Fragile base: many children + virtual methods
         from t in Types where t.IsClass && t.NbChildren > 10 && t.Methods.Any(m => m.IsVirtual)
         select new { t, Child = t.DerivedTypes, VirtualMethods = t.Methods.Count(m => m.IsVirtual) }

         // Methods using polymorphic calls
         from m in Methods
         let pCalls = m.MethodsCalled.Where(mc => mc.IsVirtual || mc.IsAbstract).ToArray()
         where pCalls.Any() orderby pCalls.Length descending
         select new { m, PolymorphicCalls = pCalls }

         // Full hierarchy map
         from t in Types where t.IsClass && t.FullName != "System.Object"
         orderby t.DepthOfInheritance descending
         select new { t, Base = t.BaseClass, Depth = t.DepthOfInheritance, Children = t.NbChildren }
         ```

         ## Best Practices and Anti-Patterns
         - DOI <= 5; prefer composition (has-a) over inheritance (is-a); LSP: derived must substitute base
         - Seal classes not designed for inheritance; make methods virtual only by explicit design
         - Abstract classes should define clear contracts; document inheritance expectations
         - Limit public/protected surface in base classes to reduce fragile base risk
         - Anti-patterns: deep hierarchy (DOI > 6), God Object Hierarchy, Yo-Yo Problem, Refused Bequest, Virtual Everything, empty abstract classes
         """;
}
