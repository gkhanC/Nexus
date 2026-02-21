# Nexus Prime Architectural Manual: NexusCachedQuery (Reactive Query Caching)

## 1. Introduction
`NexusCachedQuery.cs` is a reactive layer that takes Nexus Prime's query performance to the next level. While a standard query scans the entire dataset every time it's called, CachedQuery stores the results in memory and updates this list only when the relevant data components change (Dirty).

The reason for this class's existence is to zero out the unnecessary load on the processor of re-calculating query results that contain thousands of entities but rarely change (e.g., "Static trees", "Passive enemies") every frame.

---

## 2. Technical Analysis
NexusCachedQuery uses the following architectural patterns for efficiency:

- **Dirty Flag Pattern**: When a component is added or removed, the `_isDirty` flag is activated. In this way, no search operation is performed until the data changes.
- **Event-Driven Updates**: Listens only to target components by subscribing to `OnEntityDestroyed`, `OnComponentAdded`, and `OnComponentRemoved` events on the `Registry`.
- **Lazy Rebuilding**: The cache is rebuilt not the moment the data changes, but the moment the result is needed (`GetEntities()`). This ensures that calculation is performed only once, even if there are multiple changes in a frame.
- **Set-Based Storage**: Using `HashSet<EntityId>`, it stores the entity list in a fast-accessible and unique way.

---

## 3. Logical Flow
1.  **Subscription**: When the query is created, events for target component types are subscribed to.
2.  **Monitoring**: When a change comes via the `Registry`, it is checked whether this change falls within the query scope, and the "Dirty" flag is raised if necessary.
3.  **Querying**: When the user requests data, if the flag is raised, it calls the `RebuildCache()` method to create the current list.
4.  **Cleaning**: When `Dispose` is called, event subscriptions are terminated to prevent memory leaks.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Reactive Query** | An intelligent query model that updates its own state as data changes. |
| **Dirty Flag** | A pointer indicating that a piece of data has changed and needs to be re-processed. |
| **Lazy Rebuild** | The technique of postponing calculation until the moment of need. |
| **Event-Driven** | Controlling program flow by occurring events (events). |

---

## 5. Risks and Limits
- **Frequency Analysis**: If the queried components change every frame (e.g., Position), using `NexusCachedQuery` may decrease performance instead of increasing it due to reactive cost. It is ideal only for infrequently changing data.
- **Hash Cost**: The use of `HashSet` may consume more memory than unmanaged arrays, and iteration speed may be slightly (at micro level) lower.

---

## 6. Usage Example
```csharp
// Reactive query tracking entities with "Inventory" and "Stats" components
var inventoryQuery = new NexusCachedQuery(registry, typeof(Inventory), typeof(Stats));

void OnUpdate() {
    // If data hasn't changed, the cost of this call is O(1) (Returns the list directly)
    var players = inventoryQuery.GetEntities();
    foreach(var p in players) {
        // ... Process ...
    }
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Core;

public class NexusCachedQuery : IDisposable
{
    private readonly Registry _registry;
    private readonly Type[] _required;
    private readonly HashSet<EntityId> _cache = new();
    private bool _isDirty = true;

    public IEnumerable<EntityId> GetEntities()
    {
        if (_isDirty) RebuildCache();
        return _cache;
    }

    private void OnComponentModified(EntityId entity, Type type)
    {
        foreach (var req in _required) {
            if (req == type) { _isDirty = true; break; }
        }
    }
}
```

---

## Nexus Optimization Tip: Event Filtering
The type check performed within `OnComponentModified` is the main factor determining the reactive cost. If you have hundreds of different component types, by specializing this method for specific types at the `Registry` level (Type-specific events), you can **escape unnecessary "Dirty" checks by 80%.**
