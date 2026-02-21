using Nexus.Attributes;
using Nexus.Communication;
using Nexus.Logging;
using Nexus.Mathematics;
using Nexus.Core;
using Nexus.Collections;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Nexus.Unity
{
    /// <summary>
    /// The "Master Facade" of the Nexus Unity Framework.
    /// NexusHelper provides a unified, readable API that wraps high-performance internal modules
    /// (Pooling, Events, Logging, etc.) into a single, intuitive static-entry point.
    /// </summary>
    public static class NexusHelper
    {
        // --- LOGGING FACADE ---

        /// <summary>
        /// Logs a standard message to the Unity console. 
        /// Uses the thread-safe NexusLogger to safely capture logs coming from the C# Job System worker threads.
        /// </summary>
        /// <param name="context">The component/object sending the log (for Unity Console highlighting).</param>
        /// <param name="message">The content to log.</param>
        public static void Log(object context, object message) => Logging.NexusLogger.Instance.Log(context, message);

        /// <summary> Logs an error message to the Unity console. Thread-safe. </summary>
        public static void LogError(object context, object message) => Logging.NexusLogger.Instance.LogError(context, message);

        /// <summary> Logs a warning message to the Unity console. Thread-safe. </summary>
        public static void LogWarning(object context, object message) => Logging.NexusLogger.Instance.LogWarning(context, message);

        /// <summary> Logs a success message formatted with premium Nexus green styling. </summary>
        public static void LogSuccess(object context, object message) => Logging.NexusLogger.Instance.LogSuccess(context, message);

        // --- POOLING FACADE ---

        /// <summary>
        /// Spawns a GameObject from the pool instead of using the expensive Instantiate().
        /// This completely eliminates runtime GC spikes during object creation.
        /// </summary>
        /// <param name="prefab">The source prefab template.</param>
        /// <param name="position">World position to place the object.</param>
        /// <param name="rotation">World rotation to apply.</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <returns>An active GameObject instance.</returns>
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) 
            => NexusObjectPool.Instance.Spawn(prefab, position, rotation, parent);

        /// <summary>
        /// Returns an active GameObject to the pool. 
        /// Use this instead of Destroy() to recycle memory.
        /// </summary>
        /// <param name="instance">The object to return.</param>
        public static void Despawn(GameObject instance) => NexusObjectPool.Instance.Despawn(instance);

        // --- COMMUNICATION FACADE ---

        /// <summary>
        /// Broadcasts an event to any object subscribed to the global NexusEventBus.
        /// </summary>
        /// <typeparam name="T">The event struct/class (must implement INexusEvent).</typeparam>
        /// <param name="eventData">The payload to send.</param>
        public static void Publish<T>(T eventData) where T : INexusEvent => NexusEventBus.Instance.Publish(eventData);

        /// <summary>
        /// Connects an action callback to a specific event type.
        /// </summary>
        public static void Subscribe<T>(Action<T> onEvent) where T : INexusEvent => NexusEventBus.Instance.Subscribe(onEvent);

        /// <summary>
        /// Disconnects a callback from the event bus to prevent memory leaks.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> onEvent) where T : INexusEvent => NexusEventBus.Instance.Unsubscribe(onEvent);

        // --- HIERARCHY EXTENSIONS ---

        /// <summary>
        /// Optimized "Safe Get": returns an existing component or adds it if missing.
        /// Prevents common 'NullReferenceException' and reduces boilerplate code.
        /// </summary>
        public static T GetOrAdd<T>(this GameObject go) where T : Component => NexusHierarchyExtensions.GetOrAdd<T>(go);

        // --- CONTROLLER FACADE (Component-Driven) ---

        /// <summary>
        /// Triggers movement logic on a GameObject if it possesses a NexusRigidbodyMove component.
        /// Allows logic-friendly 'Fire and Forget' movement calls.
        /// </summary>
        public static void Move(GameObject go, Vector3 direction)
        {
            if (go.TryGetComponent<NexusRigidbodyMove>(out var mover)) mover.Move(direction);
        }

        /// <summary>
        /// Triggers rotation logic on a GameObject if it possesses a NexusRotateController.
        /// </summary>
        public static void RotateTowards(GameObject go, Vector3 direction)
        {
            if (go.TryGetComponent<NexusRotateController>(out var rotator)) rotator.RotateTowards(direction);
        }

        // --- DATA BINDING ---

        /// <summary>
        /// One-liner for binding a Nexus logical variable (like Health/Mana) to a Unity UI Image.
        /// Automatically handles FillAmount updates when the variable changes.
        /// </summary>
        public static void BindUI(this NexusAttributeVariable variable, Image targetImage) 
            => UI.NexusUIBindings.Bind(variable, targetImage);
    }
}
