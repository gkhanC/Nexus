using System;

namespace Nexus.Core
{
    /// <summary>
    /// A type-erased interface for Sparse Set storage.
    /// Allows the Registry to manage multiple typed sets in a single collection.
    /// </summary>
    public interface ISparseSet : IDisposable
    {
        /// <summary> Current number of active components in this set. </summary>
        int Count { get; }

        /// <summary> Total internal capacity of the storage arrays. </summary>
        int Capacity { get; }

        /// <summary> Returns the EntityId handle at a specific dense buffer position. </summary>
        EntityId GetEntity(int denseIndex);

        // --- Low-level Raw Memory Access for multi-threaded Jobs ---

        /// <summary> Raw pointer to the packed EntityId dense array. </summary>
        unsafe void* GetRawDense(out int count);

        /// <summary> Raw pointer to the sparse mapping array. </summary>
        unsafe void* GetRawSparse(out int capacity);

        /// <summary> Raw pointer to the dirty-bit tracking mask. </summary>
        unsafe void* GetRawDirtyBits(out int count);
        
        /// <summary> Raw pointer to the presence-bit tracking mask. </summary>
        unsafe void* GetRawPresenceBits(out int count);
 
        /// <summary> Raw pointers to the unmanaged memory chunks containing component data T. </summary>
        unsafe void** GetRawChunks(out int count);
        
        /// <summary> Returns true if the entity has this component. </summary>
        bool Has(EntityId entity);

        /// <summary> Resets all changed-state tracking flags for this type. </summary>
        void ClearAllDirty();
    }
}
