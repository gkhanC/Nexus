# Nexus Prime Architectural Manual: NexusEntity (Unity-ECS Identity Bridge)

## 1. Introduction
`NexusEntity.cs` is the most fundamental component in Nexus Prime's hybrid architecture. It acts as the "Identity Link" matching a Unity `GameObject` with an `EntityId` in Nexus's unmanaged world.

The reason for this component's existence is to ensure that visual objects in Unity's scenes can synchronize with the high-performance data-driven logic layer (Simulation). Each NexusEntity is actually the Unity-side "Representative" of an entity in the ECS world.

---

## 2. Technical Analysis
NexusEntity follows these strategies for identity management:

- **Entity Identification**: Stores the `EntityId` (Index and Version) assigned to the entity. This ID is the key used to reach data on the unmanaged `Registry`.
- **Strict Singleton Component**: Guarantees that an object is represented by only a single ECS entity with `[DisallowMultipleComponent]`.
- **Auto-Initialization**: If the object was not created by an ECS system (e.g., Standard Unity Instantiation), it creates a temporary virtual identity using Unity's `InstanceID` at the moment of `Awake`.
- **Read-Only Inspection**: Allows the ID to appear on the Inspector but protects data integrity by preventing manual modification.

---

## 3. Logical Flow
1.  **Awake**: Identity is checked when the object becomes active in the scene.
2.  **Mapping**: If the ID is invalid, a virtual ECS identity is assigned to the object.
3.  **Service**: Other Unity components (e.g., `NexusSyncTransform`) pull data from the Nexus Registry using the `Id` property of this object.

---

## 4. Usage Example
```csharp
// Learn the ECS identity of an object
var entity = GetComponent<NexusEntity>();
if (entity.Id.IsNotNull) {
    Debug.Log($"Entity Index: {entity.Id.Index}");
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity;

[DisallowMultipleComponent]
public class NexusEntity : MonoBehaviour
{
    [SerializeField, ReadOnly] private EntityId _id = EntityId.Null;

    public EntityId Id {
        get => _id;
        internal set => _id = value;
    }

    private void Awake() {
        if (_id.IsNull) _id = new EntityId { Index = (uint)gameObject.GetInstanceID(), Version = 0 };
    }
}
```

---

## Nexus Optimization Tip: Explicit ID Assignment
When instantiating objects in a hybrid project, manually match the `NexusEntity.Id` value with a real ID you created via the Nexus Registry. The use of virtual IDs (InstanceID based) **may not work fully compatibly with unmanaged systems on the Registry.**
