using UnityEngine;

namespace Nexus.Unity.Controllers
{
    /// <summary>
    /// NexusRotateController: Optimized object rotation logic.
    /// </summary>
    public class NexusRotateController : MonoBehaviour
    {
        public Vector3 RotationSpeed;
        void Update() => transform.Rotate(RotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// NexusRigidbodyMove: Physics-based, optimized movement motor.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class NexusRigidbodyMove : MonoBehaviour
    {
        private Rigidbody _rb;
        public float Speed = 10f;

        void Awake() => _rb = GetComponent<Rigidbody>();

        public void Move(Vector3 direction)
        {
            _rb.MovePosition(transform.position + direction * Speed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// NexusTrajectorySimulator: Pre-calculates physical trajectories.
    /// </summary>
    public class NexusTrajectorySimulator
    {
        public static Vector3 GetPointAtTime(Vector3 start, Vector3 velocity, float time)
        {
            return start + velocity * time + 0.5f * Physics.gravity * time * time;
        }
    }

    /// <summary>
    /// NexusHueShifter: Visual effect for color manipulation.
    /// </summary>
    public class NexusHueShifter : MonoBehaviour
    {
        public float Speed = 1f;
        private Renderer _renderer;

        void Awake() => _renderer = GetComponent<Renderer>();
        void Update()
        {
            float h, s, v;
            Color.RGBToHSV(_renderer.material.color, out h, out s, out v);
            h = (h + Time.deltaTime * Speed) % 1.0f;
            _renderer.material.color = Color.HSVToRGB(h, s, v);
        }
    }
}

namespace Nexus.Unity.Inputs
{
    /// <summary>
    /// NexusSwerveInput: Mobile-friendly swerve input system.
    /// </summary>
    public class NexusSwerveInput : MonoBehaviour
    {
        private float _lastFingerPosX;
        private float _moveFactorX;
        public float MoveFactorX => _moveFactorX;

        void Update()
        {
            if (Input.GetMouseButtonDown(0)) _lastFingerPosX = Input.mousePosition.x;
            else if (Input.GetMouseButton(0))
            {
                _moveFactorX = Input.mousePosition.x - _lastFingerPosX;
                _lastFingerPosX = Input.mousePosition.x;
            }
            else _moveFactorX = 0f;
        }
    }
}
