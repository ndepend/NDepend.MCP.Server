namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string INTERFACE_PROMPT =
         """
         # Interfaces and Implementations
         
         Generate accurate CQLinq queries analyzing interfaces, implementations, and usage in .NET codebases.
         
         ## APIS
         
         The NDepend.API interface 'IType' presents these properties and methods related to interface:
          
          ```csharp
         bool IsInterface { get; }
         IEnumerable<IType> InterfacesImplemented { get; } // empty if none
         IEnumerable<IType> TypesThatImplementMe { get; }  // empty if not interface
         bool Implement(IType @interface)
         bool Implement("interface full name")
         ushort NbInterfacesImplemented { get; }
         ```
         
         The NDepend.API interfaces 'IMethod', 'IProperty', 'IEvent' present these properties and methods related to interface:
         ```csharp
         bool IsExplicitInterfaceImpl { get; } // Gets a value indicating whether this method is an explicit interface method implementation.
         bool IsAbstract { get; } // Gets a value indicating whether this method is an abstract method, or a method declared on an interface.
         IEnumerable<IMethod> OverriddensBase { get; } // Gets a sequence of methods declared in this method? 'ParentType.BaseClasses' and 'ParentType.InterfacesImplemented', overridden by this method. If this method doesn't override any method, gets an empty sequence. (same for IProperty and IEvent)
         IEnumerable<IMethod> OverridesDirectDerived { get; } IEnumerable<IMethod> OverridesDerived { get; } // For an interface method, both properties are equivalent and return the sequence of methods that implement it. (same for IProperty and IEvent)
         ```

         ## Query Patterns
         
         ```csharp
         // All interfaces in the codebase
         from t in Types where t.IsInterface select t

         // Third-party interfaces
         from i in ThirdParty.Types where i.IsInterface select i

         // Implementors of a specific interface
         from t in Types where t.Implement("System.IDisposable") select t

         // Performance wise to prefer interface.TypesThatImplementMe over type.InterfacesImplemented
         from i in Types where i.IsInterface
         select new { i, i.TypesThatImplementMe }
         
         // Types implementing many interfaces
         from t in Types where t.NbInterfacesImplemented > 3
         select new { t, t.InterfacesImplemented }

         // Interfaces with no implementations
         from i in Types where i.IsInterface && !i.TypesThatImplementMe.Any() select i

         // Empty / marker interfaces
         from i in Types where i.IsInterface && !i.Members.Any() select i

         // Classes implementing IDisposable
         from t in Types where t.IsClass && t.Implement("System.IDisposable") select t

         // Types implementing generic interfaces
         from t in Types
         let gi = t.InterfacesImplemented.Where(i => i.IsGeneric)
         where gi.Any() select new { t, gi }

         // Interfaces extending other interfaces
         from i in Types where i.IsInterface
         let ii = i.InterfacesImplemented
         where ii.Any() orderby ii.Count() descending
         select new { i, BaseInterfaces = ii }

         // Explicit interface implementations
         from t in Types
         let eii = t.Methods.Where(m => m.IsExplicitInterfaceImpl)
         where eii.Any() select new { t, eii }

         // Implementations per interface
         from i in Types where i.IsInterface
         select new { i, i.TypesThatImplementMe }

         // Methods implementing interface methods
         from m in Methods where m.ParentType.IsInterface
         select new { m, m.OverridesDerived }

         // Fat interfaces — ISP violation
         from i in Types where i.IsInterface && i.Members.Count() > 10
         select new { i, i.Members }

         // Abstraction ratio per type
         from t in Types
         let id = t.TypesUsed.Where(tu => tu.IsInterface).ToArray()
         let cd = t.TypesUsed.Where(tu => tu.IsClass || tu.IsStructure).ToArray()
         where id.Length > 0 orderby id.Length descending
         select new { t, InterfaceDeps = id, ConcreteDeps = cd }

         // Methods returning interfaces
         from m in Methods where m.ReturnType != null && m.ReturnType.IsInterface select m

         // Fields typed as interfaces
         from f in Fields where f.FieldType != null && f.FieldType.IsInterface select f

         // Unstable / changed interfaces (risk)
         from i in Types where i.IsInterface && i.WasChanged()
         let added = i.Members.Where(m => m.WasAdded())
         let removed = i.OlderVersion().Members.Where(m => m.WasRemoved())
         select new { i, i.TypesUsingMe, added, removed }
         ```

         ## Design Rules
         - ISP: keep interfaces small and focused
         - Dependency Inversion: depend on abstractions, not concretions
         - Explicit impl: use for conflict resolution
         - Changing interfaces = breaking contract risk
         """;
}
