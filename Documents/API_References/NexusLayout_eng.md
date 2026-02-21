# Nexus Prime Architectural Manual: NexusLayout (Memory Alignment & Cache Optimization)

## 1. Introduction
`NexusLayout.cs` is the most fundamental utility class that determines the hardware-level precision of the Nexus Prime framework. Modern processors pull data from memory in blocks called "Cache Lines" (typically 64 bytes).

The reason for this class's existence is to ensure that all unmanaged memory allocations and component arrangements are perfectly aligned with these 64-byte boundaries. In this way, the processor is prevented from unnecessarily scanning two different cache lines (Cache Line Split) to read a single piece of data.

---

## 2. Technical Analysis
NexusLayout applies the following hardware rules for memory performance:

- **64-Byte Boundary Rule**: Uses 64-byte boundaries, which are the most optimized addressing for the processor's MMU (Memory Management Unit).
- **Aligned Allocation**: By using `NativeMemory.AlignedAlloc`, it guarantees that the memory address obtained from the operating system is exactly a multiple of 64.
- **Size Padding**: If the size of a data structure is not an exact multiple of 64, it is rounded to the next safe boundary with the `GetAlignedSize` function.
- **MMU Efficiency**: The use of Page Aligned and Cache Aligned memory increases the processor's TLB (Translation Lookaside Buffer) hit rate.

---

## 3. Logical Flow
1.  **Calculation**: The amount of required memory is taken.
2.  **Alignment**: Rounding is performed to the nearest multiple of 64 bytes with `GetAlignedSize`.
3.  **Allocation**: Hardware-supported aligned allocation is performed over `NativeMemory`.
4.  **Freeing**: Aligned memory is returned with a specific method (`AlignedFree`).

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Cache Line (64B)** | The amount of data the processor pulls from RAM in a single move. |
| **Memory Alignment** | The requirement that a data memory address must be a multiple of a certain number. |
| **Cache Line Split** | Performance loss resulting from a piece of data overflowing into two different cache lines. |
| **MMU** | The hardware unit that converts virtual memory addresses to physical RAM addresses. |

---

## 5. Risks and Limits
- **Memory Overhead**: Aligning small data to 64 bytes causes "Padding" (gaps) in memory and therefore a certain increase in RAM usage. Memory is sacrificed for performance.
- **Unaligned Access**: If a manual allocation is made outside of `NexusLayout`, the processor may throw a "Misaligned Access" error or reduce performance by 50%.

---

## 6. Usage Example
```csharp
// Calculate aligned size for an area of 100 bytes
int rawSize = 100;
int alignedSize = NexusLayout.GetAlignedSize(rawSize); // Returns 128

// Allocate aligned memory
unsafe {
    void* ptr = NexusLayout.Alloc(alignedSize);
    // ... Usage ...
    NexusLayout.Free(ptr);
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Core;

public static class NexusLayout
{
    public const int CACHE_LINE_SIZE = 64;

    public static int GetAlignedSize(int size)
    {
        return (size + CACHE_LINE_SIZE - 1) & ~(CACHE_LINE_SIZE - 1);
    }

    public static unsafe void* Alloc(int size)
    {
        return NativeMemory.AlignedAlloc((nuint)size, CACHE_LINE_SIZE);
    }

    public static unsafe void Free(void* ptr)
    {
        if (ptr != null) NativeMemory.AlignedFree(ptr);
    }
}
```

---

## Nexus Optimization Tip: Cache Boundary Safety
Using `NexusLayout` **minimizes the time the processor takes to reach data on a clock cycle basis.** While unaligned memory causes a "Stall" in the L1 cache, data allocated with `NexusLayout` flows through the processor pipeline without any hitch.
