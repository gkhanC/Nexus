# Nexus Prime Architectural Manual: ChunkedBuffer (Paged Memory Management)

## 1. Introduction
`ChunkedBuffer.cs` is the guarantee of "Memory Stability" for Nexus Prime. When standard unmanaged arrays (`T*`) are expanded (realloc), all data is moved to a new memory address, which can cause old pointers (those pointing to invalid addresses) to crash the system.

ChunkedBuffer solves this problem by keeping data in **16KB fixed pages (Chunks)**. Even if the buffer grows, the addresses of old pages do not change. This feature allows high-performance systems to safely look at data directly via pointers "forever" (or until the component is removed).

---

## 2. Technical Analysis
ChunkedBuffer utilizes the following advanced techniques in the memory hierarchy:

- **Segmented Allocation**: Memory is managed in 16KB (OS-friendly) pieces rather than a single massive block.
- **Pointer Stability**: The `Expand()` operation only adds a new page; it does not change the physical location of existing pages in RAM.
- **O(1) Address Math**: The address of data at any index is calculated instantly using modular arithmetic with the formula `Base + Header + (Index * Size)`.
- **Cache-Line Padding**: The beginning of each chunk is aligned to 64 bytes (ALIGNMENT). This ensures that the first element fits perfectly into the processor's cache (L1 Cache).

---

## 3. Logical Flow
1.  **Calculation**: Based on the size of the type (`T`), it is determined how many elements will fit into 16KB (16384 bytes) (`_elementsPerChunk`).
2.  **Addressing (`GetPointer`)**: With `Index / ElementsPerChunk`, which page it's on is found, and with `Index % ElementsPerChunk`, the offset within the page is found.
3.  **Expansion (`Expand`)**: When capacity is full, a new 16KB block is allocated using `NativeMemory.AlignedAlloc` and added to the main table (`_chunks`).
4.  **Cleanup**: When `Dispose` is called, each page pointer in the main table is released individually (Hardware-safe cleanup).

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Pointer Stability** | The condition where the memory address of data does not change even if the structure grows. |
| **Memory Paging** | A technique of dividing large data into small, fixed-size pages (OS-level technique). |
| **AlignedAlloc** | Allocating memory starting from addresses that are multiples of a specific number (e.g., 64). |
| **Fragmentation** | Small, inefficient gaps in memory that are unused. ChunkedBuffer keeps this under control. |

---

## 5. Risks and Limits
- **Internal Waste**: Small gaps may remain at the end of 16KB blocks depending on the type size. (Nexus accepts this cost for pointer stability).
- **Manual Lifetime**: As it is an unmanaged structure, it must be `Dispose`d, otherwise, it will not be cleared until RAM is full.

---

## 6. Usage Example
```csharp
var buffer = new ChunkedBuffer<Velocity>(1024);

// Get the pointer
Velocity* vPtr = (Velocity*)buffer.GetPointer(500);

// Grow the buffer (Does not break pointer stability!)
buffer.Count = 5000;

// vPtr still points to a valid address!
vPtr->X = 10.0f;
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
using System.Runtime.InteropServices;
namespace Nexus.Core;

public unsafe class ChunkedBuffer<T> : IDisposable where T : unmanaged
{
    private const int CHUNK_SIZE = 16 * 1024; 
    private const int ALIGNMENT = 64; 
    private readonly int _elementsPerChunk;
    private void** _chunks; 
    private int _chunkCount;
    private int _count;

    public void* GetPointer(int index)
    {
        int chunkIdx = index / _elementsPerChunk;
        int offset = index % _elementsPerChunk;
        byte* chunkBase = (byte*)_chunks[chunkIdx];
        return chunkBase + ALIGNMENT + (offset * sizeof(T));
    }

    private void Expand()
    {
        void* newChunk = NativeMemory.AlignedAlloc(CHUNK_SIZE, ALIGNMENT);
        NativeMemory.Clear(newChunk, CHUNK_SIZE);
        _chunks[_chunkCount++] = newChunk;
    }
    
    // ... Disposal logic
}
```

---

## Nexus Optimization Tip: Addressing Arithmetic
The address calculation logic of ChunkedBuffer (division/mod operations that can be converted into `Shift` and `AND` operations) takes only **1 or 2 clock cycles** on modern CPU ALUs (Arithmetic Logic Units). In contrast, traditional dynamic array reallocation (allocation) imposes a cost of **thousands of cycles and heavy memory movement (memcpy)**.
