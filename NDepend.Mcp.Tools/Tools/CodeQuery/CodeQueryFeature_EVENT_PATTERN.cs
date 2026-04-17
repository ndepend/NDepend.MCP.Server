namespace NDepend.Mcp.Tools.CodeQuery {
    internal partial class CodeQueryFeature {

        internal const string EVENT_PATTERN_PROMPT =
        """
        # NDepend: Event Memory Leak Detection Guide

        ## APIs
        
        ### IEvent Interface
        Represents an event declaration.
        
        ```csharp
        interface IEvent : IMember {
            // Accessors
            IMethod EventAdder { get; }     // add { } accessor
            IMethod EventRemover { get; }   // remove { } accessor
            
            // When the add and remove accessors of an event are not defined explicitly, the compiler defines a backing filed for the event. Returns this field if it exists, else returns <i>null</i>.
            IField BackingField { get; }
            
            // Returns a sequence of IMember object that contains the available get method, set method and backing field for this property.
            IEnumerable<IMember> AccessorsAndBackingField { get; }
            
            IEnumerable<IMethod> MethodsSubscribingToMe { get; }
            IEnumerable<IMethod> MethodsUnsubscribingToMe { get; }
            IType EventType { get; }        // Delegate type (e.g., EventHandler) — may be null
            bool IsStatic { get; }
            bool IsAbstract { get; }
            bool IsVirtual { get; }
            bool IsNewSlot { get; }
            bool IsFinal { get; }
            bool IsExplicitInterfaceImpl { get; }
            IEnumerable<IMethod> MethodsUsingMe { get; }
            IEnumerable<IMethod> MethodsCalled { get; }
            IEnumerable<IMember> MembersUsed { get; }
            IEnumerable<IField> FieldsUsed { get; }
            IEnumerable<IField> FieldsAssigned { get; }
            
            // Events in derived types overriding this event (empty if none)
            IEnumerable<IEvent> OverridesDerived { get; }

            // Events in direct derived types overriding this event (empty if none)
            IEnumerable<IEvent> OverridesDirectDerived { get; }

            // Base/interface events overridden by this event (empty if none)
            IEnumerable<IEvent> OverriddensBase { get; }

        }
        ```
        
        IMethod members related to event.
        ```csharp
        IEvent ParentEvent { get; }
        bool IsEventAdder { get; } 
        bool IsEventRemover { get; } 
        ```

        IField members related to event.
        ```csharp
        IEvent ParentEvent { get; }
        ```
        
        ### Accessing Events
        
        ```csharp
        // Get all events
        IEnumerable<IEvent> allEvents = codeBase.Events;
        
        // Events in a specific type
        IType type = codeBase.Types.WithFullName("MyApp.Publisher").Single();
        IEnumerable<IEvent> typeEvents = type.Events;
        
        // Event subscribers (methods that call add)
        IEvent evt = codeBase.Events.WithFullName("MyApp.Publisher.DataChanged").Single();
        var subscribers = evt.MethodsSubscribingToMe;
        
        // Event unsubscribers (methods that call remove)
        var unsubscribers = evt.MethodsUnsubscribingToMe;
        ```
        
        ## CORE CONCEPTS
        
        ### Memory Leak Mechanism
        Events create strong references from publisher to subscriber:
        ```
        Publisher (long-lived) --[event reference]--> Subscriber (should be short-lived)
        ```
        
        If subscriber doesn’t unregister, it cannot be garbage collected while publisher lives.
        
        ### Risk Levels
        
        | Pattern | Risk | Reason |
        |---------|------|--------|
        | Static event, no deregistration | **CRITICAL** | Subscribers live until AppDomain unload |
        | Instance event, disposable subscriber, no deregistration in Dispose | **HIGH** | Common in UI components, services |
        | Constructor registration, no Dispose implementation | **MEDIUM** | May indicate missing cleanup |
        | Lambda/anonymous subscription | **HIGH** | Cannot be deregistered |
        | Registration count > Deregistration count | **MEDIUM** | Potential imbalance |
        
        ---
        
        ## COMMON QUERY PATTERNS
        
        ### Pattern 1: Events with Registrations but No Deregistrations
        
        ```csharp
        // Find events that are subscribed to but never unsubscribed
        warnif count > 0
        from e in Events
        let registrations = e.MethodsSubscribingToMe
        let deregistrations = e.MethodsUnsubscribingToMe
        where registrations.Any() && !deregistrations.Any()
        select new {
            e,
            e.MethodsSubscribingToMe,
            Risk = e.IsStatic ? "CRITICAL - Static Event" : "HIGH",
            Issue = "Event subscribed but never unsubscribed"
        }
        ```
        
        ### Pattern 2: Disposable Types Missing Deregistration
        
        ```csharp
        // Event registration in constructor but no Dispose pattern
        warnif count > 0
        
        // Prepare a convenient lookup table first for performance reasons
        let lookup = Events
          .SelectMany(e => e.MethodsSubscribingToMe
                    .Where(m => m.IsConstructor)
                    .Select(m => new { m.ParentType, e}))
          .ToLookup(pair => pair.ParentType, pair => pair.e)

        from pair in lookup
        where pair.Key.Implement("System.IDisposable") 
        let t = pair.Key        

        // Events this Disposable type subscribes to
        let subscribedEvents = pair.ToArray()
        
        // Check if Dispose() deregisters these events
        let disposeMethod = t.Methods.FirstOrDefault(m => 
            m.SimpleName == "Dispose" && m.NbParameters == 0)
        where disposeMethod  != null        

        let notDeregisteredInDispose = subscribedEvents.Where(e => 
                !e.MethodsUnsubscribingToMe.Contains(disposeMethod)).ToArray()

        where notDeregisteredInDispose.Any()
        select new {
            t,
            subscribedEvents, 
            notDeregisteredInDispose 
        }
        ```
        """;
    }
}
