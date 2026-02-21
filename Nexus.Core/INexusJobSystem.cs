using System.Collections.Generic;

namespace Nexus.Core
{
    /// <summary>
    /// Contract for the high-level system coordinator and parallel execution engine.
    /// Handles system registration, dependency resolution, and performance monitoring.
    /// </summary>
    public interface INexusJobSystem
    {
        /// <summary>
        /// Registers a logical system. The dispatcher will automatically thread-safe this system 
        /// based on its [Read] and [Write] properties.
        /// </summary>
        /// <param name="system">The system implementation.</param>
        void AddSystem(INexusSystem system);

        /// <summary>
        /// Triggers the execution of all registered systems.
        /// Typically called once per frame in the main game loop.
        /// </summary>
        void Execute();

        /// <summary>
        /// Returns the performance results for the most recent execution pass.
        /// </summary>
        List<JobSystem.ExecutionMetrics> GetLastExecutionMetrics();
    }
}
