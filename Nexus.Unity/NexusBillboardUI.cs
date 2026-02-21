using UnityEngine;

namespace Nexus.Unity
{
    /// <summary>
    /// NexusBillboardUI: Ensures the object always faces the main camera.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public class NexusBillboardUI : MonoBehaviour
    {
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_mainCamera != null)
            {
                transform.rotation = _mainCamera.transform.rotation;
            }
        }
    }
}
