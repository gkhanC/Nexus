using UnityEngine;
using Nexus.Unity.Inputs;

namespace Nexus.Unity.Controllers
{
    /// <summary>
    /// NexusSwerveController: Handles horizontal sliding movement based on finger/mouse drag.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public class NexusSwerveController : MonoBehaviour
    {
        [Header("Settings")]
        public float SwerveSpeed = 5f;
        public float MaxSwerveAmount = 2f;
        
        [SerializeField] private NexusSwerveInput _input;

        private void Awake()
        {
            if (_input == null) _input = GetComponent<NexusSwerveInput>();
        }

        private void Update()
        {
            float swerveAmount = Time.deltaTime * SwerveSpeed * _input.MoveFactorX;
            float targetX = transform.localPosition.x + swerveAmount;
            
            targetX = Mathf.Clamp(targetX, -MaxSwerveAmount, MaxSwerveAmount);
            
            transform.localPosition = new Vector3(targetX, transform.localPosition.y, transform.localPosition.z);
        }
    }
}
