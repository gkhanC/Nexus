using System;
using System.Runtime.InteropServices;

namespace Nexus.Core
{
    /// <summary>
    /// NexusLayout: Handles memory alignment and cache optimization for Nexus Prime.
    /// Ensures that all unmanaged allocations follow the 64-byte boundary rule.
    /// </summary>
    public static class NexusLayout
    {
        public const int CACHE_LINE_SIZE = 64;

        /// <summary>
        /// Calculates the aligned size for a given byte count.
        /// </summary>
        public static int GetAlignedSize(int size)
        {
            return (size + CACHE_LINE_SIZE - 1) & ~(CACHE_LINE_SIZE - 1);
        }

        /// <summary>
        /// Allocates 64-byte aligned unmanaged memory.
        /// </summary>
        public static unsafe void* Alloc(int size)
        {
            return NativeMemory.AlignedAlloc((nuint)size, CACHE_LINE_SIZE);
        }

        /// <summary>
        /// Frees aligned unmanaged memory.
        /// </summary>
        public static unsafe void Free(void* ptr)
        {
            if (ptr != null)
                NativeMemory.AlignedFree(ptr);
        }
    }
}
