using System;
using System.Collections.Concurrent;
using Nexus.Core;

namespace Nexus.Core
{
    /// <summary>
    /// Nexus-Object-Mapping: Provides a zero-cost reference system to link 
    /// unmanaged entity data with managed Unity objects (e.g., Transform).
    /// </summary>
    public static class NexusObjectMapping
    {
        private static readonly ConcurrentDictionary<uint, object> _mappings = new();

        /// <summary>
        /// Links an entity index to a managed Unity object.
        /// </summary>
        public static void Map(uint entityIndex, object unityObject)
        {
            _mappings[entityIndex] = unityObject;
        }

        public static T Get<T>(uint entityIndex) where T : class
        {
            if (_mappings.TryGetValue(entityIndex, out var obj))
            {
                return obj as T;
            }
            return null;
        }

        /// <summary>
        /// Attempts to retrieve the mapped object. Zero-cost path for high-frequency sync.
        /// </summary>
        public static bool TryGet(uint entityIndex, out object obj)
        {
            return _mappings.TryGetValue(entityIndex, out obj);
        }

        /// <summary>
        /// Removes a mapping.
        /// </summary>
        public static void Unmap(uint entityIndex)
        {
            _mappings.TryRemove(entityIndex, out _);
        }
    }
}
