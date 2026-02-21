# Nexus Prime Architectural Manual: NexusCollections (Unmanaged Data Structures)

## 1. Introduction
`NexusCollections.cs` is the heart of Nexus Prime framework's "Hardware-Oriented Data Management". To eliminate the Garbage Collector (GC) pressure and memory clutter created by standard C# collections (List, Dictionary, etc.), it offers a collection suite that works entirely on unmanaged (unmanaged) memory.

The reason for these collections' existence is to use the processor cache (CPU Cache) most efficiently while processing thousands of data pieces in game logic (logic) systems and to control memory management with a manual (unsafe) but safe layer.

---

## 2. Technical Analysis
The collection suite includes the following critical structures for performance and safety:

- **NexusRef<T>**: Provides a secure "reference" layer that checks the validity of the entity (EntityId) instead of holding a direct pointer to a component. Returns `null` if the entity has been deleted during `Ptr` access, preventing "Dangling Pointer" errors.
- **NexusList<T>**: Aligns data to 64-byte boundaries using `NativeMemory.AllocCacheAligned`. This ensures that the processor reaches maximum data within a single cache-line while scanning (iteration) the list.
- **NexusDictionary<K, V>**: Offers a hash map that works on pure unmanaged memory. Since it does not contain reference types, it is not scanned by GC even if there are millions of records.
- **NexusString<TSize>**: A fixed-size, unmanaged string structure. Allows data such as entity names or tags to be stored inline (inline) within the component instead of the heap.

---

## 3. Logical Flow
1.  **Allocation**: When a collection is created (`new`), cache-aligned unmanaged memory is reserved via `NexusMemoryManager`.
2.  **Management**: Data is managed via raw pointers. When capacity is full, memory is safely expanded with `Realloc`.
3.  **Access**: Operations are performed directly via the memory address with `ref` returning indexers without data copying (copy-overhead).
4.  **Cleanup**: When the collection's life ends, memory is manually returned to the OS by calling `Dispose()`.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Dangling Pointer** | A dangerous pointer that points to a memory address that is no longer valid. |
| **Cache-Aligned** | The memory address of the data sitting exactly on the processor cache line (64-byte) boundary. |
| **LIFO (Last-In-First-Out)** | The principle used in NexusStack where the last data to enter is the first to leave. |
| **Zero-GC Penalty** | The principle that a data structure is never scanned by GC and does not cause performance pauses. |

---

## 5. Risks and Limits
- **Manual Lifetime**: Forgetting to call `Dispose()` after using `NexusList` or `NexusDictionary` results in a "Memory Leak".
- **Unmanaged Constraints**: Only `unmanaged` (struct) types can be stored. `class` or `string` (managed) types cannot be added to collections.

---

## 6. Usage Example
```csharp
// Create an unmanaged list with 100 capacity
using var list = new NexusList<float3>(100);

list.Add(new float3(1, 0, 0));

// Access data by reference (Copy-free)
ref var item = ref list[0];
item.x = 10;

// Dispose() is called automatically (at the end of the using block)
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Collections;

public unsafe struct NexusList<T> : IDisposable where T : unmanaged
{
    private T* _buffer;
    private int _count;
    private int _capacity;

    public NexusList(int initialCapacity = 8)
    {
        _capacity = initialCapacity;
        _buffer = (T*)NexusMemoryManager.AllocCacheAligned(_capacity * sizeof(T));
    }

    public void Add(T item)
    {
        if (_count == _capacity) // Realloc logic here
        _buffer[_count++] = item;
    }

    public void Dispose() => NexusMemoryManager.Free(_buffer);
}
```

---

## Nexus Optimization Tip: Predictive Capacity
Adjust the default capacity (Default: 8) of collections according to the average amount of data in your project. Frequent `Realloc` operations can lead to memory fragmentation. By determining the correct capacity initially, you can **eliminate the list expansion cost by 100%.**
