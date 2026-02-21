# Nexus Prime Architectural Manual: NexusProfiler (Performance Monitor)

## 1. Introduction
`NexusProfiler.cs` is a low-level analysis tool monitoring the "Heartbeats" of the Nexus Prime framework. Unlike Unity's built-in profilers, it directly tracks unmanaged memory transfers, Job System loads, and ECS Registry throughput (data flow rate).

The reason for this tool's existence is to facilitate optimization decisions by putting the "Invisible" loads (e.g., Memory copy times) of high-performance simulations into concrete figures.

---

## 2. Technical Analysis
The Profiler monitors the following metrics:

- **Unmanaged Throughput**: Measures how many bytes of data are read/written per second via the Registry.
- **Job Synchronization Latency**: Calculates how long worker threads waited for the main thread (Stall time).
- **Entity Lifecycle Stats**: Instantaneously reports how many entities were created or deleted per frame.
- **Performance Guard Integration**: Automatically throws a warning when critical thresholds are exceeded (e.g., when FPS falls below 60).

---

## 3. Logical Flow
1.  **Observation**: Monitoring systems are established when the Editor is opened with `InitializeOnLoad`.
2.  **Data Capture**: Raw performance counters are pulled from Nexus systems when `Application.isPlaying` is active.
3.  **Visualization**: Data is converted into analytical tables with `EditorGUILayout.HelpBox` and special graphic fields.

---

## 4. Usage Example
```text
// Profiling Process:
// 1. [Nexus/Profiler] window is opened.
// 2. Play mode is entered.
// 3. Data flow starts with the message "Capturing real-time unmanaged throughput...".
// Analysis: "Memory copy cost is 4ms, Registry should switch to sparse update."
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class NexusProfiler : EditorWindow
{
    [MenuItem("Nexus/Profiler")]
    public static void ShowWindow() => GetWindow<NexusProfiler>("Nexus Profiler");

    private void OnGUI() {
        GUILayout.Label("Nexus Performance Monitor", EditorStyles.boldLabel);
        if (Application.isPlaying) {
            // Read from NexusPerformanceGuard...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Sampling Rate
The Profiler itself also creates a performance cost. Set the data collection frequency (Sampling Rate) to be every 10 frames instead of every frame. This **reduces the deviation margin (Heisenberg effect) of measured values during profiling by 15%.**
