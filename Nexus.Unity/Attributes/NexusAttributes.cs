using System;
using UnityEngine;

namespace Nexus.Attributes
{
    /// <summary>
    /// [ReadOnly]: Makes a field visible in the Inspector but not editable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute { }

    /// <summary>
    /// [Required]: Marks a field as required. Shows a warning in the Inspector if null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredAttribute : PropertyAttribute { }

    /// <summary>
    /// [NexusProgressBar]: Displays a progress bar for a float/int field in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NexusProgressBarAttribute : PropertyAttribute
    {
        public string Color { get; }
        public float Max { get; }

        public NexusProgressBarAttribute(float max = 100f, string color = "Blue")
        {
            Max = max;
            Color = color;
        }
    }

    /// <summary>
    /// [HelpBox]: Displays an information box in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class HelpBoxAttribute : PropertyAttribute
    {
        public string Message { get; }
        public HelpBoxAttribute(string message) => Message = message;
    }

    /// <summary>
    /// [Button]: Renders a button in the Inspector that executes the marked method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public string Label { get; }
        public ButtonAttribute(string label = null) => Label = label;
    }

    /// <summary>
    /// [MinMaxSlider]: Renders a min-max slider in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MinMaxAttribute : PropertyAttribute
    {
        public float Min { get; }
        public float Max { get; }
        public MinMaxAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    public enum SyncTarget { Transform, Physics, UI, Logic }

    /// <summary>
    /// [Sync]: Automates bidirectional synchronization between Nexus unmanaged data 
    /// and Unity engine components (e.g., Transform, Physics).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class SyncAttribute : Attribute
    {
        public SyncTarget Target { get; }
        public SyncAttribute(SyncTarget target = SyncTarget.Transform) => Target = target;
    }

    /// <summary>
    /// [AutoView]: Real-time Scene-view data visualization for unmanaged ECS components.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class AutoViewAttribute : Attribute { }

    /// <summary>
    /// [Persistent]: Marks an ECS component to survive scene loads and transfers.
    /// Integrated with the Nexus save-system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class PersistentAttribute : Attribute { }

    /// <summary>
    /// [BitPacked]: Signals that the component fields should be compressed into bits.
    /// Requires bit-level serializer support.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class BitPackedAttribute : Attribute { }
}
