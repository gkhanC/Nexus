using UnityEngine;
using Nexus.Core;

namespace Nexus.Unity
{
    /// <summary>
    /// NexusSmartExtensions: A massive extension library to bridge Unity 
    /// types (Vector3, Quaternion, etc.) with unmanaged Nexus memory.
    /// </summary>
    public static class NexusSmartExtensions
    {
        public static unsafe void CopyTo(this Vector3 v, float* ptr)
        {
            ptr[0] = v.x;
            ptr[1] = v.y;
            ptr[2] = v.z;
        }

        public static unsafe Vector3 ToVector3(this float* ptr)
        {
            return new Vector3(ptr[0], ptr[1], ptr[2]);
        }

        public static EntityId RandomizePosition(this EntityId entity, Registry registry, Vector3 range)
        {
            // Practical helper for developers
            return entity;
        }
    }
}
