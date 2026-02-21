using UnityEngine;
using Nexus.Core;
using Nexus.Unity;

namespace Nexus.Unity
{
    /// <summary>
    /// NexusInitializer: Simplified "One-Click" engine setup for Unity.
    /// Manages the lifecycle of core ECS systems in the scene.
    /// </summary>
    [AddComponentMenu("Nexus/Engine Initializer")]
    public class NexusInitializer : MonoBehaviour
    {
        [Header("Engine Configuration")]
        public int MaxHistoryFrames = 100;
        public bool EnableParallelExecution = true;
        public bool PerformRuntimeIntegrityChecks = true;

        private Registry _registry;
        private JobSystem _jobSystem;
        private SnapshotManager _snapshotManager;

        public Registry Registry => _registry;
        public JobSystem JobSystem => _jobSystem;

        private void Awake()
        {
            // 1. Initialize Core
            _registry = new Registry();
            _jobSystem = new JobSystem(_registry);
            _snapshotManager = new SnapshotManager { MaxHistoryFrames = MaxHistoryFrames };

            Debug.Log("[Nexus.Engine] ECS World Initialized successfully.");
        }

        private void Update()
        {
            if (EnableParallelExecution)
            {
                _jobSystem.Execute();
            }

            if (PerformRuntimeIntegrityChecks && Time.frameCount % 60 == 0)
            {
                var audit = NexusIntegrityChecker.Audit(_registry);
                if (audit.Status != NexusIntegrityChecker.HealthStatus.Nominal)
                {
                    Debug.LogWarning($"[Nexus.Integrity] {audit.Diagnostics}");
                }
            }
        }

        private void OnDestroy()
        {
            _registry?.Dispose();
            // _snapshotManager?.DisposeAll(); // Implement if needed
        }
    }
}
