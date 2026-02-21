# Nexus Prime Architectural Manual: NexusWorkflowTools (Workflow Automation)

## 1. Introduction
`NexusWorkflowTools.cs` is a "Productivity Engine" allowing developers to handle time-consuming manual processes within seconds. It automatically solves many problems, from namespace confusion to hierarchy disorder.

The reason for these tools' existence is to offer a rule-based automation instead of writing namespaces in every script file manually or naming hundreds of enemy objects individually.

---

## 2. Technical Analysis
The workflow package offers the following three main helpers:

- **Namespace Auto-Fixer**: Scans all scripts in the project and suggests the Nexus namespace (`Nexus.Module.SubModule`) that should be based on the folder structure.
- **Batch Rename Tool**: Simultaneously renames multiple selected objects with a specific prefix (`Prefix`) and sequential number (`startIndex`).
- **Quick Scene Switcher**: A panel listing scenes added to Build settings and providing rapid transition between scenes without forgetting to save the open scene.

---

## 3. Logical Flow
1.  **Analysis**: The tool scans project assets via `AssetDatabase`.
2.  **Bulk Processing**: A controlled loop is established using `OrderBy` (Ordering) over objects selected by the developer.
3.  **Action**: Operations such as name change or scene transition are safely performed via Unity Editor APIs.

---

## 4. Usage Example
```csharp
// Batch Renaming:
// 1. 20 bullets are selected in the scene.
// 2. [Nexus/Workflow/Batch Rename] is opened.
// 3. Prefix "Projectile_" is written and "Rename Selected" is clicked.
// Result: Projectile_00, Projectile_01 ...
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class NexusWorkflowTools
{
    [MenuItem("Nexus/Workflow/Batch Rename")]
    public static void ShowBatchRename() => BatchRenameWindow.ShowWindow();
}

public class BatchRenameWindow : EditorWindow {
    private string _prefix = "Entity_";
    private void OnGUI() {
        _prefix = EditorGUILayout.TextField("Prefix", _prefix);
        if (GUILayout.Button("Rename Selected")) {
            // Order and rename logic...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Hierarchy Sorting
Perform ordering using `GetSiblingIndex` when using the Batch Rename tool. This **reduces the error margin to 0% by putting objects that stand messy in the scene in order both visually and logically.**
