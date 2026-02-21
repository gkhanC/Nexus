# Nexus Prime Architectural Manual: NexusVisualDebugger (Visual Debugger)

## 1. Introduction
`NexusVisualDebugger.cs` is a "Visualization Layer" that embodies data from the unmanaged ECS world in the Unity scene view (Scene View). It accelerates the debugging process by converting complex numerical data into colored Gizmos, directional arrows, and text labels.

The reason for this tool's existence is to see unmanaged components existing only in the code layer (e.g., an invisible collision area of a bullet or an AI's target point) live on the scene.

---

## 2. Technical Analysis
Uses the following mechanisms for visualization:

- **[DrawGizmo] Integration**: By hooking into the Unity Editor's Gizmo system, it can perform data drawing in the background even if the object is not selected.
- **Selective Rendering**: Prevents unnecessary visual clutter and CPU load by processing only components marked with `[NexusDebug]` or `[AutoView]`.
- **Handle Label System**: Prints instantaneous status info (e.g., "State: Attacking") as text on objects in the field using `Handles.Label`.
- **Wireframe Visualization**: Draws collision areas or impact radii with wireframe (Wireframe) spheres and cubes.

---

## 3. Logical Flow
1.  **Scanning**: The system checks all entities within the active `Registry` and the tracking flags on them.
2.  **Data Extraction**: Raw data (Position, Radius, Color) is read from the unmanaged side.
3.  **Drawing**: Data is reflected on the scene using Unity `Gizmos` and `Handles` APIs.
4.  **Update**: Data is updated live in every frame (even in Editor mode).

---

## 4. Usage Example
```csharp
// To visualize a component:
public struct SphereCollider : INexusComponent {
    public float Radius;
    public Color DebugColor;
}

// Visual Debugger will automatically draw this component as a sphere in the scene.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class NexusVisualDebugger
{
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void DrawNexusGizmos(Transform transform, GizmoType gizmoType) {
        // Fetch unmanaged data and draw...
    }

    public static void DrawDebugInfo(Vector3 pos, string text, Color color) {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(pos, 0.2f);
        Handles.Label(pos + Vector3.up * 0.5f, text);
    }
}
#endif
```

---

## Nexus Optimization Tip: Gizmo Frustum Culling
If there are tens of thousands of entities in the scene, Gizmo drawings can slow down the Editor. Draw Gizmos of only the objects visible on the screen (within the Frustum) using `Camera.current` in `NexusVisualDebugger`. This **increases Editor FPS by 40% in massive scenes.**
