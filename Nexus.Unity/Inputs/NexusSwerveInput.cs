using UnityEngine;

namespace Nexus.Unity.Inputs
{
    /// <summary>
    /// NexusSwerveInput: Captures swerve/slide input for mobile and desktop.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public class NexusSwerveInput : MonoBehaviour
    {
        [Header("Settings")]
        public float Sensitivity = 1f;

        private float _lastMouseX;
        private float _moveFactorX;
        private Camera _cam;

        public float MoveFactorX => _moveFactorX;

        private void Start()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _lastMouseX = Input.mousePosition.x;
            }
            else if (Input.GetMouseButton(0))
            {
                _moveFactorX = (Input.mousePosition.x - _lastMouseX) * Sensitivity;
                _lastMouseX = Input.mousePosition.x;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _moveFactorX = 0f;
            }
        }

        /// <summary>
        /// Gets the world-space horizontal position of the input.
        /// Direct parity with HypeFire's SwerveInputReader.
        /// </summary>
        public float GetHorizontalWorldPosition()
        {
            if (_cam == null) return 0f;
            Vector3 inputPos = Input.mousePosition;
            inputPos.z = Mathf.Abs(_cam.transform.position.z);
            return _cam.ScreenToWorldPoint(inputPos).x;
        }
    }
}
