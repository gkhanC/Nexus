using UnityEngine;
using Nexus.Core;

namespace Nexus.Unity
{
    /// <summary>
    /// NexusEntity: Bridges a Unity GameObject with the Nexus Prime identity system.
    /// Automatically manages a unique EntityId for the object.
    /// </summary>
    [DisallowMultipleComponent]
    public class NexusEntity : MonoBehaviour
    {
        [SerializeField, ReadOnly] private EntityId _id = EntityId.Null;

        /// <summary>
        /// The unique Nexus identity of this object.
        /// </summary>
        public EntityId Id
        {
            get => _id;
            internal set => _id = value;
        }

        private void Awake()
        {
            // If ID wasn't assigned (e.g. not an ECS-driven hybrid),
            // generate a virtual ID based on the instance handle or registry.
            if (_id.IsNull)
            {
                // Note: In a full integration, we'd ask the Registry to create one.
                // For simplified Unity-first usage, we use the hash as a virtual index.
                _id = new EntityId { Index = (uint)gameObject.GetInstanceID(), Version = 0 };
            }
        }
    }
}
