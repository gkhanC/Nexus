# Nexus Prime Architectural Manual: NexusAttributes (Engineering Attributes)

## 1. Introduction
`NexusAttributes.cs` is an "Attribute Package" that both organizes Unity Inspector visualization and gives hints about data management to the Nexus ECS system. It is the metadata layer determining how the code will behave on both the editor side and the unmanaged simulation side.

The reason for these attributes' existence is to control data security (`ReadOnly`), visual hierarchy (`ProgressBar`), and system behaviors (`Sync`, `Persistent`) without writing extra code, using the "Decoration" (Decoration) logic.

---

## 2. Technical Analysis
The package includes two main types of attribute groups:

### A. Visual & Editor Attributes
- **[ReadOnly]**: Makes the field appear in the Inspector but prevents manual changes.
- **[Required]**: Marks critical references. Throws a warning in the Inspector if the field is empty.
- **[NexusProgressBar]**: Draws numerical values (Health, Energy, etc.) as a visual bar.
- **[MinMaxSlider]**: Offers a two-ended (Min/Max) slider within the determined range.

### B. Architectural & ECS Attributes
- **[Sync]**: Specifies that an unmanaged piece of data will be automatically synchronized with the Unity `Transform` or `Physics` world.
- **[Persistent]**: Ensures the data is not discarded from memory during scene transitions and is included in the recording (Save) system.
- **[BitPacked]**: A flag (for the Source Generator) informing that the data should be compressed at the bit level during transmission.

---

## 3. Logical Flow
1.  **Definition**: The developer marks the component field with `[Sync]` or `[ReadOnly]`.
2.  **Editor Analysis**: Unity's `PropertyDrawer` mechanism sees this flag and performs custom drawing.
3.  **Simulation Integration**: Nexus Source Generator produces automatic synchronization code for fields tagged with `[Sync]`.
4.  **Persistence**: Data tagged with `[Persistent]` are automatically captured by the `SnapshotManager`.

---

## 4. Usage Example
```csharp
public struct HealthComponent : INexusComponent {
    [NexusProgressBar(100, "Red")]
    public float Current;

    [Sync(SyncTarget.UI)]
    public float Max;
}

[Persistent]
public struct PlayerData : INexusComponent { ... }
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Attributes;

public class ReadOnlyAttribute : PropertyAttribute { }
public class RequiredAttribute : PropertyAttribute { }
public class SyncAttribute : Attribute {
    public SyncTarget Target { get; }
    public SyncAttribute(SyncTarget target = SyncTarget.Transform) => Target = target;
}
```

---

## Nexus Optimization Tip: Attribute Stripping
Architectural attributes like `[Persistent]` or `[Sync]` are only read by systems at runtime. However, visual attributes like `[ReadOnly]` are only needed in the Editor (outside the Build). Consider including Editor-only attributes within `#if UNITY_EDITOR` blocks to reduce the build size.
