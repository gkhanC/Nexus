using System.Runtime.InteropServices;

namespace Nexus.Core;

/// <summary>
/// The central hub for entity and component management in the Nexus ECS.
/// Manages entity lifecycle (creation/destruction with versioning) and facilitates
/// high-performance component storage through SparseSet technology.
/// </summary>
public unsafe class Registry : INexusRegistry
{
    /// <summary> Pointer to an array of version numbers for each entity index. Used to validate entity handles. </summary>
    private uint* _versions;
    /// <summary> Current capacity of the versions array. Scaled dynamically as entities grow. </summary>
    private int _versionsCapacity;

    /// <summary> Pointer to an array of entity indices that have been destroyed and are available for reuse. </summary>
    private uint* _freeIndices;
    /// <summary> Number of indices currently available in the free pool. </summary>
    private int _freeCount;
    /// <summary> Maximum capacity of the free indices pool before resizing is required. </summary>
    private int _freeCapacity;

    /// <summary> The next unique ID to be assigned if the free pool is empty (monotonically increasing). </summary>
    private uint _nextId;

    /// <summary> Fast-access array for component sets indexed by their unique type ID. </summary>
    private ISparseSet[] _componentSetsArr = new ISparseSet[128]; 
    /// <summary> Tracked list of all component types for manual iteration/reflection needs. </summary>
    private readonly List<Type> _compTypesList = new();
    /// <summary> Dictionary mapping for slow-path lookups when type IDs are unavailable. </summary>
    private readonly Dictionary<Type, ISparseSet> _componentSetsDict = new(); 

    /// <summary> Cache-line alignment (64 bytes). Prevents CPU cache misses and False Sharing. </summary>
    private const int ALIGNMENT = 64;

    /// <summary>
    /// Initializes a new instance of the Registry with an initial entity capacity.
    /// Uses unmanaged AlignedAlloc to ensure 0 GC impact and optimal cache line usage.
    /// </summary>
    /// <param name="initialCapacity">Initial number of entities to allocate space for.</param>
    public Registry(int initialCapacity = 1024)
    {
        // Allocate raw memory for versioning (Page-Aligned for high scalability).
        _versionsCapacity = initialCapacity;
        _versions = (uint*)NexusMemoryManager.AllocPageAligned(_versionsCapacity * sizeof(uint));
        // Initialize all versions to 0.
        NexusMemoryManager.Clear(_versions, _versionsCapacity * sizeof(uint));

        // Pre-allocate a small pool for entity index reuse (Cache-Line Aligned).
        _freeCapacity = 256;
        _freeIndices = (uint*)NexusMemoryManager.AllocCacheAligned(_freeCapacity * sizeof(uint));
        _freeCount = 0;

        _nextId = 0;
    }

    public event Action<EntityId>? OnEntityCreated;
    public event Action<EntityId>? OnEntityDestroyed;
    public event Action<EntityId, Type>? OnComponentAdded;
    public event Action<EntityId, Type>? OnComponentRemoved;

    /// <summary>
    /// Creates a new unique entity. Reuse indices of destroyed entities when possible
    /// to maintain high data density.
    /// </summary>
    /// <returns>A unique EntityId containing the index and version generation.</returns>
    public EntityId Create()
    {
        uint index;
        // Priority 1: Check the recycle pool (Last-In-First-Out for cache benefits).
        if (_freeCount > 0)
        {
            index = _freeIndices[--_freeCount];
        }
        else
        {
            // Priority 2: Use the next available index.
            index = _nextId++;
            // Expand the version tracking buffer if we've reached memory limits.
            EnsureVersionCapacity(index);
        }

        var id = new EntityId { Index = index, Version = _versions[index] };
        OnEntityCreated?.Invoke(id);
        return id;
    }

    /// <summary>
    /// Destroys an entity and invalidates all existing references to it by incrementing its version.
    /// The index is then added to a free pool for future reuse.
    /// </summary>
    /// <param name="entity">The entity handle to invalidate.</param>
    public void Destroy(EntityId entity)
    {
        // 1. Guard against invalid IDs or double-destruction.
        if (!IsValid(entity)) return;

        OnEntityDestroyed?.Invoke(entity);

        // 2. Increment Version. This is the core magic: any EntityId with the OLD version 
        // will now fail the IsValid check instantly.
        _versions[entity.Index]++; 

        // 3. Add the index to the recycle bin. Resize stack if full.
        if (_freeCount >= _freeCapacity)
        {
            int newCapacity = _freeCapacity * 2;
            uint* newFree = (uint*)NexusMemoryManager.AllocCacheAligned(newCapacity * sizeof(uint));
            NexusMemoryManager.Copy(_freeIndices, newFree, _freeCount * sizeof(uint));
            NexusMemoryManager.Free(_freeIndices);
            _freeIndices = newFree;
            _freeCapacity = newCapacity;
        }
        _freeIndices[_freeCount++] = entity.Index;
    }

    /// <summary>
    /// Checks if an EntityId handle is still valid. 
    /// An ID is valid if its index exists and its version matches the current generation.
    /// </summary>
    /// <param name="entity">The entity handle (Index + Version).</param>
    /// <returns>True if the entity is alive and current.</returns>
    public bool IsValid(EntityId entity)
    {
        // Index must be within bounds AND version must match the current global state for that index.
        return entity.Index < _nextId && _versions[entity.Index] == entity.Version;
    }

