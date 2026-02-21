using UnityEngine;

namespace Nexus.Unity.Controllers
{
    /// <summary>
    /// NexusRigidbodyMove: Physics-based movement controller.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class NexusRigidbodyMove : MonoBehaviour
    {
        public float Speed = 5f;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Move(Vector3 direction)
        {
            _rb.MovePosition(transform.position + direction * Speed * Time.fixedDeltaTime);
        }

        public void SetVelocity(Vector3 velocity)
        {
            _rb.linearVelocity = velocity;
        }
    }
}
