using System;

namespace Nexus.Core
{
    /// <summary>
    /// The base interface for all logic-driven systems in the Nexus Framework.
    /// Systems are designed to be state-less logic containers that operate on component data.
    /// </summary>
    public interface INexusSystem
    {
        /// <summary>
        /// Where the actual logic resides. 
        /// This method is called by the JobSystem, potentially on a worker thread.
        /// </summary>
        void Execute();
    }

    /// <summary>
    /// Metadata attribute to signal that a system requires Read-Only access to a component type T.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadAttribute : Attribute { }

    /// <summary>
    /// Metadata attribute to signal that a system requires Write (exclusive) access to a component type T.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class WriteAttribute : Attribute { }

    /// <summary>
    /// Metadata attribute used for Automatic System Injection.
    /// Fields marked with [Inject] will be automatically populated by the JobSystem.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute { }
}
