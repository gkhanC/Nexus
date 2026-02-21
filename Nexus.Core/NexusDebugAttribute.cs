using System;

namespace Nexus.Attributes
{
    public enum DebugShape { Point, Arrow, Line, Label }

    /// <summary>
    /// Mark a field or component with this attribute to enable real-time 
    /// visual debugging in the Unity Editor Scene view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class NexusDebugAttribute : Attribute
    {
        public DebugShape Shape { get; }
        public string ColorHex { get; }
        public float Size { get; }

        public NexusDebugAttribute(DebugShape shape = DebugShape.Label, string colorHex = "#FFFFFF", float size = 1.0f)
        {
            Shape = shape;
            ColorHex = colorHex;
            Size = size;
        }
    }
}
