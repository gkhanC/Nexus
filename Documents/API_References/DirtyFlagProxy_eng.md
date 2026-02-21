# Nexus Prime Architectural Manual: DirtyFlagProxy (Dirty Flag Synchronization)

## 1. Introduction
`DirtyFlagProxy.cs` is the most technical implementation point of Nexus Prime's "Send Only What's Changed" philosophy. Instead of scanning thousands of entities in unmanaged component sets every frame and copying them to Unity, it is a high-performance interface that ensures only those that have experienced value changes since the last frame are determined and synchronized.

The reason for this proxy's existence is to reduce unnecessary data transfer cost (Overhead) to a level close to zero in massive simulations (e.g., 100,000 entities) and to use CPU cache (Cache) efficiently.

---

## 2. Technical Analysis
DirtyFlagProxy uses these advanced techniques for performance:

- **Raw Bitmask Access**: Accesses the dirty flag array (`DirtyBits`) storing 1-bit-per-entity (1 bit for each entity) within the `SparseSet` via a raw pointer.
- **32-Bit Block Skipping**: Instead of traversing the dirty flag array bit-by-bit, it scans it in 32-bit (`uint`) blocks. If a whole block is 0 (i.e., all 32 entities have not changed), it skips 32 entities in a single operation.
- **Bitwise Evaluation**: Performs the `(mask & (1u << bit)) != 0` bitwise check to identify the entity that is dirty within the block. This is a nanosecond-level operation on modern processors.
- **Generic Delegate Sync**: When it finds the entity that has changed, it ensures the data is patched into the Unity target (Renderer, Transform, etc.) by calling the `SyncDelegate` method given by the developer.

---

## 3. Logical Flow
1.  **Preparation**: The dirty data mask (Dirty Mask) of the relevant component set is pulled from RAM.
2.  **Block Scanning**: A uint value different from 0 is searched for within the mask.
3.  **Precise Detection**: Which bit is 1 within the 32-group is found.
4.  **Triggering**: Only the unmanaged pointer and ID of the entity that is "Dirty" are sent to the synchronization callback.
5.  **Reset**: After the process is finished, the mask is cleaned and prepared for the next frame.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Dirty Bitmask** | Memory area representing the status of each element in a data group with a single bit. |
| **Bitwise AND** | Finding common bits by subjecting two numerical values to bit-based "AND" operation. |
| **Sparse Enumeration** | Traversing only the elements of a set that comply with certain rules (e.g., only those that have changed). |

---

## 5. Usage Example
```csharp
// Update Renderer colors only when they change
DirtyFlagProxy<ColorComponent>.Sync(registry, (id, colorPtr) => {
    if (NexusObjectMapping.TryGet(id.Index, out var renderer)) {
        ((Renderer)renderer).material.color = colorPtr->ToColor();
    }
});
```

---

## 6. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Bridge;

public unsafe class DirtyFlagProxy<T> where T : unmanaged
{
    public static void Sync(Registry.Registry registry, SyncDelegate syncCallback)
    {
        SparseSet<T> set = registry.GetSet<T>();
        uint* dirtyBits = (uint*)set.GetRawDirtyBits(out int bitCount);

        for (int i = 0; i < bitCount; i++) {
            uint mask = dirtyBits[i];
            if (mask == 0) continue; // Skip 32 entities at once

            for (int bit = 0; bit < 32; bit++) {
                if ((mask & (1u << bit)) != 0) {
                    syncCallback(set.GetEntity(i * 32 + bit), set.GetComponent(i * 32 + bit));
                }
            }
            dirtyBits[i] = 0; // Clear the mask
        }
    }
}
```

---

## Nexus Optimization Tip: Early Out Block Skipping
The `mask == 0` check is the biggest factor increasing "Sparse Query" performance in modern ECS engines. If only 1% of the simulation changes frame-wise, the amount of data to be scanned is **reduced by 99%** thanks to this check.
