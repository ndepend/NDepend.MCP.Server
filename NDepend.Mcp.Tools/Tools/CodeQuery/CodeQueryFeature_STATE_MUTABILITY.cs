namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryFeature {
    
    internal const string STATE_MUTABILITY_PROMPT =
          """
          # Field & Type Mutability
          
          Your task is to generate accurate, efficient CQLinq queries that analyze field mutability, type state management, and immutability patterns in .NET codebases.
          
          ## Definitions
          
          - A type is immutable if its instance fields can’t change after construction.
          - A static field is immutable if it’s `readonly` and assigned only in the static constructor.
          - An instance field is immutable if it’s `readonly` and assigned only in constructors.
          - readonly implies immutability, but some immutable fields aren’t marked `readonly` (and can be without errors).
          - A field can be immutable even if the referenced object is mutable (e.g., a `readonly List<T>`).
          
          ## APIS
          
          ### IField
          
          At analysis time, NDepend computes the mutability of types and fields.
          The result is available through the properties *IType.IsImmutable* and *IField.IsImmutable*. 
          
          Here are properties to analyze mutability.
          
          ```csharp
          // IField
          bool IsInitOnly; // declared readonly
          IEnumerable<IMethod> MethodsUsingMe;
          IEnumerable<IMethod> MethodsAssigningMe;
          IEnumerable<IMethod> MethodsReadingMeButNotAssigningMe;
          
          // IMethod
          bool ChangesObjectState;   // modifies instance field
          bool ChangesTypeState;     // modifies static field
          IEnumerable<IField> FieldsAssigned;
          IEnumerable<IField> FieldsReadButNotAssigned;
          IEnumerable<IField> FieldsUsed;
          
          // IProperty
          IMethod GetMethod, SetMethod;
          IField  BackingField;
          bool IsReadOnly, IsWriteOnly, IsReadWrite;
          IEnumerable<IMethod> MethodsReadingMe, MethodsWritingMe, MethodsUsingMe;
          IEnumerable<IField>  FieldsUsed, FieldsAssigned;
          ```
          
          ## Queries
          
          ```csharp
          // Readonly fields
          from f in Fields where f.IsInitOnly select f

          // Mutable instance fields
          from f in Fields where !f.IsImmutable && !f.IsLiteral && !f.IsStatic select f

          // Mutable static fields
          from f in Fields where f.IsStatic && !f.IsInitOnly && !f.IsLiteral select f

          // Property getters that mutate state (anti-pattern)
          from m in Application.Methods
          where m.IsPropertyGetter && (m.ChangesObjectState || m.ChangesTypeState)
          select new { m, m.FieldsAssigned }

          // Mutable structs (anti-pattern)
          warnif count > 0
          from t in Application.Types where t.IsStructure && !t.IsImmutable
          let mutableFields = t.Fields.Where(f => !f.IsImmutable)
          select new { t, t.NbLinesOfCode, mutableFields }

          // Immutable struct not declared readonly
          from t in JustMyCode.Types
          where t.IsStructure && t.IsImmutable && !t.IsReadOnly && !t.IsGeneratedByCompiler
          select t

          // Value types with mutable fields (anti-pattern)
          from t in Types where t.IsStructure && t.InstanceFields.Any(f => !f.IsInitOnly) select t

          // Public mutable fields
          from f in Fields where f.IsPublic && !f.IsInitOnly && !f.IsLiteral select f

          // Static mutable fields (thread-safety risk)
          from f in Application.Fields
          where f.IsStatic && !f.IsInitOnly && !f.IsLiteral
                && !f.HasAttribute("System.ThreadStaticAttribute".AllowNoMatch())
          select f

          // Static fields with mutable field type
          from f in Application.Fields
          where f.IsStatic && !f.IsEnumValue && !f.IsGeneratedByCompiler && !f.IsLiteral
          let ft = f.FieldType
          where ft != null && !ft.IsThirdParty && !ft.IsInterface && !ft.IsDelegate && !ft.IsImmutable
          select new { f, mutableFieldType = ft,
                       isFieldImmutable = f.IsImmutable ? "Immutable" : "Mutable",
                       isFieldReadOnly  = f.IsInitOnly  ? "ReadOnly"  : "Not ReadOnly" }

          // Fields assigned from many methods (complexity smell)
          from f in JustMyCode.Fields
          where !f.IsEnumValue && !f.IsImmutable && !f.IsInitOnly
                && !f.IsGeneratedByCompiler && !f.IsEventDelegateObject
          let assigners = f.MethodsAssigningMe.Where(m => !m.IsConstructor)
          where assigners.Count() >= (f.IsStatic ? 3 : 4)
          select new { f, assigners, f.MethodsReadingMeButNotAssigningMe, f.MethodsUsingMe }

          // Large mutable structs
          from t in Types
          where t.IsStructure && t.SizeOfInst > 16 && t.InstanceFields.Any(f => !f.IsInitOnly)
          select t

          // Properties with public setters
          from p in Properties where p.SetMethod != null && p.SetMethod.IsPublic select p

          // Mutable properties on otherwise-immutable types
          from t in Application.Types
          let mutableProps = t.Properties.Where(p => p.SetMethod != null)
          where mutableProps.Any() && t.InstanceFields.Any()
                && t.InstanceFields.All(f => f.IsImmutable || f.IsLiteral)
          select new { t, mutableProps, t.InstanceFields }
          ```
          """;
}
