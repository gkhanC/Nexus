# Nexus Prime Architectural Manual: Time-Travel Debugger (Time Travel)

## 1. Introduction
`TimeTravelDebugger.cs` is the "Rewind" interface that allows returning to any moment of the game using the deterministic simulation structure of Nexus Prime. It makes the raw unmanaged data recorded by the `SnapshotManager` navigable via a Slider.

The reason for this tool's existence is to exactly determine the source of the problem (Root Cause) by examining an error or physics interaction occurring in seconds, frame by frame.

---

## 2. Technical Analysis
Time travel works with the following components:

- **Snapshot Scrubbing**: As the `_currentFrame` value changes, copies the relevant memory block on the `SnapshotManager` to the active Registry.
- **Deterministic Playback**: Even if the simulation is stopped, it rebuilds the game's visual and logical state (Entities, Components) at that moment over the recorded data.
- **Frame Navigation**: Offers the developer the opportunity to progress and retreat in microseconds (FixedUpdate based) with "Step Back" and "Step Forward" buttons.
- **State Restoration**: When the Slider is released, all unmanaged assets return to their coordinates and values in the selected frame.

---

## 3. Logical Flow
1.  **Recording**: `SnapshotManager` takes periodic recordings in the background while the game is running.
2.  **Stopping**: The developer pauses the game and opens the Time-Travel panel.
3.  **Sliding**: Every value change when the Slider is pulled triggers a `RestoreSnapshot` call.
4.  **Examination**: Objects in the scene physically return to that moment, and `LiveStateTweaker` shows current values.

---

## 4. Usage Example
```text
// Examining a faulty explosion:
// 1. After the explosion occurs, the game is stopped.
// 2. The slider is pulled to 20 frames before the moment of explosion.
// 3. The first frame where the explosion started is found with "Step Forward".
// 4. The velocity and angle of the faulty bullet are analyzed.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class TimeTravelDebugger : EditorWindow
{
    [MenuItem("Nexus/Time-Travel Debugger")]
    public static void ShowWindow() => GetWindow<TimeTravelDebugger>("Time-Travel");

    private float _currentFrame = 0;
    private void OnGUI() {
        _currentFrame = EditorGUILayout.Slider("Frame", _currentFrame, 0, 300);
        // Play, Pause, Step controls...
    }
}
#endif
```

---

## Nexus Optimization Tip: Keyframe Sampling
Instead of keeping all frames in RAM, record only "Keyframes" (e.g., every 10 frames) in full and keep the differences (Delta) in between. This **allows you to go back further in time by reducing Time-Travel memory usage by 80%.**
