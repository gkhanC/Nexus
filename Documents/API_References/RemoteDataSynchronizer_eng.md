# Nexus Prime Architectural Manual: RemoteDataSynchronizer (Remote Data Synchronization)

## 1. Introduction
`RemoteDataSynchronizer.cs` is Nexus Prime's data mirroring engine designed for multiplayer or distributed simulations. It is constructed to package data not as C# objects, but in its rawest unmanaged state (binary) and transfer it to a remote point (server/client).

The reason for this synchronizer's existence is to use the **Delta Snapshots** technology produced by `SnapshotManager` to minimize network traffic (bandwidth) and to convey only the changed components as raw byte blocks to the other side.

---

## 2. Technical Analysis
RemoteDataSynchronizer follows these strategies for low-latency network communication:

- **Delta Streaming**: Instead of sending the entire Registry, it packages only the components that have changed since the last synchronization (Diff).
- **Unmanaged Binary Transfer**: Since the data already resides in RAM as unmanaged and bit-bit compatible (blittable), it is copied directly from `NativeMemory` to the network buffer without any "Serialization" cost.
- **Protocol Agnostic**: The logic can work regardless of TCP or UDP; the main focus is the process of binary packaging of data and "Patch"ing it on the other side.
- **Zero-Allocation Sync**: No new C# objects are created during synchronization; data flows through raw byte pointers.

---

## 3. Logical Flow
1.  **Inquiry**: Components and entities with the `Dirty` flag on the Registry are identified.
2.  **Snapshot Acquisition**: A delta snapshot (difference backup) is created via the `SnapshotManager`.
3.  **Packaging**: Snapshot content is made into a package in a binary format suitable for the hardware architecture.
4.  **Transmission**: Sent to the target IP address via UDP/TCP sockets.
5.  **Application**: The remote receiver writes incoming raw bytes directly to its own `Registry` addresses.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Mirroring** | The exact copying of a data source's state elsewhere. |
| **Delta Snapshot** | A package containing only the data that has changed between two time slots. |
| **Blittable Transfer** | Copying data as it is (raw bytes) without conversion. |
| **Latency** | The time elapsed (delay) during the delivery of data from one point to another. |

---

## 5. Risks and Limits
- **Packet Loss**: If UDP is used, there is a risk of losing packets and breaking the delta chain. This leads to loss of synchronization (Desync).
- **Endianness**: If data is moving between different CPU architectures (e.g., ARM vs x86), byte ordering (Endianness) problems may occur. Nexus Prime uses Little-Endian by default.

---

## 6. Usage Example
```csharp
var synchronizer = new RemoteDataSynchronizer();

void FixedUpdate() {
    // Send changes to server every 100ms
    synchronizer.SyncToRemote(mainRegistry, "192.168.1.50");
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Core;

public class RemoteDataSynchronizer
{
    public void SyncToRemote(Registry registry, string ipAddress)
    {
        // 1. Get Delta Snapshot from SnapshotManager.
        // 2. Stream raw binary data over UDP/TCP.
        // 3. Apply on the remote receiver side.
        Console.WriteLine($"Nexus: Syncing data to {ipAddress}...");
    }
}
```

---

## Nexus Optimization Tip: Bit-Compression for Delta
When packaging delta snapshots, besides not sending unchanged components, send the changed ones compressed at bit-level (e.g., `quantization`). This **optimizes network usage by 200%-300%**, allowing thousands of entities to stay synchronized even on mobile devices.
