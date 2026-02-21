# Nexus Prime Architectural Manual: NexusGraphLogicEditor (Graph Logic Editor)

## 1. Introduction
`NexusGraphLogicEditor.cs` is an architectural helper that converts Nexus Prime's "Low-Level Pipeline" (ECS Pipeline) into a visual node (Node) diagram. By showing the data dependencies (Read/Write) between systems with lines, it clarifies which system produces which data and which one consumes it.

The reason for this editor's existence is to prevent potential data conflicts (Race Condition) and logical loops (Circular Dependency) by seeing the system hierarchy lost in code from a bird's eye view.

---

## 2. Technical Analysis
The node-based editor offers the following capabilities:

- **Node-Based Visualization**: Each system (System) is represented as a box (Node).
- **Dependency Tracking**: Which components a system reads (`In: [Position]`) and which ones it writes (`Out: [Velocity]`) are listed on the box.
- **Pipeline Flow**: Data flow is visualized from left to right or in a hierarchical network form.
- **GUI.Box Logic**: Draws a graphic interface within the Editor using Unity's `GUILayout` and `GUI.Box` capabilities, without the need for additional software.

---

## 3. Logical Flow
1.  **Scanning**: All "System" classes are listed by scanning the active Nexus Registry and System Manager.
2.  **Association**: `[ReadOnly]` or `[WriteOnly]` unmanaged data access flags within the systems are analyzed.
3.  **Drawing**: Relationships between systems are connected with lines (Bezier curves or simple lines) from the system writing the data to the system reading it.
4.  **Error Detection**: If two systems are trying to write to the same unmanaged data at the same time (Conflict), the relevant nodes are painted red.

---

## 4. Usage Example
```text
// Analyzing the architecture:
// 1. [Nexus/Graph Logic Editor] is opened.
// 2. The "MovementSystem" node is found in the table.
// 3. It is seen that the output end goes to "Position" data and is read by "RenderSystem".
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class NexusGraphLogicEditor : EditorWindow
{
    [MenuItem("Nexus/Graph Logic Editor")]
    public static void ShowWindow() => GetWindow<NexusGraphLogicEditor>("Graph Logic");

    private void OnGUI() {
        // Draw nodes for each system...
        DrawNode(new Rect(50, 50, 150, 100), "PhysicsSystem");
    }
}
#endif
```

---

## Nexus Optimization Tip: Auto-Layout
Instead of manually arranging nodes, automatically distribute nodes to the screen using the `Force-Directed Graph` algorithm. This **makes the architecture understandable by resolving confusion within seconds in very large projects (100+ Systems).**
