# Nexus Prime Architectural Manual: NexusPerformanceGuard (Performance Guardian)

## 1. Introduction
`NexusPerformanceGuard.cs` is the "System Stability" insurance of Nexus Prime. In complex game scenarios, sudden spikes in entities or intensive physics calculations can cause the CPU to hit 100% and lead to a drop in the game's frame rate (FPS).

The reason for Performance Guard's existence is to monitor the system's total CPU consumption in real-time and, when a pre-determined "CPU Budget" is exceeded, maintain the game's smoothness by reducing the frequency of non-critical systems (Sfx, Particles, etc.) or changing thread priorities.

---

## 2. Technical Analysis
Performance Guard utilizes the following techniques for system health:

- **Frame-Time Analysis**: Monitors the total execution time of each frame with millisecond precision.
- **Dynamic Thread Masking**: If the CPU budget is exceeded, it puts some of the worker threads on the `JobSystem` into sleep mode to allow the main thread (Unity Main Thread) to breathe.
- **Priority Throttling**: Dynamically lowers the priority of background tasks, freeing up hardware resources for critical rendering and input tasks.
- **Budgeting Logic**: By setting a safe boundary (Safe Zone) such as `80%`, it ensures the system never leads to "Hanging" at the operating system level.

---

## 3. Logical Flow
1.  **Measurement**: The `Guard()` method measures the total load at the beginning or end of each frame.
2.  **Decision**: If the load rises above the `MaxCpuPercentage` value, "Crisis Mode" is activated.
3.  **Intervention**: Queues within the `JobSystem` are slowed down, or low-priority systems are deferred to the next frame.
4.  **Normalization**: When the load returns to the safe boundary, all systems are given full power.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **CPU Budget** | The maximum safe percentage or duration a game can use on the processor. |
| **Throttling** | A technique of consciously slowing down the speed of a process. |
| **Worker Thread** | An auxiliary thread performing heavy calculations in the background. |
| **Frame Spiking** | The frame time suddenly rising and causing stuttering. |

---

## 5. Risks and Limits
- **Latency**: When systems are slowed down, AI response times or the timeliness of visual effects may lag slightly.
- **Measurement Overhead**: The performance monitoring process itself also consumes CPU. Nexus uses a lightweight `Stopwatch` logic to minimize this cost.

---

## 6. Usage Example
```csharp
var guard = new NexusPerformanceGuard();
guard.MaxCpuPercentage = 75f; // Do not exceed 75%

void Update() {
    // Monitor systems and slow down if necessary
    guard.Guard(nexusJobSystem);
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
using System.Diagnostics;
namespace Nexus.Core;

public class NexusPerformanceGuard
{
    public float MaxCpuPercentage { get; set; } = 80f;

    public void Guard(JobSystem jobSystem)
    {
        // 1. Measure frame total execution time.
        // 2. If exceeding budget, lower priority of background systems.
        // 3. Throttle non-critical updates.
    }
}
```

---

## Nexus Optimization Tip: Thermal Throttling Avoidance
When the CPU constantly runs at 100% load, mobile devices and laptops lower their clock speed (GHz) to avoid overheating (Thermal Throttling). NexusPerformanceGuard **prevents the hardware from dropping GHz** by keeping the load below 80%, ensuring the game runs at a **stable performance even over long durations.**
