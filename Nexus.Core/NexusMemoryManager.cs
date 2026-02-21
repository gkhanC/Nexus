using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Nexus.Core
{
    /// <summary>
    /// NexusMemoryManager: Professional high-performance unmanaged memory manager.
    /// Implements page-aligned (4096 bytes) allocations to optimize for CPU MMU 
    /// and ensures strict cache-line awareness for all Nexus components.
    /// </summary>
    public static unsafe class NexusMemoryManager
    {
        /// <summary> Standard system page size (4KB). Optimal for memory mapping and MMU. </summary>
        public const int PAGE_SIZE = 4096;
        
        /// <summary> Cache line size (64B). Prevents false sharing and optimizes L1 cache. </summary>
        public const int CACHE_LINE = 64;

        /// <summary>
        /// Allocates a block of unmanaged memory aligned to the system page size.
        /// Page alignment is critical for high-bandwidth data transfers (SIMD/DMA).
        /// </summary>
        /// <param name="size">Total bytes to allocate.</param>
        /// <returns>A pointer to the allocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocPageAligned(int size)
        {
            void* ptr = NativeMemory.AlignedAlloc((nuint)size, PAGE_SIZE);
            if (ptr == null) throw new OutOfMemoryException($"[Nexus.Memory.Critical] Failed to allocate page-aligned unmanaged block of {size} bytes. System memory pressure is critical.");
            return ptr;
        }

        /// <summary>
        /// Specialized allocation for component buffers, ensuring cache-line alignment.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocCacheAligned(int size)
        {
            return NativeMemory.AlignedAlloc((nuint)size, CACHE_LINE);
        }

        /// <summary>
        /// Releases unmanaged memory back to the OS.
        /// </summary>
        /// <param name="ptr">Pointer to the memory block.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* ptr)
        {
            if (ptr != null)
            {
                NativeMemory.AlignedFree(ptr);
            }
        }

        /// <summary>
        /// Zero-initializes a memory block using high-performance block operations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(void* ptr, int size)
        {
            NativeMemory.Clear(ptr, (nuint)size);
        }

        /// <summary>
        /// Copies memory using the fastest available hardware path.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(void* source, void* destination, int size)
        {
            NativeMemory.Copy(source, destination, (nuint)size);
        }

        /// <summary>
        /// Reallocates memory while preserving alignment.
        /// </summary>
        public static void* Realloc(void* ptr, int oldSize, int newSize, int alignment = CACHE_LINE)
        {
            void* newPtr = NativeMemory.AlignedAlloc((nuint)newSize, (nuint)alignment);
            if (ptr != null)
            {
                int copySize = Math.Min(oldSize, newSize);
                NativeMemory.Copy(ptr, newPtr, (nuint)copySize);
                NativeMemory.AlignedFree(ptr);
            }
            return newPtr;
        }
    }
}
