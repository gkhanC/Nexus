# Nexus Prime Architectural Manual: INexusJobSystem (System Orchestration Contract)

## 1. Introduction
`INexusJobSystem.cs` is the high-level interface defining the job execution layer of the Nexus Prime framework. It is the contract for the orchestrator that determines in what order and using what hardware resources all logical systems (`INexusSystem`) will be run.

The reason for this interface's existence is to break the connection between the game loop and business logic, to automatically parallelize systems according to data dependencies, and to make the cost of each job on the processor (Metrics) traceable.

---

## 2. Technical Analysis
INexusJobSystem mandates the following capabilities for high-performance system management:

- **System Registration**: Every system registered with `AddSystem()` is subjected to dependency analysis via Reflection. This eliminates manual "Thread" management.
- **Dependency-Aware Execution**: When `Execute()` is called, systems are triggered in parallel on the most appropriate cores without blocking each other (Data Race priority).
- **Performance Monitoring**: Provides scientific data on how many microseconds each system took to complete via `GetLastExecutionMetrics()`.
- **Thread Safety Abstraction**: Prevents the developer from dealing with lock (Lock/Mutex) mechanisms; it provides parallel safety at the interface level through [Read]/[Write] attributes.

---

## 3. Logical Flow
1.  **Registration**: Systems are added to the `JobSystem` at the start of the game.
2.  **Ordering**: A dependency graph (DAG) between systems is created.
3.  **Triggering**: The main loop calls the `Execute()` method every frame.
4.  **Analysis**: Performance metrics are collected and presented to the developer for optimization.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Orchestrator** | Coordinator of threads and systems. |
| **Execution Metrics** | Working time, CPU usage, and latency data of a job. |
| **Worker Thread** | Auxiliary processor arm doing heavy calculations in the background. |
| **Dispatcher** | The unit that distributes jobs to available empty cores. |

---

## 5. Risks and Limits
- **Main Thread Stalls**: If there is a very heavy and non-parallel job within `Execute()`, the game's frame rate (FPS) may drop.
- **Ordering Dependencies**: If two systems write to the same data, the interface guarantees their order, but this limits parallelism.

---

## 6. Usage Example
```csharp
public void SetupGame(INexusJobSystem jobSystem, Registry registry)
{
    // Add systems
    jobSystem.AddSystem(new PhysicsSystem());
    jobSystem.AddSystem(new AISystem());

    // Run every frame
    void OnUpdate() {
        jobSystem.Execute();
        
        // Monitor performance
        var metrics = jobSystem.GetLastExecutionMetrics();
        foreach(var m in metrics)
            Console.WriteLine($"{m.SystemName}: {m.DurationMs}ms");
    }
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
using System.Collections.Generic;
namespace Nexus.Core;

public interface INexusJobSystem
{
    void AddSystem(INexusSystem system);
    void Execute();
    List<JobSystem.ExecutionMetrics> GetLastExecutionMetrics();
}
```

---

## Nexus Optimization Tip: Cycle Budgeting
Using `GetLastExecutionMetrics()` data, set a **Clock Cycle Budget** for each frame. If a system exceeds the budget, aim to get maximum benefit from parallelism by dividing it into smaller pieces (Jobs).
