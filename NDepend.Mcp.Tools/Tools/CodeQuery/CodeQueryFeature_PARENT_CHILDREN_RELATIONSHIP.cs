namespace NDepend.Mcp.Tools.CodeQuery {
    internal partial class CodeQueryFeature {


        internal const string PARENT_CHILDREN_RELATIONSHIP_PROMPT =
     """
     # NAVIGATING CODE HIERARCHY
     
     This guide demonstrates how to navigate the parent-child relationships in the NDepend code model hierarchy: 
     Assembly → Namespace (Hierarchy) → Type → (Nested Type) → Member (Methods, Fields, Properties, Events).
     
     ## CODE MODEL HIERARCHY
     
     ```
     CodeBase (ICodeBase)
      └─ Assemblies (IAssembly)   (user might use the term Project instead of Assembly)
          └─ Namespaces (INamespace)
              └─ Types (IType)
                  ├─ Nested Types (IType)
                  ├─ Methods (IMethod)
                  ├─ Fields (IField)
                  ├─ Properties (IProperty)
                  └─ Events (IEvent)
                  
     ```
     
     ## PARENT-CHILD RELATIONSHIP PROPERTIES
     
     ### Navigating DOWN (Parent → Children)
     
     #### ICodeBaseView members (implemented by ICodeBase)

     JustMyCode, Application and ThirdParty are typed as ICodeBaseView that has these properties:
     
     ```csharp
     IEnumerable<IAssembly> Assemblies { get; }
     IEnumerable<INamespace> Namespaces { get; }
     IEnumerable<IType> Types { get; }
     IEnumerable<IMethod> Methods { get; }
     IEnumerable<IField> Fields { get; }
     IEnumerable<IProperty> Properties { get; }
     IEnumerable<IEvent> Events { get; }
     IEnumerable<ICodeElement> CodeElements { get; }
     IEnumerable<ICodeElementParent> CodeElementParents { get; }
     IEnumerable<ICodeContainer> CodeContainers { get; }
     IEnumerable<IMember> Members { get; }
     IEnumerable<IMember> TypesAndMembers { get; }
     IEnumerable<IAttributeTarget> AttributeTargets { get; }
     ```
     
     #### ICodeElementParent members (implemented by ICodeBase IAssembly, INamespace, IType)
     ```csharp
     // Return empty sequence if this is a namespace or a type
     IEnumerable<INamespace> ChildNamespaces { get; } 
     
     // Return empty sequence if this is a type
     IEnumerable<IType> ChildTypes { get; }   
     
     // Ignore type if this is a type
     IEnumerable<IMember> ChildTypesAndMembers { get; }  
     
     IEnumerable<IMember> ChildMembers { get; }  
     IEnumerable<IMethod> ChildMethods { get; }
     IEnumerable<IField> ChildFields { get; }
     IEnumerable<IProperty> ChildProperties { get; }
     IEnumerable<IEvent> ChildEvents { get; }
     ```
     
     #### INamespace members
     ```csharp
     //Gets a sequence of direct child namespaces of this namespace. For example child namespaces of the namespace 'AA.BB' could be 'AA.BB.CC' but not 'AA.BB.CC.DD'.
     IEnumerable<INamespace> DirectChildNamespaces { get; }
     ```
     
     #### IType members
     ```csharp
     IEnumerable<IMember> Members { get; }
     IEnumerable<IMember> InstanceMembers { get; }
     IEnumerable<IMember> StaticMembers { get; }
     
     IEnumerable<IMethod> Methods { get; }
     IEnumerable<IMethod> InstanceMethods { get; }
     IEnumerable<IMethod> StaticMethods { get; }
     
     IEnumerable<IField> Fields { get; }
     IEnumerable<IField> InstanceFields { get; }
     IEnumerable<IField> StaticFields { get; }
     
     IEnumerable<IMethod> Constructors { get; }
     IEnumerable<IMethod> MethodsAndConstructors { get; }
     IEnumerable<IMethod> InstanceMethodsAndConstructors { get; }
     
     IMethod ClassConstructor { get; } // Return null if not applicable
     IEnumerable<IProperty> Properties { get; }
     IEnumerable<IEvent> Events { get; }
     
     //Gets a sequence of types nested in this type, or nested in a type nested in this type (recursive).
     IEnumerable<IType> NestedTypes { get; }
     
     //Gets a sequence of types nested in this type. The sequence doesn't contain types nested in types nested in this type.
     IEnumerable<IType> DirectNestedTypes { get; }
     ```
    
     ### Navigating UP (Child → Parent)
     
     #### ICodeElement members
     ```csharp
     // Gets the parent code base of this code element, If 'this' is a code base, return 'this'.
     ICodeBase ParentCodeBase { get; }
     
     // Returns the code element parent of this code element. 
     ICodeElementParent Parent { get; }
     
     //Returns a sequence of code element parent of this code element. 
     IEnumerable<ICodeElementParent> Parents { get; }
     ```
     
     #### INamespace members
     ```csharp
     IAssembly ParentAssembly { get; }
     ```
     
     #### IType members
     ```csharp
     //Gets a value indicating whether this type is nested in a parent type.
     bool IsNested { get; }
     
     //Gets a sequence of parent type(s) in which this type is nested. 
     // The first type in sequence is the encapsulating type and so on.
     IEnumerable<IType> OutterTypes { get; }
     ```
     
     #### IMember members (implemented by IType, IMethod, IField, IProperty, IEvent)
     ```csharp
     IAssembly ParentAssembly { get; }
     INamespace ParentNamespace { get; }
     
     //Gets the parent type of this member. If this is a non-nested type returns null.
     IType ParentType { get; }
     ```
     
     ### CODE ELEMENTS HIERARCHY PROJECTION
     
     NDepend provides special extension methods to navigate the code element hierarchy of multiple elements at once.
     
     PATTERN:
     - `Parent*()` - Navigate UP to parent elements
     - `Child*()` - Navigate DOWN to child elements  
     - `UsAnd*()` - Include both the original elements AND the related elements
     
     EXAMPLE:
     
     Types.ParentAssemblies() → IEnumerable<IAssembly> 
     Types.UsAndParentNamespaces() → IEnumerable<ICodeElementParent>
     Types.ChildMethods() → IEnumerable<IMethod>
     Types.UsAndChildMethods() → IEnumerable<IMember>
     Methods.UsAndParentTypes() → IEnumerable<IMember>
     
     All possible variations are exposed through the NDepend API.
     
     WHY USE PROJECTIONS:
     - Cleaner queries - avoid nested loops
     - Better performance - single operation
     - More readable - express intent clearly
     - Automatic deduplication - no duplicate parents
     
     ## CQLINQ EXAMPLES
     
     ### Example 1: Assembly → Types
     
     ```csharp
     // Find all types in specific assembly
     from a in Assemblies
     where a.Name == "MyApp.Core"
     from t in a.ChildTypes
     select new {
         t,
         t.ParentNamespace,
         t.ParentAssembly
     }
     ```
     
     ### Example 2: Type → Members (All Children)
     
     ```csharp
     // List all members of a type with their parent
     from t in Types
     where t.FullName == "MyApp.Services.UserService"
     from m in t.Members
     select new {
         m,
         m.Parent
     }
     ```
     
     ### Example 3: Methods and Their Parents
     
     ```csharp
     from m in Methods
     where m.NbLinesOfCode > 50
     select new {
         m,
         m.NbLinesOfCode,
         m.ParentType,
         m.ParentNamespace,
         m.ParentAssembly
         m.Parents
     }
     ```
     
     ### Example 4: Namespace Hierarchy Navigation
     
     ```csharp
     // Navigate namespace hierarchy
     from ns in Namespaces
     where ns.FullName.StartsWith("MyApp")
     select new {
         ns,
         ns.ParentNamespace,
         ns.ChildNamespaces,
         IsRoot = ns.ParentNamespace == null
     }
     ```
     
     ### Example 5: Properties and Parent Relationships
     
     ```csharp
     // Properties with parent context
     from p in Properties
     where p.IsPublic
     select new {
         p,
         p.Parent,
         p.PropertyType,
         HasGetter = p.GetMethod != null,
         HasSetter = p.SetMethod != null,
         ParentTypeIsAbstract = p.ParentType.IsAbstract
     }
     ```
     
     ### Example 6: Nested Types and Parent Types
     
     ```csharp
     // Find nested types and their parents
     from t in Types
     where t.IsNested
     select new {
         t,
         t.ParentType,
         t.OutterTypes,
         NestingLevel = t.OutterTypes.Count()
     }
     ```
     
     ### Example 7: Assembly → All Descendants
     
     ```csharp
     // Assemblies with name like 'Domain' and their child namespaces and types
     let asm = Application.Assemblies.WithNameLike("Domain")
     from x in asm.UsAndChildTypes().Concat(asm.ChildNamespaces())
     select x
     ```
     """;
    }
}
