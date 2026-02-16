namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryFeature {
    
    internal const string STATE_MUTABILITY_PROMPT =
          """
          # FIELD AND TYPE STATE AND MUTABILITY
          
          Your task is to generate accurate, efficient CQLinq queries that analyze field mutability, type state management, and immutability patterns in .NET codebases.
          
          ## DEFINITIONS
          
          - A type is considered *immutable* if its instance fields’ state cannot be modified after an instance has been constructed.
          - A static field is considered immutable if it is private and is assigned only by the static constructor.
          - An instance field is considered immutable if it is private and is assigned only by the type’s constructor(s) or by the type’s static constructor.
          - A field declared as readonly is necessarily immutable. However, a field can be immutable without being declared readonly. In this case, the readonly keyword can be added to the field declaration without causing any compilation error.
          - A field can be immutable even if the object it points to is mutable. For example, a `private readonly` field that references a `List<T>` is itself immutable,  but the `List<T>` object it refers to can still be modified.
          
          ## APIS
          
          At analysis time, NDepend computes the mutability of types and fields.
          The result is available through the properties *IType.IsImmutable* and *IField.IsImmutable*. 
          
          The IField interface also exposes other properties that help analyze field mutability.
          
          ```csharp
          bool IsInitOnly { get; } // Gets true if the field is declared with the readonly modifier.
          IEnumerable<IMethod> MethodsUsingMe { get; } // Gets a sequence of methods that are using this field. A field is considered as used if it is read or assigned.
          IEnumerable<IMethod> MethodsAssigningMe { get; } // Gets a sequence of methods that are assigning this field.
          IEnumerable<IMethod> MethodsReadingMeButNotAssigningMe { get; } // Gets a sequence of methods that are reading this field but not assigning it.
          ```
          
          The IMethod interface provides these related properties:
          ```csharp
          bool ChangesObjectState { get; } // Indicate whether the method modifies an instance field
          bool ChangesTypeState { get; }, // Indicate whether the method modifies a static field
          IEnumerable<IField> FieldsUsed { get; } // Gets a sequence of fields that this method is using.
          IEnumerable<IField> FieldsAssigned { get; } // Gets a sequence of fields that this method is assigning.
          IEnumerable<IField> FieldsReadButNotAssigned { get; } // Gets a sequence of fields that this method is reading but not assigning.
          ```
          
          The IProperty interface provides these properties:
          ```csharp
          IMethod GetMethod { get; } // If this property has a get method, returns it, else returns 'null'
          IMethod SetMethod { get; } // If this property has a set method, returns it, else returns 'null'
          IField BackingField { get; } // If this property has a generated or manually declared backing field, returns it. Else returns 'null'
          bool IsReadOnly { get; } // Returns 'true' if this property has a get method but not a set method
          bool IsWriteOnly { get; } //Returns 'true' if this property has a set method but not a get method
          bool IsReadWrite { get; } // Returns 'true' if this property has both a get method and a set method
          IEnumerable<IMethod> MethodsUsingMe { get; }        // Methods reading or writing this property
          IEnumerable<IMethod> MethodsReadingMe { get; }      // Methods reading this property
          IEnumerable<IMethod> MethodsWritingMe { get; }      // Methods writing this property
          IEnumerable<IMethod> MethodsCalled { get; }         // Methods called by this property (if any)
          IEnumerable<IMember> MembersUsed { get; }           // Members used by this property
          IEnumerable<IField> FieldsUsed { get; }             // Fields used by this property
          IEnumerable<IField> FieldsAssigned { get; }         // Fields assigned by this property
          ```
          
          ## COMMON QUERY PATTERNS
          
          ### BASIC MUTABILITY QUERIES
          ```csharp
          // <Name>All readonly fields</Name>
          from f in Fields where f.IsInitOnly select f
          
          // <Name>All const fields</Name>
          from f in Fields where f.IsLiteral select f
          
          // <Name>Mutable instance fields</Name>
          from f in Fields 
          where !f.IsImmutable && !f.IsLiteral && !f.IsStatic
          select f
          
          // <Name>Mutable static fields (potential issues)</Name>
          from f in Fields 
          where f.IsStatic && !f.IsInitOnly && !f.IsLiteral
          
          // <Name>Property getters should be pure</Name>
          from m in Application.Methods
          where m.IsPropertyGetter && (m.ChangesObjectState || m.ChangesTypeState)
          select new { m, m.FieldsAssigned }
          select f
          ```
          
          ### IMMUTABLE TYPE DETECTION
          ```csharp
          // <Name>Structures should be immutable</Name>
          warnif count > 0 from t in Application.Types where 
             t.IsStructure && !t.IsImmutable
          let mutableFields = t.Fields.Where(f => !f.IsImmutable)
          select new { t, t.NbLinesOfCode, mutableFields }
          
          // <Name>Potentially immutable types (all fields readonly)</Name>
          from t in Types 
          where t.IsClass && 
                t.Fields.All(f => f.IsInitOnly || f.IsLiteral || f.IsStatic)
          select t
          
          // <Name>Types with no instance state</Name>
          from t in Types 
          where t.InstanceFields.Count() == 0
          select t
          
          // <Name>Value types with mutable fields (anti-pattern)</Name>
          from t in Types 
          where t.IsValueType && 
                t.InstanceFields.Any(f => !f.IsInitOnly)
          select t
          
          // <Name>Immutable struct should be declared as readonly</Name>
          from t in JustMyCode.Types
             //  Match structures that are immutable but not declared as *readonly struct*
          where t.IsStructure && 
                t.IsImmutable && 
               !t.IsReadOnly &&  
               !t.IsGeneratedByCompiler
          select t
          ```
          
          ### STATE MANAGEMENT ISSUES
          ```csharp
          // <Name>Public mutable fields</Name>
          from f in Fields 
          where f.IsPublic && !f.IsInitOnly && !f.IsLiteral
          select f
          
          // <Name>Static mutable fields (thread-safety concerns)</Name>
          from f in Application.Fields 
          where f.IsStatic && 
               !f.IsInitOnly && 
               !f.IsLiteral && 
               !f.HasAttribute("System.ThreadStaticAttribute".AllowNoMatch())
          select f
          
          // <Name>Large mutable structs</Name>
          from t in Types 
          where t.IsValueType && 
                t.SizeOfInst > 16 && 
                t.InstanceFields.Any(f => !f.IsInitOnly)
          select t
          
          // <Name>Static fields with a mutable field type</Name>
          from f in Application.Fields
          where f.IsStatic && 
               !f.IsEnumValue && 
               !f.IsGeneratedByCompiler &&
               !f.IsLiteral
          let fieldType = f.FieldType
          where fieldType != null && 
               !fieldType.IsThirdParty && 
               !fieldType.IsInterface && 
               !fieldType.IsDelegate &&
               !fieldType.IsImmutable
               
          select new { 
             f, 
             mutableFieldType = fieldType , 
             isFieldImmutable = f.IsImmutable ? "Immutable" : "Mutable", 
             isFieldReadOnly = f.IsInitOnly ? "ReadOnly" : "Not ReadOnly",
          }
          
          // <Name>Fields assigned from many methods</Name>
          from f in JustMyCode.Fields where 
            !f.IsEnumValue &&
            !f.IsImmutable && 
            !f.IsInitOnly &&
            !f.IsGeneratedByCompiler &&
            !f.IsEventDelegateObject

          let methodsAssigningMe = f.MethodsAssigningMe.Where(m => !m.IsConstructor)

          // The thresholds 4 and 3 are arbitrary and it should avoid matching too many fields.
          // Threshold is even lower for static fields because this reveals situations even more complex.
          where methodsAssigningMe.Count() >= (!f.IsStatic ? 4 : 3)

          select new { 
             f, 
             methodsAssigningMe, 
             f.MethodsReadingMeButNotAssigningMe, 
             f.MethodsUsingMe,
             staticOrInst = f.IsStatic ? "static" : "instance",
          }
          ```
          
          ### PROPERTY MUTABILITY
          ```csharp
          // <Name>Properties with public setters</Name>
          from p in Properties 
          where p.SetMethod != null && p.SetMethod.IsPublic
          select p
          
          // <Name>Mutable properties in otherwise immutable types</Name>
          from t in Application.Types 
          let mutableProps = t.Properties.Where(p => p.SetMethod != null)
          where mutableProps.Any() &&
                t.InstanceFields.Any() &&
                t.InstanceFields.All(f => f.IsImmutable || f.IsLiteral)
          select new { t, mutableProps, t.InstanceFields }
          ```
          
          ## COMMON ANALYSIS SCENARIOS
          
          ### Scenario 1: Immutability Audit
          Identify which types are immutable, partially immutable, or fully mutable to understand design patterns.
          
          ### Scenario 2: Thread-Safety Analysis
          Find mutable static state and shared mutable fields that could cause concurrency issues.
          
          ### Scenario 3: Value Type Validation
          Ensure structs follow best practices (small, immutable, value semantics).
          
          ### Scenario 4: Refactoring to Immutability
          Identify types that could be made immutable or fields that could be readonly.
          
          ### Scenario 5: State Encapsulation
          Verify that mutable state is properly encapsulated and not exposed publicly.
          
          ### Scenario 6: Performance Analysis
          Find unnecessary mutability or identify opportunities for readonly optimizations.
          """;
}
