using System;
using UnityEngine;

namespace Nexus.Mathematics
{
    /// <summary>
    /// NexusRotationField: An unmanaged structure representing rotation as Euler angles.
    /// Seamlessly converts to Quaternion and supports scalar operations.
    /// </summary>
    [Serializable]
    public struct NexusRotationField
    {
        public Vector3 Euler;

        public Quaternion Quaternion => Quaternion.Euler(Euler);

        public NexusRotationField(Vector3 euler)
        {
            Euler = euler;
        }

        public NexusRotationField(Quaternion quaternion)
        {
            Euler = quaternion.eulerAngles;
        }

        public static implicit operator Quaternion(NexusRotationField field) => field.Quaternion;
        public static implicit operator Vector3(NexusRotationField field) => field.Euler;
        public static implicit operator NexusRotationField(Vector3 euler) => new NexusRotationField(euler);
        public static implicit operator NexusRotationField(Quaternion quaternion) => new NexusRotationField(quaternion);

        public static NexusRotationField operator *(NexusRotationField a, float b) => new NexusRotationField(a.Euler * b);
    }
}
