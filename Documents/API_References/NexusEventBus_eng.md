# Nexus Prime Architectural Manual: NexusEventBus (Messaging Backbone)

## 1. Introduction
`NexusEventBus.cs` is the "Nervous System" of the Nexus Prime framework. It is a high-performance distribution center providing communication between independent systems (e.g., ECS Simulation, Unity UI, Network Layer) without them knowing about each other (Decoupled).

The reason for this bus's existence is to offer a data-driven reactive architecture by removing the requirement "System A must know system B" (Tight Coupling). Any type of data structure using the `INexusEvent` interface can be distributed at the speed of light via this system.

---

## 2. Technical Analysis
Offers the following multi-modal structures for maximum flexibility and performance:

- **Global Pub/Sub**: Standard events that can be listened to and triggered from anywhere. Provides thread-safe management with `ConcurrentDictionary`.
- **Local (Per-Entity) Events**: Events concerning only a specific entity (EntityId). E.g.: Transmits "This enemy took damage" info only to the UI component attached to that enemy.
- **Buffered Publishing**: Queues (Buffers) events that do not yet have subscribers and sprays them to them when the first subscriber arrives.
- **Debounced Publishing**: Prevents the same event from being triggered thousands of times within a certain time interval (e.g., UI update signals).
- **Unity-to-Entity Resolution**: Simplifies coding in hybrid projects by automatically converting Unity `GameObject` or `Component` references into `EntityId`.

---

## 3. Logical Flow
1.  **Subscription (Subscribe)**: A system records itself saying "I am interested in `PlayerSpawnEvent`".
2.  **Publishing (Publish)**: Another system throws the event.
3.  **Filtering**: If the event is local (Local), it is distributed only to the target entity's subscribers; if it is global, to all relevant systems.
4.  **Execution**: Callback methods (Delegate) are called in order.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Decoupling** | Systems being able to communicate without knowing each other's internal structure. |
| **Reactive Logic** | Automatic response given to an event when it occurs (e.g., taking damage). |
| **Event Debouncing** | Reducing recurring events within a short period to a single process. |
| **Unified Identity** | Recognition of Unity objects and ECS entities via the same ID system. |

---

## 5. Usage Example
```csharp
// Global Subscriber
NexusEventBus.Subscribe<PlayerDiedEvent>(e => Debug.Log("Player died!"));

// Local Subscriber (Listen only to messages coming to this object)
NexusEventBus.SubscribeLocal<DamageEvent>(myEntityId, e => ShowDamageNumbers(e.Amount));

// Publishing
NexusEventBus.Publish(new PlayerDiedEvent { Time = DateTime.Now });
```

---

## 6. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Communication;

public static class NexusEventBus
{
    private static readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();

    public static void Subscribe<T>(Action<T> handler) where T : INexusEvent {
        // Safe lock & add logic...
    }

    public static void Publish<T>(T @event) where T : INexusEvent {
        // Broadcast to all subscribers...
    }

    public static void PublishLocal<T>(EntityId id, T @event) where T : INexusEvent {
        // Precise target delivery...
    }
}
```

---

## Nexus Optimization Tip: Handler Copying
Execute by copying the subscriber list (`ToArray`) during the `Publish` process. This prevents "Collection Modified" errors that may occur when a new `Subscribe` or `Unsubscribe` is performed while inside an event handler and **increases thread-safe read performance.**
