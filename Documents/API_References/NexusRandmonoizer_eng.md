# Nexus Prime Architectural Manual: NexusRandmonoizer (Variation Engine)

## 1. Introduction
`NexusRandmonoizer.cs` is an Editor helper tool allowing for rapid diversification of transform (Position, Rotation, Scale) values of hundreds or thousands of objects selected in the scene. It creates the "irregularity" (irregularity) necessary for a natural look within seconds.

The reason for this tool's existence is to increase the scene quality by breaking the artificial (robotic) look resulting from many objects such as trees, rocks, or bullets having the same direction and the same scale.

---

## 2. Technical Analysis
The Randomizer offers the following operational capabilities:

- **Multi-Axis Position Logic**: Ensures that the objects are distributed only on the desired axes with the choice of `AxisType` (X, Y, Z, XY, XZ, YZ, All).
- **Unit Circle/Sphere Randomization**: Provides more organic (clustered) settlements by randomly distributing objects within a circle or sphere volume.
- **Rotation Variation**: Breaks symmetry by randomly rotating objects between 0-360 degrees.
- **Uniform Scale Randomization**: Prevents form distortions by changing the X, Y, and Z scales of the objects at the same rate (proportional).

---

## 3. Logical Flow
1.  **Selection Capture**: Objects currently selected in the scene are listed via `Selection.gameObjects`.
2.  **Parameter Determination**: The developer selects Min/Max ranges and the axis type.
3.  **Cyclic Process**: New values are calculated with `Random.Range` for each object in the list.
4.  **Application**: Calculated new `localPosition`, `localRotation`, or `localScale` values are assigned to the objects.

---

## 4. Usage Example
```text
// While creating a forest:
// 1. 100 tree prefabs are selected.
// 2. "Randomize Rotation" is clicked via the Window.
// 3. "Randomize Scale" (Min: 0.8, Max: 1.2) is clicked.
// Result: An organic forest look standing different from each other.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class NexusRandmonoizer
{
    public static void RandomizePosition(float min, float max, AxisType axis = AxisType.All) {
        foreach (var go in Selection.gameObjects) {
            // Calculate and apply random pos based on axis...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Local vs World Space
If possible, always perform randomization via `localPosition` and `localRotation`. This **allows you to diversify only the child objects without breaking the parent (parent) objects' order.**
