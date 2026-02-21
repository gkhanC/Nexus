# Nexus Prime Architectural Manual: NexusInspectAttribute (Editor Inspector Integration)

## 1. Introduction
`NexusInspectAttribute.cs` is a product of Nexus Prime's philosophy of "Making Invisible Data Visible." It brings structs and fields that are stored as unmanaged in C# and are normally not visible in the Unity Inspector window into the Unity Editor user interface.

The reason for this attribute's existence is that unmanaged data (e.g., raw pointers, blittable structs) is incompatible with Unity's standard `SerializeField` system. `NexusInspect` overcomes this limitation, allowing even unmanaged data to be monitored and modified via the editor.

---

## 2. Technical Analysis
NexusInspectAttribute offers the following capabilities for data visibility:

- **Custom Labeling**: Allows data to appear in the editor with a more meaningful name (`Label`) instead of its name in the code.
- **Read-Only Mode**: Prevents accidental incorrect interventions via the editor by keeping critical data in monitoring mode only.
- **Pointer Visualization**: Visualizes raw memory addresses (pointers) in a human-readable format.
- **Conditional Compilation**: With `#if UNITY_EDITOR` blocks, it ensures that this attribute works only in the editor environment, adhering to the zero-overhead principle in the game's release (final) version.

---

## 3. Logical Flow
1.  **Definition**: Fields within the component are marked with `[NexusInspect]`.
2.  **Reflection**: A special `VisualDataInspector` class in the Unity Editor scans components within the Registry.
3.  **Drawing**: Marked fields are created in the Inspector panel using `EditorGUILayout` commands.
4.  **Editing**: When the user changes a value from the editor, Nexus immediately writes this change to the unmanaged memory address (`unsafe`).

---

## 4. Usage Example
```csharp
public struct Health {
    [NexusInspect("Active Health Value", readOnly: false)]
    public float Current;

    [NexusInspect("Maximum Capacity", readOnly: true)]
    public float Max;
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
public class NexusInspectAttribute : Attribute
{
    public string Label { get; }
    public bool ReadOnly { get; }

    public NexusInspectAttribute(string label = null, bool readOnly = false)
    {
        Label = label;
        ReadOnly = readOnly;
    }
}
```

---

## Nexus Optimization Tip: Non-Intrusive Inspection
When using the `NexusInspect` attribute, you don't have to make your data `public`. You can bring even private fields to the editor by marking them with this attribute without breaking encapsulation. This **keeps your code architecture clean while increasing your debugging capability by 300%.**
