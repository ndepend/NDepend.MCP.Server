namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryFeature {
    internal const string DDD_PROMPT =
         """
         # Domain Driven Design with CQLinq
         
         ## DDD Concepts

         - **Entity**: Defined by unique identity. State-mutating properties must have private/protected setters. Parameterless constructors must be non-public. Collections exposed as IReadOnlyList<T>/IReadOnlyCollection<T>.
         - **Value Object**: Defined entirely by attributes (no identity). All fields readonly, no setters (or `init`). Must override Equals()/GetHashCode(), or use C# records.
         - **Aggregate Root**: Cluster entry point. Must expose public behavioral methods. References other Aggregates by ID only (never object references). Implements IAggregateRoot.
         - **Domain Layer Purity**: Domain project must not depend on third-party assemblies (ORM, HTTP, etc.).

         ## CQLinq Rules

         ```csharp
         // <Name>Ensure domain layer purity</Name>
         warnif count > 0
         
         // 1. Define the domain and assemblies it can use
         let domainProjects = Application.Assemblies
            .WithNameWildcardMatchIn("*.Domain", "*.Domain.*")
            .ToHashSetEx()
         let systemAssemblies = ThirdParty.Assemblies
            .Where(a => a.Name.StartsWithAny("System", "Microsoft", "mscorlib", "netstandard"))
            .ToHashSetEx()
         
         from a in domainProjects
         from t in a.ChildTypes
         where JustMyCode.Contains(t)
         
         // 2. Identify and group illegal types by their parent assembly
         let illegalTypeUsages = t.TypesUsed
             .Where(tu => !domainProjects.Contains(tu.ParentAssembly) && 
                          !systemAssemblies.Contains(tu.ParentAssembly))
             .GroupBy(tu => tu.ParentAssembly)
         
         where illegalTypeUsages.Any()
         
         // 3. Flatten the results so you see one row per (Domain Type + Illegal Assembly)
         from usageGroup in illegalTypeUsages
         let typesUsedInThisAssembly = usageGroup.Distinct()
         let debt = (10 + (typesUsedInThisAssembly.Count() * 2)).ToMinutes().ToDebt()
         
         select new { 
            DomainType = t, 
            IllegalAssembly = usageGroup.Key, 
            UsedTypes = typesUsedInThisAssembly, 
            Debt = debt,
            Severity = Severity.High 
         }
         
         // <Expl>
         // The {0} belongs to a domain project and shouldn’t use these types defined in the {1}:
         // {2}
         // </Expl>
         ```
         
         ```csharp
         // <Name>Entity should not expose mutable state</Name>
         warnif count > 0
         
         // 1. Define the Domain projects
         let domainProjects = Application.Assemblies
            .WithNameWildcardMatchIn("*.Domain", "*.Domain.*")
            .ToHashSetEx()
         
         // 2. Search for baseEntityClasses 
         let baseEntityClasses = Types.Where(t => t.IsAbstract && t.IsClass)
            .WithSimpleNameIn("BaseEntity", "EntityBase", "Entity")
            .ToHashSetEx()
         where baseEntityClasses.Any()
         
         from a in domainProjects
         from t in a.ChildTypes
         where JustMyCode.Contains(t) && t.IsClass &&
           t.BaseClasses.Intersect(baseEntityClasses).Any()
         
         // 3. Find public property setter or field for the entity class t
         let mutableMembers = t.Members.Where(m => 
            (!m.IsPrivate && !m.IsProtected) &&
            (m.IsMethod && m.AsMethod.IsPropertySetter) || m.IsField)
            .ToArray()
         where mutableMembers.Any()
         
         select new { 
           t,
           mutableMembers,
           Debt = (mutableMembers.Length*2).ToMinutes().ToDebt(),
           Severity = Severity.High
          }
         
         // <Expl>
         // The entity {0} exposes mutable state through non-private members:
         // {1}
         // </Expl>
         ```
         
         ```csharp
         // <Name>Entity must not have public parameterless constructors</Name>
         warnif count > 0
         
         // 1. Define the Domain projects
         let domainProjects = Application.Assemblies
         .WithNameWildcardMatchIn("*.Domain", "*.Domain.*")
         .ToHashSetEx()
         
         // 2. Search for baseEntityClasses 
         let baseEntityClasses = Types.Where(t => t.IsAbstract && t.IsClass)
         .WithSimpleNameIn("BaseEntity", "EntityBase", "Entity")
         .ToHashSetEx()
         where baseEntityClasses.Any()
         
         from a in domainProjects
         from t in a.ChildTypes
         where JustMyCode.Contains(t) && t.IsClass &&
         t.BaseClasses.Intersect(baseEntityClasses).Any()
         
         // 2. Flag constructors that are public and take no parameters
         from c in t.Constructors
         where c.NbParameters == 0 &&
           (c.IsPublic || c.IsInternal)
         
         select new { 
            Entity = t, 
            Constructor = c,
            Issue = "Entity has a public parameterless constructor. This allows creating entities in an invalid/empty state.",
            Debt = 2.ToMinutes().ToDebt(),
            Severity = Severity.Medium
         }
         
         // <Expl>
         // The entity {0} exposes a public parameterless constructor.
         // This allows creating entities in an invalid/empty state.
         // </Expl>
         ```
         
         ```csharp
         // <Name>Value object should not expose mutable state</Name>
         warnif count > 0
         
         // 1. Define the Domain projects
         let domainProjects = Application.Assemblies
            .WithNameWildcardMatchIn("*.Domain", "*.Domain.*")
            .ToHashSetEx()
         
         // 2. Search for valueObjects
         let valueObjects = domainProjects.ChildTypes().Where(t =>
             (t.InterfacesImplemented.Any(i => i.SimpleName == "IValueObject") ||
              t.BaseClasses.Any(bc => bc.SimpleName == "ValueObject") ||
              (t.IsRecord) && !t.IsAbstract)).ToArray()
         where valueObjects.Any()
         
         from t in valueObjects
         where JustMyCode.Contains(t)
         
         // 3. Find public property setter or field for the value object t
         let mutableMembers = t.Members.Where(m => 
            (!m.IsPrivate && !m.IsProtected) &&
            (m.IsMethod && m.AsMethod.IsPropertySetter) || m.IsField)
            .ToArray()
         where mutableMembers.Any()
         
         select new { 
           t,
           mutableMembers,
           Debt = (mutableMembers.Length*2).ToMinutes().ToDebt(),
           Severity = Severity.High
          }
         
         // <Expl>
         // The value object {0} exposes mutable state through non-private members:
         // {1}
         // </Expl>
         ```
         
         ```csharp
         // <Name>Value object should implement structural equality</Name>
         warnif count > 0
         
         // 1. Define the Domain projects
         let domainProjects = Application.Assemblies
            .WithNameWildcardMatchIn("*.Domain", "*.Domain.*")
            .ToHashSetEx()
         
         // 2. Search for valueObjects
         let valueObjects = domainProjects.ChildTypes().Where(t =>
             (t.InterfacesImplemented.Any(i => i.SimpleName == "IValueObject") ||
              t.BaseClasses.Any(bc => bc.SimpleName == "ValueObject")) &&
              !t.IsRecord) // With C# records, structural equality is automatic.
              .ToArray()
         where valueObjects.Any()
         
         from t in valueObjects
         where JustMyCode.Contains(t)
         
         let hasEqualsOverride = t.InstanceMethods.Any(
            m => m.SimpleName == "Equals" && m.OverriddensBase.Any())
         let hasHashCodeOverride = t.InstanceMethods.Any(
            m => m.SimpleName == "GetHashCode" && m.OverriddensBase.Any())
         let hasGetEqualityComponentsOverride= t.InstanceMethods.Any(
            m => m.SimpleName == "GetEqualityComponents" && m.OverriddensBase.Any())
         
         where (!hasEqualsOverride || !hasHashCodeOverride) && !hasGetEqualityComponentsOverride
         
         select new { 
           t,
           MissingEquals = !hasEqualsOverride,
           MissingHashCode = !hasHashCodeOverride,
           MissingGetEqualityComponents = !hasGetEqualityComponentsOverride,
           Debt = 5.ToMinutes().ToDebt(),
           Severity = Severity.High
          }
         
         // <Expl>
         // The value object {0} should override both Equals() and GetHashCode() or GetEqualityComponents() to implement structural equality.
         // </Expl>
         ```
         
         ```csharp
         // <Name>Aggregate Roots must have public behavioral methods</Name>
         warnif count > 0
          
         // 1. Define the Domain projects
         let domainProjects = Application.Assemblies
             .WithNameWildcardMatchIn("*.Domain", "*.Domain.*")
             .ToHashSetEx()
          
         // 2. Search for aggregate roots
         let aggRoots = domainProjects.ChildTypes().Where(t =>
              (t.InterfacesImplemented.Any(i => i.SimpleName == "IAggregateRoot") ||
               t.BaseClasses.Any(bc => bc.SimpleName == "AggregateRoot"))).ToArray()
         where aggRoots.Any()
         
         from t in aggRoots
         let publicBehaviorMethods = t.InstanceMethods
             // Exclude standard object overrides as they don’t count as "Domain Logic"
            .WithSimpleNameNotIn("ToString","GetHashCode", "Equals", "GetType")
            .Where(m => 
             m.IsPublic && 
             !m.IsConstructor && 
             !m.IsPropertyGetter && 
             !m.IsPropertySetter && 
             !m.IsOperator).ToArray()
         where !publicBehaviorMethods.Any()
         
         select new { 
            AggregateRoot = t, 
            Severity = Severity.Medium,
            Debt = 10.ToMinutes().ToDebt()
         }
         
         // <Expl>
         // The aggregate root {0} exposes no public behavioral methods.
         // </Expl>
         ```
         
         ```csharp
         // <Name>Aggregate Roots should reference other Aggregates by ID only</Name>
         warnif count > 0
          
         // 1. Define the Domain projects
         let domainProjects = Application.Assemblies
             .WithNameWildcardMatchIn("*.Domain", "*.Domain.*")
             .ToHashSetEx()
          
         // 2. Search for aggregate roots
         let aggRoots = domainProjects.ChildTypes().Where(t =>
              (t.InterfacesImplemented.Any(i => i.SimpleName == "IAggregateRoot") ||
               t.BaseClasses.Any(bc => bc.SimpleName == "AggregateRoot"))).ToHashSetEx()
         where aggRoots.Any()
         
         // 3. Find properties referencing directly other aggregate roots
         from t in aggRoots
         from p in t.Properties
         where p.PropertyType != null && aggRoots.Contains(p.PropertyType)
         
         select new { 
            AggregateRoot = t, 
            Property = p,
            ReferencedType = p.PropertyType,
            Severity = Severity.High,
            Debt = 15.ToMinutes().ToDebt()
         }
         
         // <Expl>
         // The aggregate root {0} references directly the {2} through the {1}.
         // It should reference it by Id.
         // </Expl>
         ```
         """;
}
