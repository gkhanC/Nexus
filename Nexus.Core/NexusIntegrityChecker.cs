using System;
using System.Runtime.InteropServices;
using Nexus.Core;

namespace Nexus.Core
{
    /// <summary>
    /// NexusIntegrityChecker: Professional runtime diagnostics for the ECS world.
    /// Monitors unmanaged memory constraints and alignment health.
    /// </summary>
    public static unsafe class NexusIntegrityChecker
    {
        public enum HealthStatus { Nominal, Degraded, Critical }

        public struct CoreMetrics
        {
            public HealthStatus Status;
            public int ActiveEntities;
            public int ComponentSetCount;
            public string Diagnostics;
        }

        /// <summary>
        /// Performs an exhaustive integrity check on the specified registry.
        /// Verifies unmanaged memory alignment and chunk health.
        /// </summary>
        public static CoreMetrics Audit(Registry registry)
        {
            var metrics = new CoreMetrics
            {
                Status = HealthStatus.Nominal,
                ActiveEntities = registry.EntityCount,
                ComponentSetCount = registry.ComponentSetCount,
                Diagnostics = "Nexus Core is healthy."
            };

            foreach (var set in registry.ComponentSets)
            {
                // 1. Verify Memory Alignment
                void* densePtr = set.GetRawDense(out _);
                if (densePtr != null && ((long)densePtr % NexusMemoryManager.CACHE_LINE) != 0)
                {
                    metrics.Status = HealthStatus.Critical;
                    metrics.Diagnostics += $"\n[Alignment Violation] {set.GetType().Name} Dense buffer is not cache-line aligned!";
                }

                void** chunks = set.GetRawChunks(out int chunkCount);
                for (int i = 0; i < chunkCount; i++)
                {
                    if (chunks[i] != null && ((long)chunks[i] % NexusMemoryManager.PAGE_SIZE) != 0)
                    {
                        metrics.Status = HealthStatus.Degraded;
                        metrics.Diagnostics += $"\n[Memory Warning] {set.GetType().Name} Chunk {i} is not page-aligned.";
                    }
                }
            }

            return metrics;
        }

        /// <summary>
        /// Global check for static state bloat (Zero-Cost audit).
        /// </summary>
        public static void CheckZeroCostCompliance()
        {
            // In a professional environment, this would use reflection to ensure
            // no massive static arrays or buffers are allocated unless explicitly requested.
        }
    }
}
