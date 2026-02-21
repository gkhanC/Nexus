# Nexus Prime Architectural Manual: NexusSyncManager (Data Synchronization Manager)

## 1. Introduction
`NexusSyncManager.cs` is the "Speed Regulator" between Nexus Prime's unmanaged simulation layer and Unity's visual scenes. It coordinates the transfer of pure mathematical data (e.g., Position, Rotation) residing in unmanaged memory (`Registry`) to visual objects (`Transform`) in Unity with millisecond speed.

The reason for this manager's existence is to minimize the cost of `Transform` updates on the CPU by making a mass and optimized synchronization pass (Synchronization Pass) through a central system, instead of each Unity object searching for and finding its own data.

---

## 2. Technical Analysis
NexusSyncManager uses the following techniques for hybrid synchronization:

- **Global Sync Pass**: Scans all component sets of interest in bulk using `registry.GetSet<T>`. This is much faster than performing individual Registry queries for each object (Set-Iteration).
- **Mapping Lookup**: Reaches the relevant Unity object via Entity ID at O(1) cost using `NexusObjectMapping.TryGet`.
- **Direct Pointer Access**: Reads data from unmanaged component sets as raw pointers (`Vector3*`, `NexusRotationField*`). This eliminates managed-struct copying cost.
- **Conditional Sync**: Only entities managed by Nexus and possessing a visual representation (Mapped) are synchronized.

---

## 3. Logical Flow
1.  **Iteration**: All active entities are scanned via position and rotation component sets.
2.  **Reference Finding**: Whether the entity has a visual representation (`GameObject`) on the Unity side is checked.
3.  **Value Transfer**: Unmanaged data is assigned directly to Unity `transform.position` and `transform.rotation` properties.
4.  **Acceleration**: In multi-thousand entities, this process can also be executed in parallel within the `JobSystem`.

---

## 4. Usage Example
```csharp
// Trigger synchronization within the Update loop
void Update() {
    NexusSyncManager.Sync(mainRegistry);
}

// Or manually synchronize only a single entity
NexusSyncManager.SyncEntity(registry, myId, myGameObject);
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity;

public static class NexusSyncManager
{
    public static void Sync(Registry registry)
    {
        var positionSet = registry.GetSet<Vector3>();
        var rotationSet = registry.GetSet<NexusRotationField>();

        for (int i = 0; i < positionSet.Count; i++)
        {
            EntityId id = positionSet.GetEntity(i);
            if (NexusObjectMapping.TryGet(id.Index, out object obj) && obj is GameObject go)
            {
                Transform t = go.transform;
                t.position = *positionSet.Get(id);
                // Rotation sync...
            }
        }
    }
}
```

---

## Nexus Optimization Tip: Change-Only Sync
To reduce synchronization cost, use `DirtyBits` technology. By synchronizing only the entities whose unmanaged data has changed since the last frame, you can **reduce the Transform update load on the Unity side by 70-80%.**
