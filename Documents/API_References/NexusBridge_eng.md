# Nexus Prime Architectural Manual: NexusBridge (Generic Synchronization Interface)

## 1. Introduction
`NexusBridge.cs` is the most basic and generic "Protocol Layer" that enables Nexus Prime to talk with any game engine (Unity, Unreal, Godot, etc.). It is a high-performance template defining how unmanaged simulation data will exit to the outside world (`Push`) and how outside world data will enter the simulation (`Pull`).

The reason for this structure's existence is to minimize the cost of engine integration by offering a type-safe (type-safe) and memory-efficient (memory-efficient) standard, instead of writing separate synchronization codes for each component type.

---

## 2. Technical Analysis
Manages these two main flow models for maximum efficiency:

- **Push (Nexus -> Engine)**: After the simulation is over, it reflects data marked as "Dirty" (Dirty) in the unmanaged Registry to visual objects of the external engine. Optimizes CPU load by processing only those that have changed (Sparse Iteration).
- **Pull (Engine -> Nexus)**: Before the simulation starts, it pulls changes in the outside world (e.g., user keyboard input or Unity physics engine results) into unmanaged memory.
- **BridgeOrchestrator**: A scheduler managing the synchronization frequency (e.g., 30 FPS or 60 FPS). Ensures visual transmission stays stable regardless of simulation speed.
- **Zero-Allocation Sync**: Since copy operations are performed via Raw Pointers (`T*`), no managed objects are created (GC-Free).

---

## 3. Logical Flow
1.  **Iteration**: All entities in the relevant component set are scanned rapidly.
2.  **Dirty Check (Dirty Check)**: Only those marked with `SetDirty` are filtered.
3.  **Data Transfer**: Data is copied between memory addresses via delegates (Callback).
4.  **Cleaning**: Dirty flags are cleaned at the end of synchronization.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Pull Strategy** | Direction of data flow from engine to ECS. |
| **Push Strategy** | Direction of data flow from ECS to engine (Visualization). |
| **Frequency Capping** | Limiting synchronization at a specific speed to prevent unnecessary processing load. |
| **Bi-Directional Sync** | Structure where data can flow safely in both directions. |

---

## 5. Usage Example
```csharp
// Example of manual synchronization
NexusBridge<Position>.Push(registry, (id, posPtr) => {
    // Apply data to Unity object
    ApplyToTransform(id, posPtr->ToVector3());
});
```

---

## 6. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Bridge;

public unsafe class NexusBridge<T> where T : unmanaged
{
    public static void Push(Registry.Registry registry, PushDelegate pushCallback) {
        var set = registry.GetSet<T>();
        for (int i = 0; i < set.Count; i++) {
            if (set.IsDirty((uint)i)) {
                pushCallback(set.GetEntity(i), set.GetComponent(i));
                set.ClearDirty((uint)i);
            }
        }
    }

    public static void Pull(Registry.Registry registry, PullDelegate pullCallback) {
        var set = registry.GetSet<T>();
        for (int i = 0; i < set.Count; i++) {
            pullCallback(set.GetEntity(i), set.GetComponent(i));
            set.SetDirty((uint)i);
        }
    }
}
```

---

## Nexus Optimization Tip: Selective Dirty Clearing
Instead of always calling `ClearDirty` after the `Push` process is finished, if you are sending data to both video recording and network (Network) systems, keep the flag until all systems read it. This **prevents unnecessarily re-calculating the same data for multiple systems.**
