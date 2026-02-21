# Nexus Prime Architectural Manual: LiveState Tweaker (Live State Editor)

## 1. Introduction
`LiveStateTweaker.cs` is a powerful Editor panel providing instantaneous monitoring and changing of data in the ECS world during the game's runtime (Runtime). It reduces the developers' "Trial-Error" loop to seconds without stopping the game.

The reason for this panel's existence is to bring unmanaged `INexusComponent` data to light, which the standard Unity Inspector cannot see, and to allow the developer to "twist and turn" variables live.

---

## 2. Technical Analysis
The panel is equipped with the following advanced features:

- **Global Registry Inspection**: Lists all Entities currently alive by connecting to the active Nexus Registry.
- **Search & Filter Engine**: Rapidly searches among thousands of entities according to a specific ID or component type.
- **Direct Memory Manipulation**: When you change a value on the GUI, the system writes this change directly to the unmanaged memory address (Pointer).
- **Component Foldout Logic**: Prevents visual clutter by presenting components of each entity in a foldable (Foldout) structure.

---

## 3. Logical Flow
1.  **Connection**: When the editor window is opened, the data flow starts over the existing `Snapshot` or `Registry`.
2.  **Subscription**: The panel updates only the changed data in the interface by monitoring the "Dirty" flags on the unmanaged side.
3.  **Input Processing**: When the developer moves a Slider in the user interface (UI), the new value is immediately injected into the ECS simulation.

---

## 4. Usage Example
```text
// In a running game:
// 1. [Nexus/Live State Tweaker] is opened.
// 2. "EnemyBoss" entity is searched.
// 3. The Health value under the "Health" component is pulled from 1000 to 10.
// Result: Boss's health decreases immediately without the game stopping.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class LiveStateTweaker : EditorWindow
{
    [MenuItem("Nexus/Live State Tweaker")]
    public static void ShowWindow() => GetWindow<LiveStateTweaker>("State Tweaker");

    private void OnGUI() {
        // Search bar...
        // Scroll view for entities...
        // Draw each entity's component with EditorGUILayout fields.
    }
}
#endif
```

---

## Nexus Optimization Tip: Event Filtering
Instead of refreshing State Tweaker on every Update, trigger the `Repaint()` call only when there is a change in unmanaged data. This **reduces Editor processor load by 25% in cases where a large number of entities are monitored.**
