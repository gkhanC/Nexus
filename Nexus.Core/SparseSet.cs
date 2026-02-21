using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Nexus.Core;

namespace Nexus.Core;

/// <summary>
/// A high-performance Sparse Set implementation for unmanaged components.
/// Sparse Sets provide O(1) Add, Remove, and Get operations while ensuring that 
/// component data remains contiguous in memory for maximum cache efficiency.
/// </summary>
/// <typeparam name="T">The unmanaged component type (must be a struct).</typeparam>
public unsafe class SparseSet<T> : ISparseSet where T : unmanaged
{
    /// <summary> 
    /// The Sparse Array: Maps an Entity index to its offset in the Dense array.
    /// This array is large and may have holes (null/uint.MaxValue).
    /// </summary>
    private uint* _sparse; 
    private int _sparseCapacity;

    /// <summary> 
    /// The Dense Array: Stores the EntityIds currently possessing this component type.
    /// This array is packed contiguously (no holes) for iteration efficiency.
    /// </summary>
    private EntityId* _dense; 
    private int _denseCount;
    private int _denseCapacity;

    /// <summary> 
    /// Bitmask for tracking 'dirty' components (modified since last frame).
    /// Uses 1 bit per component to minimize memory footprint.
    /// </summary>
    private uint* _dirtyBits; 

    /// <summary>
    /// Bitmask for tracking component presence.
    /// Essential for SIMD-accelerated filtering in queries.
    /// Bit i is set if entity with index i has this component.
    /// </summary>
    private uint* _presenceBits;
    private int _presenceCapacity; // in uints

    /// <summary> 
    /// The actual component data storage. Uses stable pointers to avoid invalidating 
    /// references when the buffer grows or reshuffles.
    /// </summary>
    private readonly ChunkedBuffer<T> _components;

    /// <summary> The total number of components currently stored in this set. </summary>
    public int Count => _denseCount;

    /// <summary> Cache-line alignment (64 bytes). Prevents CPU cache misses and False Sharing. </summary>
    private const int ALIGNMENT = 64;

    /// <summary>
    /// Initializes a new SparseSet with non-managed memory allocations.
    /// </summary>
    /// <param name="initialCapacity">The starting number of entities/components to support.</param>
    public SparseSet(int initialCapacity = 1024)
    {
        // 1. Setup Sparse Array: Every entry initialized to MaxValue (64-byte aligned).
        _sparseCapacity = initialCapacity;
        _sparse = (uint*)NativeMemory.AlignedAlloc((nuint)(_sparseCapacity * sizeof(uint)), ALIGNMENT);
        for (int i = 0; i < _sparseCapacity; i++) _sparse[i] = uint.MaxValue;

        // 2. Setup Dense Array: Packed list of owner EntityIds (64-byte aligned).
        _denseCapacity = initialCapacity;
        _dense = (EntityId*)NativeMemory.AlignedAlloc((nuint)(_denseCapacity * sizeof(EntityId)), ALIGNMENT);

        // 3. Setup Dirty Bits: Bitset with 32 components per uint (64-byte aligned).
        int dirtyCapacity = (initialCapacity / 32) + 1;
        _dirtyBits = (uint*)NativeMemory.AlignedAlloc((nuint)(dirtyCapacity * sizeof(uint)), ALIGNMENT);
        NativeMemory.Clear(_dirtyBits, (nuint)(dirtyCapacity * sizeof(uint)));

        // 3b. Setup Presence Bits: Bitset with 32 entities per uint (64-byte aligned).
        _presenceCapacity = dirtyCapacity;
        _presenceBits = (uint*)NativeMemory.AlignedAlloc((nuint)(_presenceCapacity * sizeof(uint)), ALIGNMENT);
        NativeMemory.Clear(_presenceBits, (nuint)(_presenceCapacity * sizeof(uint)));

        // 4. Setup Components: Unmanaged chunked storage for the payload data.
        _components = new ChunkedBuffer<T>(initialCapacity / 1024 + 1);
    }

