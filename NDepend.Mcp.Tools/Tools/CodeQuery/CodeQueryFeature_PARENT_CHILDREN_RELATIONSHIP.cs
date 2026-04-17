namespace NDepend.Mcp.Tools.CodeQuery {
    internal partial class CodeQueryFeature {


        internal const string PARENT_CHILDREN_RELATIONSHIP_PROMPT =
     """
     # Code Hierarchy Navigation
     
     ## Hierarchy
     `CodeBase → Assembly → Namespace → Type → [NestedType] → Member (Method/Field/Property/Event)`
     
     ## DOWN Navigation (Parent → Children)
     
     ### ICodeBaseView properties (JustMyCode / Application / ThirdParty)

     ```csharp
     IEnumerable<IAssembly> Assemblies
     IEnumerable<INamespace> Namespaces
     IEnumerable<IType> Types
     IEnumerable<IMethod> Methods
     IEnumerable<IField> Fields
     IEnumerable<IProperty> Properties
     IEnumerable<IEvent> Events
     IEnumerable<ICodeElement> CodeElements
     IEnumerable<ICodeElementParent> CodeElementParents
     IEnumerable<ICodeContainer> CodeContainers
     IEnumerable<IMember> Members
     IEnumerable<IMember> TypesAndMembers
     IEnumerable<IAttributeTarget> AttributeTargets
     ```
     
     ### ICodeElementParent properties (implemented by ICodeBase IAssembly, INamespace, IType)
     ```csharp
     IEnumerable<INamespace> ChildNamespaces   // empty for namespace/type
     IEnumerable<IType>      ChildTypes        // empty for type
     IEnumerable<IMember>    ChildTypesAndMembers, ChildMembers
     IEnumerable<IMethod>    ChildMethods
     IEnumerable<IField>     ChildFields
     IEnumerable<IProperty>  ChildProperties
     IEnumerable<IEvent>     ChildEvents
     ```
     
     ### INamespace
     ```csharp
     // Get direct children only, child namespaces of the namespace 'AA.BB' could be 'AA.BB.CC' but not 'AA.BB.CC.DD'.
     IEnumerable<INamespace> DirectChildNamespaces { get; } // direct children only
     ```
     
     ### IType members
     ```csharp
     IEnumerable<IMember> Members { get; }
     IEnumerable<IMember> InstanceMembers;
     IEnumerable<IMember> StaticMembers;
     
     IEnumerable<IMethod> Methods;
     IEnumerable<IMethod> InstanceMethods;
     IEnumerable<IMethod> StaticMethods;
     
     IEnumerable<IField> Fields;
     IEnumerable<IField> InstanceFields;
     IEnumerable<IField> StaticFields;
     
     IEnumerable<IMethod> Constructors;
     IEnumerable<IMethod> MethodsAndConstructors;
     IEnumerable<IMethod> InstanceMethodsAndConstructors;
     
     IMethod ClassConstructor; // null if n/a
     IEnumerable<IProperty> Properties;
     IEnumerable<IEvent> Events;
     
     IEnumerable<IType> NestedTypes; // recursive
     
     //Gets a sequence of types nested in this type. The sequence doesn't contain types nested in types nested in this type.
     IEnumerable<IType> DirectNestedTypes;
     ```
    
     ## UP Navigation
     
     ### ICodeElement
     
     ```csharp
     ICodeBase ParentCodeBase { get; }
     ICodeElementParent Parent;
     IEnumerable<ICodeElementParent> Parents;
     ```
     
     ### INamespace
     
     ```csharp
     IAssembly ParentAssembly { get; }
     ```
     
     ### IType
     ```csharp
     bool IsNested { get; }
     
     // Gets a sequence of parent type(s) in which this type is nested. 
     // The first type in sequence is the encapsulating type and so on.
     IEnumerable<IType> OutterTypes { get; } // innermost first
     ```
     
     ### IMember  (implemented by IType, IMethod, IField, IProperty, IEvent)
     ```csharp
     IAssembly ParentAssembly { get; }
     INamespace ParentNamespace;
     IType ParentType;  // null for non-nested types
     ```
     
     ### Bulk Navigation Extensions
     
     Special extension methods to navigate the code element hierarchy of multiple elements at once.
     
     PATTERN:
     - `Parent*()` - Navigate UP to parent elements
     - `Child*()` - Navigate DOWN to child elements  
     - `UsAnd*()` - Include both the original elements AND the related elements
     
     EXAMPLE:
     
     ```csharp
     Types.ParentAssemblies()          // → IEnumerable<IAssembly>
     Types.UsAndParentNamespaces()     // → IEnumerable<ICodeElementParent>
     Types.ChildMethods()              // → IEnumerable<IMethod>
     Methods.UsAndParentTypes()        // → IEnumerable<IMember>
     ```
     
     ## Examples
     
     ```csharp
     // Types in a specific assembly
     from a in Assemblies where a.Name == "MyApp.Core"
     from t in a.ChildTypes select new { t, t.ParentNamespace, t.ParentAssembly }

     // Members of a specific type
     from t in Types where t.FullName == "MyApp.Services.UserService"
     from m in t.Members select new { m, m.Parent }

     // Large methods with full parent context
     from m in Methods where m.NbLinesOfCode > 50
     select new { m, m.ParentType, m.ParentNamespace, m.ParentAssembly }

     // Nested types with nesting depth
     from t in Types where t.IsNested
     select new { t, t.ParentType, NestingLevel = t.OutterTypes.Count() }

     // Assembly descendants
     let asm = Application.Assemblies.WithNameLike("Domain")
     from x in asm.UsAndChildTypes().Concat(asm.ChildNamespaces()) select x
     ```
     """;
    }
}
