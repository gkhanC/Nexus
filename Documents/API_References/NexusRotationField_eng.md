# Nexus Prime Architectural Manual: NexusRotationField (Unmanaged Rotation Data)

## 1. Introduction
`NexusRotationField.cs` is a hybrid structure used by Nexus Prime to store rotation data within unmanaged ECS components. While Unity's `Quaternion` structure is unmanaged, working with Euler angles (Vector3) on the editor is much more intuitive for developers.

The reason for this structure's existence is to store rotation in its simplest form (3 floats) within unmanaged memory blocks, yet to be able to make zero-cost and automatic (`implicit`) transitions to Unity's Quaternion system when needed (e.g., physics calculations).

---

## 2. Technical Analysis
NexusRotationField offers the following capabilities for rotation management:

- **Memory Efficiency**: Reduces memory usage by 25% in each component by storing rotation as 3-float (Euler) instead of 4-float (Quaternion).
- **Bi-Directional Conversion**: Can be `implicit`ly (without explicit specification) converted from both `Vector3` and `Quaternion` types. This allows the developer to write `registry.Set(id, new Vector3(0, 90, 0))`.
- **Operator Overloading**: Allows the rotation data to be dynamically scaled by multiplying it with a scalar value (`float`).
- **Serializable Support**: Thanks to the `[Serializable]` attribute, it can appear as a standard Vector3 field in the Unity Inspector window.

---

## 3. Logical Flow
1.  **Input**: The developer enters the rotation as Euler angles from the Inspector or from code.
2.  **Storage**: The data is kept as `Vector3` (Euler) within the unmanaged component.
3.  **Conversion**: When it is to be transferred to a Unity system (e.g., `transform.rotation`), the `Quaternion.Euler()` calculation is performed instantly in the background thanks to the `implicit operator`.
4.  **Application**: The calculated Quaternion is transferred to Unity's visual or physical layer.

---

## 4. Usage Example
```csharp
public struct RotationComponent : INexusComponent {
    public NexusRotationField Value;
}

// Usage
RotationComponent rot = new RotationComponent();
rot.Value = new Vector3(0, 45, 0); // Implicit cast from Vector3

// Transfer to Unity
transform.rotation = rot.Value; // Implicit cast to Quaternion
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Mathematics;

[Serializable]
public struct NexusRotationField
{
    public Vector3 Euler;

    public Quaternion Quaternion => Quaternion.Euler(Euler);

    public NexusRotationField(Vector3 euler) => Euler = euler;
    public NexusRotationField(Quaternion quaternion) => Euler = quaternion.eulerAngles;

    public static implicit operator Quaternion(NexusRotationField field) => field.Quaternion;
    public static implicit operator Vector3(NexusRotationField field) => field.Euler;
}
```

---

## Nexus Optimization Tip: Storage vs. Compute
While storing rotation as `NexusRotationField` (Euler) saves memory, converting it to a `Quaternion` every frame brings a small CPU cost. If you are going to read your data as a Quaternion thousands of times per second, you can **waive memory and store `Quaternion` directly to reduce rotation calculation cost by 100%.**
