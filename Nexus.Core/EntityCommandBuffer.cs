using System.Collections.Concurrent;
using Nexus.Core;

namespace Nexus.Core
{
    /// <summary>
    /// A thread-safe buffer for queuing entity operations (Create, Destroy, Add/Remove components).
    /// Prevents race conditions by delaying structural changes until a safe "Playback" point.
    /// </summary>
    public class EntityCommandBuffer
    {
        private readonly ConcurrentQueue<Action<Registry.Registry>> _commands = new();

        /// <summary>
        /// Queues the creation of a new entity.
        /// </summary>
        public void CreateEntity()
        {
            _commands.Enqueue(reg => reg.Create());
        }

        /// <summary>
        /// Queues the destruction of an entity.
        /// </summary>
        public void DestroyEntity(EntityId entity)
        {
            _commands.Enqueue(reg => reg.Destroy(entity));
        }

        /// <summary>
        /// Queues adding a component to an entity.
        /// </summary>
        public void AddComponent<T>(EntityId entity, T component = default) where T : unmanaged
        {
            _commands.Enqueue(reg => reg.Add(entity, component));
        }

        /// <summary>
        /// Queues removing a component from an entity.
        /// </summary>
        public void RemoveComponent<T>(EntityId entity) where T : unmanaged
        {
            _commands.Enqueue(reg => reg.Remove<T>(entity));
        }

        /// <summary>
        /// Executes all queued commands on the target registry.
        /// This should be called from the main thread at a safe sync point.
        /// </summary>
        public void Playback(Registry.Registry registry)
        {
            while (_commands.TryDequeue(out var command))
            {
                command(registry);
            }
        }
    }
}
