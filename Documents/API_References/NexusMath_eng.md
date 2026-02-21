# Nexus Prime Architectural Manual: NexusMath (Hardware-Accelerated Math)

## 1. Introduction
`NexusMath.cs` is the engine behind Nexus Prime's computing power. Unlike standard math libraries, it uses the SIMD (Single Instruction, Multiple Data) capabilities of modern processors directly, allowing operations on multiple data pieces within a single clock cycle.

The reason for this library's existence is to eliminate CPU bottlenecks in scenarios requiring intensive data processing (high-throughput) such as physics simulations, particle systems, or artificial intelligence and to approach the theoretical limits of the hardware.

---

## 2. Technical Analysis
NexusMath follows a three-layered execution strategy for performance:

- **AVX (Advanced Vector Extensions)**: If the processor supports it (modern x86), it loads data into 256-bit wide registers. This way, **it adds or multiplies 8 float numbers with a single command.**
- **SSE (Streaming SIMD Extensions)**: Performs operations in groups of 4 using 128-bit registers on older processors without AVX.
- **Scalar Fallback**: If the data amount is not a multiple of 4 or 8, it returns to the standard single operation loop for the remaining elements.
- **Aggressive Inlining**: Methods are marked with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`. This buries the code directly where it is called, reducing method call cost (stack frame overhead) to zero.

---

## 3. Logical Flow
1.  **Hardware Detection**: CPU capabilities are detected with `Avx.IsSupported` or `Sse.IsSupported` checks.
2.  **Vectorization**: Pulled from memory into registers in blocks of 8 (AVX) or 4 (SSE) via data pointers.
3.  **Accelerated Operation**: Parallel mathematical operation is executed at the hardware level.
4.  **Write to Memory**: Results are dumped back to the target memory address (`float*`).

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **SIMD** | Technique of parallel processing of multiple data with a single command. |
| **AVX-256** | 256-bit wide processor register set with 8-float capacity. |
| **Fast Inverse Sqrt** | A very fast algorithm that calculates the inverse of the square root (`1/√x`) using a "magic" constant at bit level. |
| **Vectorization** | The process of parallelizing (vectorizing) a serial process. |

---

## 5. Risks and Limits
- **Alignment Requirement**: For best performance in AVX operations, memory addresses must be 32-byte aligned. Nexus Prime guarantees this with `NexusLayout`.
- **Precision**: Approximate functions like `FastInverseSqrt` are not suitable for financial calculations requiring 100% precision; they are designed for game mechanics.

---

## 6. Usage Example
```csharp
// Add two massive float arrays with SIMD
unsafe {
    float* a = stackalloc float[1024];
    float* b = stackalloc float[1024];
    float* result = stackalloc float[1024];
    
    NexusMath.Add(a, b, result, 1024); // Added in blocks of 8 (AVX)
}

// Fast interpolation
float smoothVal = NexusMath.FastSmoothStep(0, 1, 0.5f);
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
public static unsafe class NexusMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(float* a, float* b, float* result, int count)
    {
        int i = 0;
        if (Avx.IsSupported)
        {
            for (; i <= count - 8; i += 8) {
                var va = Avx.LoadVector256(a + i);
                var vb = Avx.LoadVector256(b + i);
                Avx.Store(result + i, Avx.Add(va, vb));
            }
        }
        // Fallback loops...
    }
}
```

---

## Nexus Optimization Tip: Instruction Pipelining
To get the highest efficiency from SIMD operations, keep your data in "Sequential" memory blocks. In case of scattered memory (Cache Miss), the processor has to wait to fill SIMD registers. **Sequential data alignment will increase SIMD throughput by 300-400%.**
