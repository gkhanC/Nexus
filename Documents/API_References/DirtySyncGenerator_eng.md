# Nexus Prime Architectural Manual: DirtySyncGenerator (Automated Synchronization Worker)

## 1. Introduction
`DirtySyncGenerator.cs` is a draft "Job Generator" (Job Generator) designed to transfer Nexus Prime's unmanaged data to Unity's `Transform` components in maximum parallelism. It distributes the synchronization cost to multiple CPU cores using Unity's `C# Job System` and `TransformAccessArray` architecture instead of the standard `Update` loop.

The reason for this generator's existence is to avoid locking the main thread (Main Thread) while synchronizing the positions of thousands of entities and to push data to Unity's "Internal Physics/Transform" system using the fastest path.

---

## 2. Technical Analysis
Predicts the following strategies for accelerating synchronization:

- **Dirty Bit-Sweep**: Scans dirty flags (Dirty Flags) on `SparseSet` with a sweep (sweep) logic to identify only changed position data.
- **Parallel Execution**: After the changed data is identified, it writes the data to Transforms in parallel threads using Unity's `IJobParallelForTransform` interface.
- **Burst Compatibility**: The code structure is optimized to be fully compatible with the Unity Burst compiler (using blittable data).
- **Selective Dispatch**: Instead of synchronizing everything every frame, it "Dispatches" only those entities that visually need to change at that moment.

---

## 3. Logical Flow
1.  **Analysis**: The dirty flag array of the Position component on the unmanaged Registry is scanned.
2.  **Mapping**: Changed unmanaged data and their corresponding Unity Transforms are matched in a fast array (AccessArray).
3.  **Execution (Job)**: Unity Job System is triggered and data is copied in the background (Worker Threads).
4.  **Finalization**: Dirty flags are cleaned when synchronization is finished.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **C# Job System** | Structure distributing heavy computational tasks to parallel cores within Unity. |
| **TransformAccessArray** | Special data structure providing bulk and performant access to Unity Transform data. |
| **Bit-Sweep** | Rapid scanning of bit flags in memory from beginning to end. |
| **Sync Dispatch** | Sending the data copy process to a worker queue (Job Queue) to be executed. |

---

## 5. Usage Scenario
This component is usually managed by a "Source Generator" or "NexusInitializer". The developer only calls the `RunSync` method every frame; the system's parallel workers handle the rest.

---

## 6. Full Source Implementation (Conceptual Implementation)

```csharp
namespace Nexus.Bridge;

public class DirtySyncGenerator : MonoBehaviour
{
    public void RunSync(Registry.Registry registry)
    {
        // 1. Get bitmask of changed positions
        // 2. Dispatch Parallel Transform Job
    }
}
```

---

## Nexus Optimization Tip: Transform-Only Update
In Unity, if only the positions of objects change, target only `Transform.position` instead of updating the whole hierarchy (Scale/Rotation). This constraint combined with `DirtySyncGenerator` can **reduce Transform synchronization cost by an additional 20%.**
