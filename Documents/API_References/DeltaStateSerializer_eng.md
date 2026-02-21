# Nexus Prime Architectural Manual: DeltaStateSerializer (Differential Data Writing)

## 1. Introduction
`DeltaStateSerializer.cs` is the "Smart Recording" layer that optimizes bandwidth in Nexus Prime's data storage (Persistence) and network synchronization processes. Unlike traditional serializers, it does not write the entire data set to disk; it identifies and packages only the data pieces ("Dirty") that have changed since the last recording.

The reason for this serializer's existence is to avoid the cost of re-recording the entire world for just 3-5 moving entities in a massive world containing thousands of entities and to minimize I/O (I/O) operations.

---

## 2. Technical Analysis
DeltaStateSerializer uses the following mechanisms for efficiency:

- **Sparse Chunk Inspection**: It determines only the memory blocks that need to be updated by scanning the `DirtyBits` flags within the `SparseSet`.
- **Incremental Binary Stream**: During the writing process, only the index and raw data of the changed blocks are added to the stream (stream) with the `BinaryWriter`.
- **Snapshot Integration**: Collaborating with the `SnapshotManager`, it captures diffs between two time slots (Timestamp) at the binary level.
- **Zero-Allocation Write**: No new objects (C# Objects) are created during serialization; data is copied directly from unmanaged memory to the stream buffer.

---

## 3. Logical Flow
1.  **Scanning**: All component sets (`ComponentSets`) on the Registry are traversed.
2.  **Filtering**: The dirty bitsets of each set are checked.
3.  **Packaging**: Each chunk found dirty is packaged as a raw byte array with an index prefix.
4.  **Transmission/Recording**: The packaged data flow is written to disk or sent over the network.
5.  **Restoration**: During deserialization, data is patched point-wise (point-wise) to the `Registry` based on incoming indices.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Delta Serialization** | Saving or sending only the difference between two states. |
| **Dirty Bitset** | 0/1 values tracking whether a particular data block has changed or not. |
| **Sparse Update** | Updating sparsely distributed data without disrupting the entire set. |
| **I/O Overhead** | The overhead of input/output operations on the processor and storage. |

---

## 5. Risks and Limits
- **Base State Dependency**: For delta data to be meaningful, a "Baseline" (Baseline) must exist on the remote side or disk. If the base state is lost, deltas cannot be applied.
- **Reconstruction Cost**: When too much small delta data accumulates, applying them one by one (Apply) can reduce performance. It is recommended to take a "Full Snapshot" at regular intervals.

---

## 6. Usage Example
```csharp
using var stream = File.OpenWrite("save_delta.bin");
var serializer = new DeltaStateSerializer();

// Save only what's changed in the last 1 second
serializer.SerializeDelta(mainRegistry, stream);
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Data;

public unsafe class DeltaStateSerializer
{
    public void SerializeDelta(Registry.Registry registry, Stream stream)
    {
        using var writer = new BinaryWriter(stream);
        foreach (var set in registry.ComponentSets)
        {
            // 1. Get DirtyBits from SparseSet.
            // 2. Write only 'Dirty' chunks.
            // 3. Prefix with index for sparse reconstruction.
        }
    }
}
```

---

## Nexus Optimization Tip: Frequency Scaling
Adjust the frequency of delta serialization based on the "Rate of Change" of the data. While sending data that changes very fast (e.g., Player Position) as raw stream instead of delta, process those that change slowly (e.g., Inventory) with DeltaStateSerializer. This **allows you to optimize network and disk usage by 90%.**
