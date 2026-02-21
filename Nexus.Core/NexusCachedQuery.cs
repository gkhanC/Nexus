using System;
using System.Collections.Generic;

namespace Nexus.Core
{
    /// <summary>
    /// NexusCachedQuery: A reactive query that caches its results.
    /// Updates automatically when entities or components are modified.
    /// </summary>
    public class NexusCachedQuery : IDisposable
    {
        private readonly Registry _registry;
        private readonly Type[] _required;
        private readonly HashSet<EntityId> _cache = new();
        private bool _isDirty = true;

        public NexusCachedQuery(Registry registry, params Type[] required)
        {
            _registry = registry;
            _required = required;

            _registry.OnEntityDestroyed += OnEntityModified;
            _registry.OnComponentAdded += OnComponentModified;
            _registry.OnComponentRemoved += OnComponentModified;
        }

        private void OnEntityModified(EntityId entity) => _cache.Remove(entity);

        private void OnComponentModified(EntityId entity, Type type)
        {
            // If any of the modified components are in our requirement list, we mark the cache as dirty.
            foreach (var req in _required)
            {
                if (req == type)
                {
                    _isDirty = true;
                    break;
                }
            }
        }

        public IEnumerable<EntityId> GetEntities()
        {
            if (_isDirty)
            {
                RebuildCache();
            }
            return _cache;
        }

        private void RebuildCache()
        {
            _cache.Clear();
            // Perform a full scan (could be optimized with bitsets)
            _registry.Query()
                .WithAll(_required)
                .Execute(e => _cache.Add(e));
            _isDirty = false;
        }

        public void Dispose()
        {
            _registry.OnEntityDestroyed -= OnEntityModified;
            _registry.OnComponentAdded -= OnComponentModified;
            _registry.OnComponentRemoved -= OnComponentModified;
        }
    }

    public static class QueryBuilderExtensions
    {
        public static NexusQueryBuilder WithAll(this NexusQueryBuilder builder, Type[] types)
        {
            // Helper to add multiple types to the builder.
            // (Implementation in NexusQueryBuilder.cs would be needed)
            return builder;
        }
    }
}
