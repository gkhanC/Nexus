# Nexus Prime Architectural Manual: NexusDebugAttribute (Visual Debugging)

## 1. Introduction
`NexusDebugAttribute.cs` is the "Visual Diagnostics" tool that connects Nexus Prime's unmanaged data world to the visual world of the Unity Editor. It allows data that exists only as numbers and bytes within the code (e.g., Position, Velocity, Target) to appear as shapes or labels on the Unity Scene view.

The reason for this attribute's existence is that unmanaged structs cannot directly access the Unity Gizmos system. Data marked with `NexusDebug` is automatically captured by the Nexus Editor Suite and drawn on the screen.

---

## 2. Technical Analysis
NexusDebugAttribute offers the following parameters for debugging processes:

- **DebugShape Enum**: Provides the option to visualize data as a `Point`, `Arrow`, `Line`, or `Label`.
- **Color Selection**: Allows determining the color of visualization with hexadecimal (Hex) color codes.
- **Scale Control**: Dynamically adjusts the size (`Size`) of the drawn shapes.
- **Reflective Discovery**: Editor tools automatically add fields carrying this attribute to the "Draw List" while scanning systems.

---

## 3. Logical Flow
1.  **Marking**: The developer marks a component they want to debug with `[NexusDebug]`.
2.  **Capturing**: While the Unity Editor is running, Nexus's Gizmo manager finds active entities carrying this attribute.
3.  **Conversion**: Unmanaged data (e.g., `Vector3`) is converted into the Unity Gizmos command suitable for the selected shape (Shape).
4.  **Drawing**: Visualized in real-time on the Scene view.

---

## 4. Usage Example
```csharp
[NexusDebug(DebugShape.Arrow, "#FF0000", 2.0f)]
public struct Velocity {
    public float3 Value;
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Attributes;

public enum DebugShape { Point, Arrow, Line, Label }

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
```

---

## Nexus Optimization Tip: Targeted Debugging
Instead of debugging all entities, filter `NexusDebug` parameters to visualize only selected entities or entities within a certain radius. This **prevents Scene view FPS drop, allowing you to work comfortably among thousands of entities.**
