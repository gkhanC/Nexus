using UnityEngine;
using Nexus.Core;
using Nexus.Attributes;
using Nexus.Mathematics;

namespace Nexus.Unity
{
    /// <summary>
    /// Professional SyncManager: Orchestrates the bidirectional flow of data
    /// between the Nexus Unmanaged Engine and Unity GameObjects.
    /// </summary>
    public static class NexusSyncManager
    {
        /// <summary>
        /// Global synchronization pass.
        /// Iterates only through entities possessing [Sync] components.
        /// </summary>
        public static void Sync(Registry registry)
        {
            // PERFORMANCE: We identify systems/components marked with [Sync]
            // For this POC, we synchronize the most common: Position and Rotation.
            
            var positionSet = registry.GetSet<Vector3>(); // Assuming Vector3 is used as a component
            var rotationSet = registry.GetSet<NexusRotationField>();

            // Iterating the smaller set or a joined set
            for (int i = 0; i < positionSet.Count; i++)
            {
                EntityId id = positionSet.GetEntity(i);
                
                // Fetch the mapped Unity object (Zero-cost mapping)
                if (NexusObjectMapping.TryGet(id.Index, out object obj) && obj is GameObject go)
                {
                    Transform t = go.transform;
                    
                    // 1. Sync Position
                    Vector3* pPtr = positionSet.Get(id);
                    if (pPtr != null) t.position = *pPtr;

                    // 2. Sync Rotation (if exists)
                    NexusRotationField* rPtr = rotationSet.Get(id);
                    if (rPtr != null) t.rotation = rPtr->ToQuaternion();
                }
            }
        }

        /// <summary>
        /// Alternative: Localized sync for specific entities.
        /// </summary>
        public static void SyncEntity(Registry registry, EntityId id, GameObject target)
        {
            if (registry.Has<Vector3>(id))
            {
                target.transform.position = *registry.Get<Vector3>(id);
            }
        }
    }
}
