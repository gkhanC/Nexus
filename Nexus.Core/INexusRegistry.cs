using System;
using System.Collections.Generic;

namespace Nexus.Core
{
    /// <summary>
    /// Contract for the high-performance central entity and component manager.
    /// Defines the lifecycle and storage interface for the Nexus ECS core.
    /// </summary>
    public unsafe interface INexusRegistry : IDisposable
    {
        /// <summary> Allocates a new entity index or reuses a recycled one. </summary>
        EntityId Create();

        /// <summary> Invalidates an entity handle and clears its component data. </summary>
        void Destroy(EntityId entity);

        /// <summary> Validates if an entity handle version matches the registry's current generation. </summary>
        bool IsValid(EntityId entity);
        
        /// <summary> Attaches a component to an entity. Overwrites if already present. </summary>
        T* Add<T>(EntityId entity, T component = default) where T : unmanaged;

        /// <summary> Fetches a raw pointer to an entity's component. Returns null if missing. </summary>
        T* Get<T>(EntityId entity) where T : unmanaged;

        /// <summary> Returns true if the entity possesses the component type T. </summary>
        bool Has<T>(EntityId entity) where T : unmanaged;

        /// <summary> Removes component T from the entity. O(1) swap-and-pop removal. </summary>
        void Remove<T>(EntityId entity) where T : unmanaged;
        
        /// <summary> Direct access to the internal SparseSet storage for type T. </summary>
        SparseSet<T> GetSet<T>() where T : unmanaged;
        
        /// <summary> Enumerable view of all typed storage containers. </summary>
        IEnumerable<ISparseSet> ComponentSets { get; }

        /// <summary> List of all registered component types. </summary>
        IEnumerable<Type> ComponentTypes { get; }
    }
}