    /// <summary>
    /// Adds or updates a component for a specific entity.
    /// </summary>
    /// <param name="entity">The entity handle.</param>
    /// <param name="component">The component data.</param>
    /// <returns>A permanent pointer to the component data.</returns>
    public T* Add(EntityId entity, T component = default)
    {
        // Ensure we can index into the sparse array for this entity.
        EnsureSparseCapacity(entity.Index);

        uint denseIndex = _sparse[entity.Index];
        
        // CASE A: Component already exists. Overwrite and mark dirty.
        if (denseIndex != uint.MaxValue && _dense[denseIndex] == entity)
        {
            T* existing = (T*)_components.GetPointer((int)denseIndex);
            *existing = component;
            SetDirty(denseIndex);
            return existing;
        }

        // CASE B: Component is new.
        
        // 1. Expand dense array if full.
        if (_denseCount >= _denseCapacity)
        {
            int newCapacity = _denseCapacity * 2;
            EntityId* newDense = (EntityId*)NativeMemory.AlignedAlloc((nuint)(newCapacity * sizeof(EntityId)), ALIGNMENT);
            NativeMemory.Copy(_dense, newDense, (nuint)(_denseCount * sizeof(EntityId)));
            NativeMemory.AlignedFree(_dense);
            _dense = newDense;
            _denseCapacity = newCapacity;
        }

        // 2. Update mapping: Sparse[EntityIndex] = CurrentDenseCount
        denseIndex = (uint)_denseCount;
        _dense[denseIndex] = entity;
        _sparse[entity.Index] = denseIndex;

        // 3. Allocate space in the chunked buffer and copy data.
        _components.Count = _denseCount + 1;
        T* compPtr = (T*)_components.GetPointer((int)denseIndex);
        *compPtr = component;

        // 4. Update counters and tracking.
        _denseCount++;
        SetDirty(denseIndex);
        SetPresence(entity.Index);
        return compPtr;
    }

    /// <summary>
    /// Checks if the entity has this component. 
    /// Requires O(1) double-check: Sparse[index] -> Dense[offset] == Entity.
    /// </summary>
    public bool Has(EntityId entity)
    {
        if (entity.Index >= _sparseCapacity) return false;
        uint denseIndex = _sparse[entity.Index];
        return denseIndex != uint.MaxValue && _dense[denseIndex] == entity;
    }

    /// <summary> Retrieves the owner EntityId at a specific dense index. </summary>
    public EntityId GetEntity(int denseIndex)
    {
        if (denseIndex < 0 || denseIndex >= _denseCount) return EntityId.Null;
        return _dense[denseIndex];
    }

    /// <summary> Retrieves the component pointer at a specific dense index. </summary>
    public T* GetComponent(int denseIndex)
    {
        if (denseIndex < 0 || denseIndex >= _denseCount) return null;
        return (T*)_components.GetPointer(denseIndex);
    }

    /// <summary>
    /// Retrieves a component pointer for an entity.
    /// </summary>
    public T* Get(EntityId entity)
    {
        if (entity.Index >= _sparseCapacity) return null;
        uint denseIndex = _sparse[entity.Index];
        // Validate that this entity actually owns the data at this dense position.
        if (denseIndex == uint.MaxValue || _dense[denseIndex] != entity) return null;

        return (T*)_components.GetPointer((int)denseIndex);
    }

    /// <summary>
    /// Removes a component from an entity. Uses "Swap and Pop" to maintain memory contiguity.
    /// The last element in the dense array is moved to fill the hole left by the removed element.
    /// </summary>
    public void Remove(EntityId entity)
    {
        if (entity.Index >= _sparseCapacity) return;
        uint denseIndex = _sparse[entity.Index];
        if (denseIndex == uint.MaxValue || _dense[denseIndex] != entity) return;

        uint lastDenseIndex = (uint)_denseCount - 1;

        // Logic: If we are not removing the absolute last element, we must 'swap' it into the hole.
        if (denseIndex != lastDenseIndex)
        {
            // 1. Identify the entity and component at the end of the dense array.
            EntityId lastEntity = _dense[lastDenseIndex];
            
            // 2. Move the last entity into the position of the removed entity.
            _dense[denseIndex] = lastEntity;

            // 3. Move the last component data into the hole.
            T* targetComp = (T*)_components.GetPointer((int)denseIndex);
            T* lastComp = (T*)_components.GetPointer((int)lastDenseIndex);
            *targetComp = *lastComp;

            // 4. Update the dirty state of the moved component to match its old state.
            if (IsDirty(lastDenseIndex)) SetDirty(denseIndex);
            else ClearDirty(denseIndex);

            // 5. Update the sparse index for the entity that was just moved.
            _sparse[lastEntity.Index] = denseIndex;
        }

        // 6. Invalidate the old sparse entry for the removed entity.
        _sparse[entity.Index] = uint.MaxValue;
        ClearPresence(entity.Index);

        // 7. Clear the dirty bit for the now-empty last slot.
        ClearDirty(lastDenseIndex);
        
        // 8. Pop! Decrement counters.
        _denseCount--;
        _components.Count = _denseCount;
    }

    // --- Raw Access for Jobs/SIMD ---
    public void* GetRawDense(out int count) { count = _denseCount; return _dense; }
    public void* GetRawSparse(out int capacity) { capacity = _sparseCapacity; return _sparse; }
    public void* GetRawDirtyBits(out int count) { count = (_denseCount / 32) + 1; return _dirtyBits; }
    public void* GetRawPresenceBits(out int count) { count = _presenceCapacity; return _presenceBits; }
    public void** GetRawChunks(out int count) => _components.GetRawChunks(out count);
    public int Capacity => _denseCapacity;

