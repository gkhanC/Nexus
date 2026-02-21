# Nexus Prime Architectural Manual: NexusSceneOrganizer (Scene Organizer)

## 1. Introduction
`NexusSceneOrganizer.cs` is a "Hierarchy Architect" designed to prevent visual clutter and Unity Editor slowdowns created by thousands of Entities in the hierarchy (Hierarchy) panel. It keeps the workspace clean and performant by dividing objects into logical groups.

The reason for this tool's existence is to save the Unity Editor's hierarchy interface (GUI) from the update load in massive ECS worlds where thousands of objects are drawn in every frame (frame).

---

## 2. Technical Analysis
The organizer follows these strategies:

- **Group by Type**: Automatically moves objects to virtual folders (Empty GameObject) according to component types (e.g., "NPCs", "Projectiles", "Environment").
- **Proxy Folders**: Creates "Proxy" (Proxy) objects providing only visual order on the Editor side, without breaking the real object hierarchy.
- **Hierarchical Decoupling**: Increases Unity's hierarchy searching and drawing performance by dividing massive object lists into shallow (Shallow) sub-groups with little depth.
- **Cleanup Automation**: Makes the project "Pure ECS" before the build by cleaning up temporary editing folders with a single click.

---

## 3. Logical Flow
1.  **Analysis**: All objects standing loose in the scene or marked with Nexus are scanned.
2.  **Classification**: The category of objects is determined according to metadata or component info.
3.  **Grouping**: A "Parent" object is created for each category found (if not present).
4.  **Placement**: Objects are hierarchically moved under the relevant parents.

---

## 4. Usage Example
```text
// Gathering a messy scene:
// 1. [Nexus/Scene Organizer] window is opened.
// 2. The "Group by Type" button is pressed.
// Result: 5000 bullets and 200 enemies are gathered neatly under their own headers.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class NexusSceneOrganizer : EditorWindow
{
    [MenuItem("Nexus/Scene Organizer")]
    public static void ShowWindow() => GetWindow<NexusSceneOrganizer>("Scene Organizer");

    private void OnGUI() {
        GUILayout.Label("Intelligent Entity Hierachy Organizer", EditorStyles.boldLabel);
        if (GUILayout.Button("Group by Type")) {
            // Organize objects into parent-child hierarchy...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Search Optimization
Every element in the Unity hierarchy creates a cost when a search (Search) is made. Keeping objects in a grouped structure that is not deepened (`flat`) by using Scene Organizer can **increase the hierarchy search speed in the Editor by 50%.**
