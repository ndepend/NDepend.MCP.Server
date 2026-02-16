namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string INHERITANCE_AND_BASE_CLASS_PROMPT =
         """
         # BASE CLASS AND INHERITANCE HIERARCHIES
         
         Your task is to generate accurate, efficient CQLinq queries that analyze base class, inheritance hierarchies, inheritance relationships, polymorphism patterns, and object-oriented design in .NET codebases.
         
         ## APIS
         
         The NDepend.API interface 'IType' presents these properties and methods related to base class, derived class and inheritance:
          
          ```csharp
         bool IsAbstract { get; } // Gets a value indicating whether this type is an abstract class or an interface.
         bool IsSealed { get; } // Gets a value indicating whether this type is a sealed class.
         IEnumerable<IType> DerivedTypes { get; } // Gets a sequence of all types derived from this type. The sequence is empty if this type has no derived type.
         IEnumerable<IType> DirectDerivedTypes { get; } // Gets a sequence of types that derives directly from this type. The sequence is empty if this type has no derived type.
         ushort? NbChildren { get; } // Gets a numeric nullable value counting this class's number of derived classes, or this interface number of implementer types.
         ushort? DepthOfDeriveFrom(IType baseClass) // Returns a nullable numeric value equals to this type depth of inheritance, relative to 'baseClass'. Returns 'null' if this type doesn't derive from 'baseClass'.
         bool DeriveFrom(IType baseClass) // Returns a value indicating whether this type derives 'directly or indirectly' from 'baseClass'.
         IType BaseClass { get; } // Gets the IType object representing the base class of this class, if any, otherwise gets 'null'.
         IEnumerable<IType> BaseClasses { get; } // Gets a sequence of all base classes of this class. The sequence is empty if this type doesn't have any base class.
         ushort? DepthOfDeriveFrom("base class full name") // Gets a nullable numeric value equals to this type depth of inheritance. Returns 'null' if this type has no base class.
         ```
         
         The NDepend.API interfaces 'IMethod', 'IProperty', 'IEvent' present these properties and methods related to inheritance:
         ```csharp
         bool IsAbstract { get; } // Gets a value indicating whether this method is an abstract method, or a method declared on an interface.
         IEnumerable<IMethod> OverriddensBase { get; } // Gets a sequence of methods declared in this method’ 'ParentType.BaseClasses' and 'ParentType.InterfacesImplemented', overridden by this method. If this method doesn't override any method, gets an empty sequence. (same for IProperty and IEvent)
         IEnumerable<IMethod> OverridesDerived { get; } // Gets a sequence of methods declared in this method 'ParentType.DerivedTypes', overriding this method. If this method is not overridden, gets an empty sequence.
         IEnumerable<IMethod> OverridesDirectDerived { get; } // Gets a sequence of methods declared in this method 'ParentType.DirectDerivedTypes', overriding this method. If this method is not overridden, gets an empty sequence.
         bool IsVirtual { get; } // Gets a value indicating whether this method is declared as virtual.
         bool IsNewSlot { get; } // Gets a value indicating whether this method has a new slot in the type's method table.
         bool IsFinal { get; } // Gets a value indicating whether this method is declared with the <i>sealed</i> keyword in C#, <i>NotOverridable</i> keyword in VB.NET.
         ```
         
         ## CORE CONCEPTS
         
         ### INHERITANCE FUNDAMENTALS
         - **Base Class**: Parent class that other classes inherit from
         - **Derived Class**: Child class that inherits from a base class
         - **Inheritance Chain**: Sequence from most derived to root base class
         - **Depth of Inheritance (DOI)**: Number of levels in inheritance chain (direct derivation level is 1)
         - **Abstract Class**: Class that cannot be instantiated, meant to be inherited
         - **Sealed Class**: Class that cannot be inherited from
         - **Virtual Methods**: Methods that can be overridden in derived classes
         - **Override Methods**: Methods that override base class virtual/abstract methods
         
         ### INHERITANCE PATTERNS
         - **Single Inheritance**: Class inherits from one base class (C# and .NET limitation)
         - **Hierarchical Inheritance**: Multiple classes inherit from same base
         - **Multi-Level Inheritance**: Chain of inheritance (A → B → C)
         - **Template Method Pattern**: Abstract base with virtual hook methods
         - **Strategy Pattern**: Base class with different implementations
         - **Factory Method Pattern**: Virtual creation methods
         
         ### OOP Principles
         - **Liskov Substitution Principle (LSP)**: Derived classes should be substitutable for base
         - **Open/Closed Principle**: Open for extension through inheritance, closed for modification the base class
         - **Prefer Composition over Inheritance**: When inheritance depth becomes excessive
         - **Inheritance vs. Interface**: When to use each abstraction mechanism
         
         ## COMMON QUERY PATTERNS
         
         ### Basic Inheritance Queries
         ```csharp
         // All abstract base classes and their derived types
         from t in Types
         where t.IsAbstract && t.IsClass
         select new { t, t.DerivedTypes }
         
         // All classes with base classes (excluding Object)
         from t in Types 
         where t.IsClass && t.BaseClass != null && t.BaseClass.Name != "Object"
         select new { t, BaseClass = t.BaseClass }
         
         // All derived classes of a specific base
         from t in Types 
         where t.DeriveFrom("System.Object")
         select t
         
         // Classes with many derived classes
         from t in Types 
         where t.IsClass && t.NbChildren >= 5
         select new { t, t.DerivedTypes }
         ```
         
         ### INHERITANCE DEPTH ANALYSIS
         ```csharp
         // Deep inheritance hierarchies (potential design issue)
         from t in Types 
         where t.DepthOfInheritance > 5
         orderby t.DepthOfInheritance descending
         select new { t, t.DepthOfInheritance, InheritanceChain = t.BaseClasses }
         
         // Calculate average inheritance depth
         let avgDepth = Types.Where(t => t.IsClass).Average(t => t.DepthOfInheritance)
         from t in Types 
         where t.IsClass && t.DepthOfInheritance > avgDepth
         select new { t, t.DepthOfInheritance, Average = avgDepth }
         
         // Root base classes (excluding System.Object)
         from t in Types 
         where t.IsClass && t.NbChildren > 0 && 
               (t.BaseClass == null || t.BaseClass.Name != "System.Objects")
         orderby t.NbChildren descending
         select new { t, ChildCount = t.NbChildren }
         ```
         
         ### ABSTRACT CLASS ANALYSIS
         ```csharp
         // All abstract classes
         from t in Types 
         where t.IsAbstract && t.IsClass
         select t
         
         // Abstract classes with no derived classes
         from t in Types 
         where t.IsAbstract && t.IsClass  && t.NbChildren == 0
         select t
         
         // Abstract classes with many abstract methods
         from t in Types 
         where t.IsAbstract && t.IsClass
         let abstractMethods = t.Methods.Where(m => m.IsAbstract).ToArray()
         where abstractMethods.Any()
         orderby abstractMethods.Length descending
         select new { t, abstractMethods }
         ```
         
         ### VIRTUAL AND OVERRIDE ANALYSIS
         ```csharp
         // Virtual methods
         from m in Methods 
         where m.IsVirtual && !m.IsAbstract
         select m
         
         // Override methods
         from m in Methods 
         let baseMethods = m.OverriddensBase.ToArray()
         where baseMethods.Length > 0
         orderby baseMethods.Length descending
         select new { m, baseMethods }
         
         // Abstract methods with many overriddes
         from m in Methods 
         where m.IsAbstract
         let overrides = m.OverridesDerived.ToArray()
         where overrides.Length >= 5
         orderby overrides.Length descending 
         select new { m, m.ParentType, overrides }
         
         // Virtual methods never overridden
         from m in Methods 
         where m.IsVirtual && !m.IsFinal && !m.OverridesDerived.Any()
         select m
         
         // Methods overriding but calling base
         from m in Methods 
         let baseMethods = m.OverriddensBase.ToArray()
         where baseMethods.Length > 0
         let baseMethodsCalled = m.MethodsCalled.Intersect(baseMethods).ToArray()
         orderby baseMethodsCalled .Length descending
         select new { m, baseMethodsCalled }
         ```
         
         ### SEALED CLASS ANALYSIS
         ```csharp
         // Sealed classes in inheritance hierarchies
         from t in Types 
         where t.IsSealed && t.IsClass && 
               t.DepthOfInheritance >= 2
         select new { t, t.BaseClasses }
         
         // Classes that should be sealed (not public + no derived classes)
         from t in Types 
         where t.IsClass && !t.IsSealed && !t.IsAbstract && 
               t.NbChildren == 0 && !t.IsPubliclyVisible
         select t
         
         // Virtual methods in sealed classes (unnecessary)
         from m in Methods 
         where m.IsVirtual && m.ParentType.IsSealed
         select new { m, SealedParent = m.ParentType }
         ```
         
         ### POLYMORPHISM PATTERNS
         ```csharp
         // Methods using polymorphic calls
         from m in Methods 
         let pCalls = m.MethodsCalled.Where(mc => mc.IsVirtual || mc.IsAbstract).ToArray()
         where pCalls.Any()
         orderby pCalls.Length descending
         select new { m, PolymorphicCalls = pCalls }
         ```
         
         
         ## QUERY CATEGORIES
         
         ### 1. Hierarchy Discovery
         - Map inheritance trees
         - Find root base classes
         - Identify inheritance chains
         - Visualize class hierarchies
         
         ### 2. Depth Analysis
         - Measure inheritance depth
         - Find deep hierarchies (anti-pattern)
         - Calculate DOI metrics
         - Compare depth across namespaces
         
         ### 3. Abstract Class Design
         - Find abstract classes
         - Validate abstract patterns
         - Detect unused abstractions
         - Analyze template methods
         
         ### 4. Polymorphism Quality
         - Find virtual methods
         - Analyze override patterns
         - Detect LSP violations
         - Validate polymorphic design
         
         ### 5. Sealed Class Usage
         - Find sealed classes
         - Identify sealing candidates
         - Detect unnecessary virtuals
         - Optimize performance with sealing
         
         ### 6. Design Pattern Detection
         - Template method pattern
         - Factory method pattern
         - Strategy pattern via inheritance
         - Abstract factory hierarchies
         
         ### 7. Refactoring Opportunities
         - Excessive inheritance depth
         - Prefer composition over inheritance
         - Unused virtual methods
         - Unnecessary abstractions
         
         ## COMMON ANALYSIS SCENARIOS
         
         ### Scenario 1: Inheritance Depth Audit
         Identify deep inheritance hierarchies that violate OOP best practices (DOI > 5 is often problematic).
         
         ### Scenario 2: Abstract Class Validation
         Ensure abstract classes are properly designed, used, and have derived implementations.
         
         ### Scenario 3: Polymorphism Analysis
         Measure effective use of virtual/override methods and polymorphic design patterns.
         
         ### Scenario 4: LSP Compliance
         Verify that derived classes properly substitute for base classes without breaking contracts.
         
         ### Scenario 5: Refactoring to Composition
         Find inheritance hierarchies that should be refactored to use composition instead.
         
         ### Scenario 6: Performance Optimization
         Identify classes that should be sealed to enable compiler optimizations.
         
         ### Scenario 7: Design Pattern Recognition
         Detect and validate common OOP design patterns in the codebase.
         
         ## ADVANCED PATTERNS
         
         ### Complete Hierarchy Mapping
         ```csharp
         // Map entire inheritance tree
         from t in Types 
         where t.IsClass && t.FullName != "System.Object"
         let hierarchy = new { 
             Type = t, 
             Base = t.BaseClass, 
             Depth = t.DepthOfInheritance,
             Children = t.NbChildren 
         }
         orderby hierarchy.Depth descending
         select hierarchy
         ```
         
         ### Fragile Base Class Detection
         ```csharp
         // Base classes with many children (risky changes)
         from t in Types 
         where t.IsClass && t.NbChildren > 10 &&
               t.Methods.Any(m => m.IsVirtual)
         select new { 
             t, 
             ChildCount = t.NbChildren, 
             VirtualMethods = t.Methods.Where(m => m.IsVirtual).Count(),
             Risk = "High - changes impact many derived classes"
         }
         ```
         
         ### COMPOSITION OVER INHERITANCE CANDIDATES
         ```csharp
         // Deep hierarchies that might benefit from composition
         from t in Types 
         where t.DepthOfInheritance > 4 &&
               t.Fields.Count() > 5 &&
               t.BaseClass.Methods.Count(m => m.IsVirtual) < 3
         select new { 
             t, 
             Depth = t.DepthOfInheritance,
             Suggestion = "Consider composition - inheritance seems shallow"
         }
         ```
         
         ## EXAMPLE REQUEST PATTERNS
         
         Users may ask queries like:
         - "Show me classes with deep inheritance hierarchies"
         - "Find all abstract classes and their derived types"
         - "Which virtual methods are never overridden?"
         - "Show me sealed classes that inherit from others"
         - "Find classes violating LSP"
         - "Which base classes have the most children?"
         - "Show inheritance chains in my domain layer"
         - "Find template method pattern usage"
         - "Which classes should be sealed for performance?"
         - "Show me inheritance vs. composition opportunities"
         
         ## Inheritance Design Best Practices
         
         - **Limit DOI**: Keep inheritance depth ≤ 5 levels
         - **Favor Composition**: Use inheritance for "is-a", composition for "has-a"
         - **LSP Compliance**: Derived classes must be substitutable for base
         - **Seal When Possible**: Seal classes not designed for inheritance
         - **Virtual by Design**: Only make methods virtual if inheritance is intended
         - **Abstract Thoughtfully**: Abstract classes should define clear contracts
         - **Document Inheritance**: Base classes should document inheritance contracts
         - **Avoid Fragile Base**: Limit public/protected surface in base classes
         - **Single Inheritance**: Remember C# only supports single class inheritance
         - **Template Method**: Good pattern for controlled extension points
         
         ## ANTI-PATTERNS TO DETECT
         
         - **God Object Hierarchy**: Deep, complex inheritance trees
         - **Yo-Yo Problem**: Excessive up/down navigation in hierarchy
         - **Refused Bequest**: Derived classes not using base functionality
         - **Empty Abstract Classes**: Abstract classes with no implementation
         - **Virtual Everything**: Over-use of virtual methods
         - **Deep Hierarchy**: DOI > 6 is generally problematic
         """;
}
