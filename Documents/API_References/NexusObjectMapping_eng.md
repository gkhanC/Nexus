# Nexus Prime Architectural Manual: NexusObjectMapping (Unmanaged-Managed Bridge)

## 1. Introduction
`NexusObjectMapping.cs` is the critical connection point between Nexus Prime's unmanaged ECS world and Unity's managed object world (GameObject, Transform, Renderer). In a pure ECS structure, entities are just numbers (IDs), but to have a representation on screen, they need to quickly reach Unity objects.

The reason for this mapper's existence is to reach the relevant Unity object via entity ID in **O(1)** complexity and a thread-safe manner, instead of calling heavy Unity operations like `GetComponent` or `Find` for thousands of entities every frame.

---

## 2. Technical Analysis
NexusObjectMapping uses the following techniques to support hybrid architecture:

- **Concurrent Mapping**: Uses `ConcurrentDictionary<uint, object>` to manage mapping requests coming from different threads without locking.
- **Reference Bridging**: Keeps the reference of the managed object in memory by using the index of the unmanaged entity as a key. In this way, the `Registry` data and the visual object are connected to each other.
- **Generic Access**: Automatically casts the returned object to the target type (e.g., Transform) with the `Get<T>` method, which makes the developer code cleaner.
- **Zero-Cost Lookup**: After mapping is done, the cost of reaching an object is only as much as a hash table read.

---

## 3. Logical Flow
1.  **Mapping (`Map`)**: When an entity is created and a corresponding GameObject is instantiated, the two are connected to each other with this class.
2.  **Inquiry (`Get`)**: While systems are executing business logic, they pull the relevant object with `NexusObjectMapping.Get<Transform>(id)` for visual updates.
3.  **Synchronization**: Changes in unmanaged data are transferred to the Unity object via this bridge.
4.  **Cleaning (`Unmap`)**: When an entity is deleted, the mapping record is deleted to prevent memory leaks.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Object Mapping** | Matching objects between two different worlds (unmanaged/managed) with each other. |
| **Hybrid ECS** | Strategy of using both pure data structures (unmanaged) and engine objects (managed) together. |
| **Type Casting** | Forcing an object to be treated as if it were a certain type (Object -> Transform). |
| **Sync Bridge** | Structure providing data flow between the data layer and the visual layer. |

---

## 5. Risks and Limits
- **GC Pressure**: Since key-value pairs in `ConcurrentDictionary` reside on the managed heap, GC (Garbage Collector) pressure may occur if millions of mappings are made. Only entities with visual representations should be mapped.
- **Lifetime Management**: If a GameObject is `Destroy`ed on the Unity side but not `Unmap`ed on the Nexus side, "MissingReferenceException" risks arise.

---

## 6. Usage Example
```csharp
// Connect an entity and a visual object together
EntityId entity = registry.Create();
GameObject go = Instantiate(prefab);
NexusObjectMapping.Map(entity.Index, go.transform);

// Transfer data to visual within the system
void OnUpdate(EntityId id, Position* pos) {
    if (NexusObjectMapping.TryGet(id.Index, out var obj)) {
        var trans = (Transform)obj;
        trans.position = pos->Value;
    }
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
using System.Collections.Concurrent;
namespace Nexus.Core;

public static class NexusObjectMapping
{
    private static readonly ConcurrentDictionary<uint, object> _mappings = new();

    public static void Map(uint entityIndex, object unityObject)
    {
        _mappings[entityIndex] = unityObject;
    }

    public static T Get<T>(uint entityIndex) where T : class
    {
        if (_mappings.TryGetValue(entityIndex, out var obj)) return obj as T;
        return null;
    }

    public static void Unmap(uint entityIndex)
    {
        _mappings.TryRemove(entityIndex, out _);
    }
}
```

---

## Nexus Optimization Tip: Predictive Pre-Mapping
If you are using thousands of bullets or particles, instead of creating a Mapping for each, map to a "Pool" system. By updating the Mapping when the bullet object leaves the pool, you can **reduce dictionary insertion/removal cost by 90%.**
