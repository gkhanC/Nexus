using UnityEngine;

namespace Nexus.Unity.Controllers
{
    /// <summary>
    /// NexusRotateController: Handles entity rotation with smooth interpolation.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public class NexusRotateController : MonoBehaviour
    {
        public float RotateSpeed = 10f;

        public void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return;
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * RotateSpeed);
        }
    }
}
