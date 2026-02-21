# Nexus Prime Architectural Manual: Bottleneck Visualizer (Bottleneck Visualizer)

## 1. Introduction
`BottleneckVisualizer.cs` is a visual warning system reflecting Nexus Prime's "Real-Time Profiler" data onto the Unity scene view (Scene View). It shows which system or which object tires the processor (CPU) how much with colored bars.

The reason for this tool's existence is to immediately see where the bottleneck is physically ("This enemy group is working very heavy!") instead of getting lost among complex performance tables.

---

## 2. Technical Analysis
Visualization uses the following methods:

- **Handles.HighLevel UI**: Draws 2D bars and text boxes onto the 3D world using the Unity Editor's `Handles` class.
- **System Cost Mapping**: Takes millisecond (ms) based data provided by `NexusPerformanceGuard`.
- **Color Coding**: Colors systems as **Green** between 0-5ms, **Yellow** between 5-10ms, and **Red** for 10ms+.
- **Frustum Dependent Rendering**: Preserves Editor performance by drawing only in the area seen by the camera.

---

## 3. Logical Flow
1.  **Measurement**: `NexusProfiler` and sub-systems measure the cost of each frame.
2.  **Conversion**: Entity positions in world coordinates are projected onto screen coordinates (Screen Space).
3.  **Drawing**: A bar graph showing the relevant cost is drawn over the head of each system/object.

---

## 4. Usage Example
```csharp
// Showing a system's cost in the scene:
BottleneckVisualizer.DrawSystemMetrics(screenPos, "PhysicsJob", 2.4f);

// Result: A green bar labeled PhysicsJob and 25% full appears on the screen.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class BottleneckVisualizer
{
    public static void DrawSystemMetrics(Vector2 screenPos, string name, float costMs) {
        // Use Handles UI to draw bar and label...
    }
}
#endif
```

---

## Nexus Optimization Tip: Cluster Visualization
Instead of drawing separate bars for each of thousands of bullets, see the bullet group as a "Cluster" (Cluster) and draw a single aggregate cost bar. This **allows you to focus on the actual problem by reducing visual clutter by 90% during profiling.**
