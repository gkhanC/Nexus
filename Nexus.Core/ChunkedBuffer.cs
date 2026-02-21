using System.Runtime.InteropServices;

namespace Nexus.Core;

/// <summary>
/// A high-performance, unmanaged buffer that stores data in fixed-size 16KB chunks.
/// Unlike standard lists, ChunkedBuffer ensures **pointer stability**: once a pointer 
/// is retrieved for an element at 'index', that pointer remains valid even if the 
/// buffer grows or reshuffles. This is critical for high-speed ECS data access.
/// </summary>
/// <typeparam name="T">The unmanaged type to store (must be a struct).</typeparam>
public unsafe class ChunkedBuffer<T> : IDisposable where T : unmanaged
{
    /// <summary> Standard memory chunk size (16KB). Balances internal waste vs overhead. </summary>
    private const int CHUNK_SIZE = 16 * 1024; 
    /// <summary> Cache-line alignment (64 bytes). Prevents CPU cache misses and aligns with modern architecture. </summary>
    private const int ALIGNMENT = 64; 

    /// <summary> Pre-calculated number of elements of type T that fit into a single 16KB chunk. </summary>
    private readonly int _elementsPerChunk;
    /// <summary> Padding at the start of each chunk to ensure alignment for the first element. </summary>
    private readonly int _headerSize;

    /// <summary> Pointer to an array of pointers, each pointing to a 16KB unmanaged memory block. </summary>
    private void** _chunks; 
    /// <summary> Number of chunks currently allocated. </summary>
    private int _chunkCount;
    /// <summary> Total capacity of the '_chunks' pointer array itself. </summary>
    private int _chunkCapacity;

    /// <summary> Backing field for current logical element count. </summary>
    private int _count;

    /// <summary>
    /// Gets or sets the number of elements in the buffer. 
    /// If increased, the buffer ensures enough unmanaged chunks are allocated.
    /// </summary>
    public int Count
    {
        get => _count;
        set
        {
            // Expansion Logic: Keep adding chunks until the capacity exceeds requested count.
            while (value > Capacity) Expand();
            _count = value;
        }
    }

    /// <summary>
    /// Returns the total number of elements that all currently allocated chunks can hold.
    /// </summary>
    public int Capacity => _chunkCount * _elementsPerChunk;

    /// <summary>
    /// Initializes a new ChunkedBuffer with an initial capacity for chunks.
    /// All memory is allocated outside the managed heap.
    /// </summary>
    /// <param name="initialChunkCapacity">The initial size of the chunk pointer array.</param>
    public ChunkedBuffer(int initialChunkCapacity = 4)
    {
        // Internal logic: account for alignment headers when calculating space.
        _headerSize = ALIGNMENT; 
        _elementsPerChunk = (CHUNK_SIZE - _headerSize) / sizeof(T);

        _chunkCapacity = initialChunkCapacity;
        // Allocate the array that holds our chunk pointers (64-byte aligned).
        _chunks = (void**)NativeMemory.AlignedAlloc((nuint)(_chunkCapacity * sizeof(void*)), ALIGNMENT);
        if (_chunks == null) throw new OutOfMemoryException("Failed to allocate chunk pointer array.");

        // Start with at least one chunk available.
        Expand();
    }

    /// <summary>
    /// Reserves space for one new element at the end of the buffer.
    /// </summary>
    /// <returns>A permanent pointer to the newly allocated unmanaged memory block for type T.</returns>
    public void* Add()
    {
        if (_count >= Capacity)
        {
            Expand();
        }

        void* ptr = GetPointer(_count);
        _count++;
        return ptr;
    }

    /// <summary>
    /// Computes the precise memory address for an element based on its index.
    /// Uses O(1) integer math: chunk = index / size, offset = index % size.
    /// </summary>
    /// <param name="index">The logical position in the buffer.</param>
    /// <returns>A direct pointer to the unmanaged data.</returns>
    public void* GetPointer(int index)
    {
        // Boundary Check: Essential for safety in unsafe contexts.
        if (index < 0 || index >= Capacity)
            throw new IndexOutOfRangeException($"Index {index} is out of buffer range {Capacity}.");

        // 1. Identify which physical memory chunk holds this index.
        int chunkIdx = index / _elementsPerChunk;
        // 2. Identify the position within that chunk.
        int offset = index % _elementsPerChunk;

        // 3. Pointer Arithmetic: Base + Header + (Index * Size)
        byte* chunkBase = (byte*)_chunks[chunkIdx];
        return chunkBase + _headerSize + (offset * sizeof(T));
    }

    /// <summary>
    /// Physical expansion of the buffer. 
    /// Allocates a new 16KB chunk and adds it to the list.
    /// </summary>
    private void Expand()
    {
        // Logic: If the pointer array is full, we must reallocate it.
        if (_chunkCount >= _chunkCapacity)
        {
            int newCapacity = _chunkCapacity * 2;
            void** newChunks = (void**)NativeMemory.AlignedAlloc((nuint)(newCapacity * sizeof(void*)), ALIGNMENT);
            if (newChunks == null) throw new OutOfMemoryException();
            
            // Copy the existing chunk pointers to the new, larger array.
            NativeMemory.Copy(_chunks, newChunks, (nuint)(_chunkCount * sizeof(void*)));
            NativeMemory.AlignedFree(_chunks);
            
            _chunks = newChunks;
            _chunkCapacity = newCapacity;
        }

        // Allocate a new aligned block of unmanaged memory for data.
        void* newChunk = NativeMemory.AlignedAlloc(CHUNK_SIZE, ALIGNMENT);
        if (newChunk == null) throw new OutOfMemoryException();
        
        // Zero-initialize the memory block for safety.
        NativeMemory.Clear(newChunk, CHUNK_SIZE);
        
        _chunks[_chunkCount] = newChunk;
        _chunkCount++;
    }

    /// <summary>
    /// Returns the internal chunk pointer table. Used by multi-threaded systems 
    /// for high-speed direct memory access without method-call overhead.
    /// </summary>
    /// <param name="count">Outputs the number of pointers in the returned array.</param>
    /// <returns>An array of raw memory addresses.</returns>
    public void** GetRawChunks(out int count)
    {
        count = _chunkCount;
        return _chunks;
    }

    /// <summary>
    /// Recursively frees all unmanaged chunks and the primary pointer array.
    /// This is the MOST IMPORTANT method to call in the buffer's lifecycle.
    /// </summary>
    public void Dispose()
    {
        if (_chunks == null) return;

        // Logic: Every chunk was AlignedAlloc'd, so every chunk must be AlignedFree'd.
        for (int i = 0; i < _chunkCount; i++)
        {
            NativeMemory.AlignedFree(_chunks[i]);
        }

        // Finally, free the pointer table itself.
        NativeMemory.AlignedFree(_chunks);
        _chunks = null;
        
        // Suppress finalization: we've already manually cleaned up.
        GC.SuppressFinalize(this);
    }

    /// <summary> Finalizer safety net for unmanaged memory leaks. </summary>
    ~ChunkedBuffer() => Dispose();
}
