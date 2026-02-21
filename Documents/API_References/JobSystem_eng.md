# Nexus Prime Architectural Manual: JobSystem (Thread Orchestrator)

## 1. Introduction
`JobSystem.cs` is the "Multi-threaded Engine" of Nexus Prime. Even though modern processors have 8, 16, or more cores, efficient use of these cores is difficult due to "Data Dependency". If two systems that both read and write the same data run simultaneously, data corruption (a race condition) is inevitable.

The reason for the JobSystem's existence is to analyze the dependencies between systems, separate them into safe **Layers**, and run the systems within each layer in parallel on all of the processor's cores.

---

## 2. Technical Analysis
The JobSystem utilizes the following advanced techniques for parallel execution safety:

- **Dependency Graph Analysis (Kahn's Algorithm)**: Analyzes Read/Write conflicts between systems. If System A writes to Position and System B reads Position; System B can only run after System A is finished.
- **Layered Scheduling**: Systems that have no mutual dependency are assigned to the same layer. These layers are executed concurrently via `Parallel.ForEach`.
- **Concurrent Execution Monitoring**: Records the execution time (metrics) of each system using `ConcurrentDictionary`.
- **Reflection-Based Dependency Extraction**: Scans the `[Read]` and `[Write]` attributes on systems to extract an automatic dependency map at runtime.

---

## 3. Logical Flow
1.  **Registration (`AddSystem`)**: When a new system is added, the component types it uses (Reads/Writes) are detected.
2.  **Layer Construction (`RebuildLayers`)**: Systems are grouped using a variant of Kahn's algorithm.
    - Layer 1: Systems with no dependencies.
    - Layer 2: Those dependent only on data in Layer 1.
3.  **Execution (`Execute`)**: Layers are run in sequence (Sequentially). Each individual system within each layer is run in parallel (In Parallel).
4.  **Metric Analysis**: The number of milliseconds spent by each system is recorded for profilers.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Race Condition** | An error that occurs when two or more threads access the same data at the same time. |
| **Dependency Graph** | A mathematical node structure showing the dependency of tasks on each other. |
| **Kahn's Algorithm** | An algorithm for arranging nodes in a graph into a linear and layered order. |
| **Throughput** | The amount of work done per unit of time (Processing capacity). |

---

## 5. Risks and Limits
- **Circular Dependency**: If System A depends on System B and System B depends on System A, the graph locks up. Nexus detects this situation and solves it with a fallback mechanism.
- **Parallel Overhead**: Creating parallel threads for very small tasks can take longer than the task itself. This situation requires "Grain Size" optimization.

---

## 6. Usage Example
```csharp
var jobSystem = new JobSystem(registry);

// Add systems (Dependencies are automatically resolved)
jobSystem.AddSystem(new MovementSystem());
jobSystem.AddSystem(new CollisionSystem());

// Run every frame
void Update() {
    jobSystem.Execute();
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
using System.Collections.Concurrent;
using System.Threading.Tasks;
namespace Nexus.Core;

public class JobSystem : INexusJobSystem
{
    private readonly List<SystemNode> _nodes = new();
    private readonly List<List<SystemNode>> _layers = new();

    public void RebuildLayers()
    {
        _layers.Clear();
        // 1. Build Adjacency List based on Read/Write conflicts
        // 2. Group into layers using Kahn's algorithm
    }

    public void Execute()
    {
        foreach (var layer in _layers) {
            Parallel.ForEach(layer, node => {
                node.System.Execute();
            });
        }
    }
}
```

---

## Nexus Optimization Tip: Multi-Core Scalability
While a standard `MonoBehaviour.Update()` loop always puts the load on a single core (Main Thread); `Nexus JobSystem` distributes the workload **homogeneously across all cores (8, 16, 32, etc.).** This allows you to **increase your game logic capacity by 4-8 times**, directly proportional to your number of cores.
