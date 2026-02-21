# Nexus Prime Architectural Manual: NexusDashboard (Nexus Hub)

## 1. Introduction
`NexusDashboard.cs` is the main command center for all Nexus Prime tools and is called "The Hub" within the Editor. It allows the developer to manage complex unmanaged systems, optimization tools, and visual debuggers from a single window.

The reason for this interface's existence is to offer a logically grouped "Developer OS" (Developer Operating System) experience, instead of getting lost among dozens of different editor windows.

---

## 2. Technical Analysis
The Dashboard organizes tools into three main categories:

- **Architectural & Logic**: Low-level systems such as constraint checkers (Constraint Checker), DI engine, Graph Editor, and Bit-Level compression tools.
- **Unity Editor & DX**: Visual debugger (Visual Debugger), Time-travel (Time-Travel), heat maps (Memory Heatmap), and Prefab converters.
- **Multimedia & Integration**: Shader bridges, Audio linkers, VFX providers, and physics integration tools.

---

## 3. Logical Flow
1.  **Opening**: The developer opens the panel from the `Nexus -> The Hub` menu.
2.  **Grouping**: The `DrawGroup` method provides a visual hierarchy by putting tools into boxes.
3.  **Initialization**: The "Initialize All Systems" button prepares all Nexus sub-systems (Registry, Pool, Sync, etc.) at once.
4.  **Quick Access**: Each button triggers the relevant tool's Editor window.

---

## 4. Usage Example
```csharp
// System initialization via Dashboard
// 1. [Nexus/The Hub] is opened.
// 2. [Initialize All Systems] is clicked.
// 3. The message "Nexus: Initializing Developer OS Toolset..." is seen in the Console.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class NexusDashboard : EditorWindow
{
    [MenuItem("Nexus/The Hub (Dashboard)")]
    public static void ShowWindow() => GetWindow<NexusDashboard>("Nexus Dashboard");

    private void OnGUI() {
        GUILayout.Label("Nexus Developer OS - The Hub", EditorStyles.boldLabel);
        // Draw groups...
    }
}
#endif
```

---

## Nexus Optimization Tip: Window Docking
Dock (Dock) the Nexus Dashboard next to the "Inspector" or under the "Game View" in the Unity interface. This way, you **100% eliminate the time spent searching for windows to change system states during development.**