    /// <summary>
    /// Adds a component to an entity. If it already has one of type T, the existing data is overwritten.
    /// </summary>
    /// <typeparam name="T">The unmanaged struct component.</typeparam>
    /// <param name="entity">The target entity.</param>
    /// <param name="component">The initial data payload.</param>
    /// <returns>A raw pointer to the component's location in unmanaged memory.</returns>
    public T* Add<T>(EntityId entity, T component = default) where T : unmanaged
    {
        if (!IsValid(entity)) return null;
        // Delegate to the specific SparseSet for this component type.
        var ptr = GetSet<T>().Add(entity, component);
        OnComponentAdded?.Invoke(entity, typeof(T));
        return ptr;
    }

    /// <summary>
    /// Gets a pointer to a component of type T. 
    /// Pointer stability is guaranteed as long as the component is not removed or the Registry disposed.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The target entity handle.</param>
    /// <returns>A pointer to T, or null if the component is missing or entity invalid.</returns>
    public T* Get<T>(EntityId entity) where T : unmanaged
    {
        if (!IsValid(entity)) return null;
        // Fast direct lookup via SparseSet internal logic.
        return GetSet<T>().Get(entity);
    }

    /// <summary>
    /// Determines whether the entity currently possesses a component of type T.
    /// </summary>
    public bool Has<T>(EntityId entity) where T : unmanaged
    {
        if (!IsValid(entity)) return false;
        return GetSet<T>().Has(entity);
    }

    /// <summary>
    /// Removes the component of type T from the entity. 
    /// This will trigger a "swap-and-pop" internally to keep memory dense.
    /// </summary>
    public void Remove<T>(EntityId entity) where T : unmanaged
    {
        if (!IsValid(entity)) return;
        GetSet<T>().Remove(entity);
        OnComponentRemoved?.Invoke(entity, typeof(T));
    }

    /// <summary>
    /// Retrieves (or initializes) the internal SparseSet storage for a specific component type.
    /// This uses a fast-path type ID system for O(1) retrieval.
    /// </summary>
    public SparseSet<T> GetSet<T>() where T : unmanaged
    {
        // 1. Get the globally unique ID for this type.
        int id = ComponentTypeManager.GetId<T>();
        
        // 2. Resize the pool array if this is a newly discovered type.
        if (id >= _componentSetsArr.Length)
        {
            Array.Resize(ref _componentSetsArr, Math.Max(id + 1, _componentSetsArr.Length * 2));
        }

        // 3. If this set doesn't exist yet, create it.
        var set = _componentSetsArr[id];
        if (set == null)
        {
            var newSet = new SparseSet<T>();
            _componentSetsArr[id] = newSet;
            
            var type = typeof(T);
            _componentSetsDict[type] = newSet;
            _compTypesList.Add(type);
            set = newSet;
        }
        return (SparseSet<T>)set;
    }

    /// <summary>
    /// Type-erased version of GetSet for dynamic runtime lookups.
    /// </summary>
    public ISparseSet GetSetByType(Type type)
    {
        if (_componentSetsDict.TryGetValue(type, out var set)) return set;
        
        // If not found, use reflection to trigger typed GetSet<T> (which initializes the set)
        var method = GetType().GetMethod("GetSet").MakeGenericMethod(type);
        return (ISparseSet)method.Invoke(this, null);
    }

    /// <summary>
    /// Resizes the version tracking array when we exceed the current allocation.
    /// </summary>
    private void EnsureVersionCapacity(uint index)
    {
        if (index >= _versionsCapacity)
        {
            // Calculate new capacity (typical doubling strategy).
            int newCapacity = (int)Math.Max(_versionsCapacity * 2, index + 1);
            // Reallocate using page-aligned logic for better MMU stability.
            _versions = (uint*)NexusMemoryManager.Realloc(_versions, _versionsCapacity * sizeof(uint), newCapacity * sizeof(uint), NexusMemoryManager.PAGE_SIZE);
            _versionsCapacity = newCapacity;
        }
    }

    /// <summary> Provides an enumerable view of all active component storage sets. </summary>
    public IEnumerable<ISparseSet> ComponentSets => _componentSetsDict.Values;
    /// <summary> Provides a list of all component types currently managed by this registry. </summary>
    public IEnumerable<Type> ComponentTypes => _compTypesList;

    /// <summary>
    /// Performs a full cleanup of the registry, freeing all unmanaged buffers.
    /// Must be called manually or via using-block to prevent severe memory leaks.
    /// </summary>
    public void Dispose()
    {
        // Free base entity tracking buffers using standard manager path.
        if (_versions != null) { NexusMemoryManager.Free(_versions); _versions = null; }
        if (_freeIndices != null) { NexusMemoryManager.Free(_freeIndices); _freeIndices = null; }

        // Dispose all component storage (each SparseSet has its own unmanaged buffers).
        foreach (var set in _componentSetsDict.Values)
        {
            set.Dispose();
        }
        _componentSetsDict.Clear();
        
        // Flag for GC that we've cleaned up manually.
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Resets all dirty bits across all component storage sets. 
    /// Should be called at the end of every simulation frame to prepare for the next delta-capture.
    /// </summary>
    public void ClearAllDirtyBits()
    {
        foreach (var set in _componentSetsDict.Values)
        {
            set.ClearAllDirty();
        }
    }

    /// <summary>
    /// Starts a chainable, fluid query for entities and components.
    /// </summary>
    public NexusQueryBuilder Query() => new NexusQueryBuilder(this);

    /// <summary> Safety finalizer to ensure memory is freed even if Dispose() is forgotten. </summary>
    ~Registry() => Dispose();
}
