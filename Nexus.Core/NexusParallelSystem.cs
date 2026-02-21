using System;
using System.Reflection;
using System.Collections.Generic;

namespace Nexus.Core
{
    /// <summary>
    /// Base class for autonomous, parallel-ready systems.
    /// Provides automatic dependency resolution and lifecycle management.
    /// </summary>
    public abstract class NexusParallelSystem : INexusSystem
    {
        /// <summary> The localized registry for this system. </summary>
        [Inject] protected Registry Registry;

        /// <summary>
        /// Human-readable name of the system.
        /// </summary>
        public string Name => GetType().Name;

        /// <summary>
        /// Returns the data access requirements for this system.
        /// Derived classes can override this or use attributes for reflection-based identification.
        /// </summary>
        public virtual (HashSet<Type> Reads, HashSet<Type> Writes) GetAccessInfo()
        {
            var reads = new HashSet<Type>();
            var writes = new HashSet<Type>();

            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<ReadAttribute>() != null) reads.Add(field.FieldType);
                if (field.GetCustomAttribute<WriteAttribute>() != null) writes.Add(field.FieldType);
            }

            return (reads, writes);
        }

        /// <summary>
        /// Entry point for the system's logic.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Optional: Called when the system is added to the job orchestrator.
        /// </summary>
        public virtual void OnCreate() { }

        /// <summary>
        /// Optional: Called when the system or orchestrator is disposed.
        /// </summary>
        public virtual void OnDestroy() { }
    }
}