    // --- Dirty Bit Management ---

    /// <summary> Marks a component as modified. </summary>
    public void SetDirty(uint denseIndex)
    {
        _dirtyBits[denseIndex >> 5] |= (1u << (int)(denseIndex & 31));
    }

    /// <summary> Resets the modified flag for a component. </summary>
    public void ClearDirty(uint denseIndex)
    {
        _dirtyBits[denseIndex >> 5] &= ~(1u << (int)(denseIndex & 31));
    }

    /// <summary> Checks if a component has been modified since the last clear. </summary>
    public bool IsDirty(uint denseIndex)
    {
        return (_dirtyBits[denseIndex >> 5] & (1u << (int)(denseIndex & 31))) != 0;
    }

    // --- Presence Bit Management ---

    public void SetPresence(uint entityIndex) => _presenceBits[entityIndex >> 5] |= (1u << (int)(entityIndex & 31));
    public void ClearPresence(uint entityIndex) => _presenceBits[entityIndex >> 5] &= ~(1u << (int)(entityIndex & 31));
    public bool HasPresence(uint entityIndex) => (_presenceBits[entityIndex >> 5] & (1u << (int)(entityIndex & 31))) != 0;

    /// <summary>
    /// Resets all dirty bits to 0. Uses AVX/SSE acceleration if available for lightning-fast clears.
    /// Essential for high-performance delta tracking.
    /// </summary>
    public void ClearAllDirty()
    {
        int dirtyCount = (_denseCount / 32) + 1;
        
        // Optimization path: 256-bit AVX
        if (Avx.IsSupported && dirtyCount >= 8)
        {
            int i = 0;
            for (; i <= dirtyCount - 8; i += 8)
            {
                Avx.Store(_dirtyBits + i, Vector256<uint>.Zero);
            }
            for (; i < dirtyCount; i++) _dirtyBits[i] = 0;
        }
        // Fallback path: 128-bit SSE
        else if (Sse2.IsSupported && dirtyCount >= 4)
        {
            int i = 0;
            for (; i <= dirtyCount - 4; i += 4)
            {
                Sse2.Store(_dirtyBits + i, Vector128<uint>.Zero);
            }
            for (; i < dirtyCount; i++) _dirtyBits[i] = 0;
        }
        // Generic fallback
        else
        {
            NativeMemory.Clear(_dirtyBits, (nuint)(dirtyCount * sizeof(uint)));
        }
    }

    /// <summary>
    /// Resizes the sparse array to accommodate a new larger entity index.
    /// </summary>
    private void EnsureSparseCapacity(uint index)
    {
        if (index >= _sparseCapacity)
        {
            int newCapacity = (int)Math.Max(_sparseCapacity * 2, index + 1);
            uint* newSparse = (uint*)NativeMemory.AlignedAlloc((nuint)(newCapacity * sizeof(uint)), ALIGNMENT);
            // Copy old mapping data.
            NativeMemory.Copy(_sparse, newSparse, (nuint)(_sparseCapacity * sizeof(uint)));
            // Initialize the newly allocated section with uint.MaxValue (null).
            for (int i = _sparseCapacity; i < newCapacity; i++) newSparse[i] = uint.MaxValue;
            NativeMemory.AlignedFree(_sparse);
            _sparse = newSparse;
            _sparseCapacity = newCapacity;

            // Resize Presence Bits if needed
            int newPresenceCapacity = (newCapacity / 32) + 1;
            if (newPresenceCapacity > _presenceCapacity)
            {
                uint* newPresence = (uint*)NativeMemory.AlignedAlloc((nuint)(newPresenceCapacity * sizeof(uint)), ALIGNMENT);
                NativeMemory.Clear(newPresence, (nuint)(newPresenceCapacity * sizeof(uint)));
                NativeMemory.Copy(_presenceBits, newPresence, (nuint)(_presenceCapacity * sizeof(uint)));
                NativeMemory.AlignedFree(_presenceBits);
                _presenceBits = newPresence;
                _presenceCapacity = newPresenceCapacity;
            }
        }
    }

    /// <summary>
    /// Complete cleanup of unmanaged sparse set memory.
    /// </summary>
    public void Dispose()
    {
        if (_sparse != null) { NativeMemory.AlignedFree(_sparse); _sparse = null; }
        if (_dense != null) { NativeMemory.AlignedFree(_dense); _dense = null; }
        if (_dirtyBits != null) { NativeMemory.AlignedFree(_dirtyBits); _dirtyBits = null; }
        if (_presenceBits != null) { NativeMemory.AlignedFree(_presenceBits); _presenceBits = null; }
        _components?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary> Prevents memory leaks if the consumer forgets to call Dispose(). </summary>
    ~SparseSet() => Dispose();
}
