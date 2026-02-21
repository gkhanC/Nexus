# Nexus Prime Architectural Manual: AutoSystemGenerator (Automatic System Graph Generation)

## 1. Introduction
`AutoSystemGenerator.cs` is the "smart" brain of Nexus Prime. It is a static orchestration tool that resolves data conflicts between dozens of different systems written by developers and prepares the most efficient parallel execution plan.

The reason for this generator's existence is to analyze the `[Read]` and `[Write]` tags of systems to create a **Directed Acyclic Graph (DAG)** and dynamically build the most intensive parallel plan that will not violate data safety without leaving any core of the processor idle.

---

## 2. Technical Analysis
AutoSystemGenerator undertakes the following main tasks for system management:

- **Conflict Analysis**: Detects which systems write to the same component. Separates systems with write conflicts into different time slots (Layer/Sync Point).
- **Dependency Injection Orchestration**: Automatically injects tools required by systems at runtime, such as `Registry`, `EntityCommandBuffer`, into `[Inject]` fields.
- **Optimal Pathing**: Ensures that the processor can use all threads simultaneously by putting all independent systems into the same "Layer".
- **Waste Cycle Prevention**: Detects threads waiting due to unnecessary dependencies and prevents wasted clock cycles by narrowing the graph (Compact Graph).

---

## 3. Logical Flow
1.  **Scanning**: All registered `INexusSystem` types are scanned via Reflection.
2.  **Dependency Map**: Read/write requirements of systems are converted into a matrix.
3.  **Graph Construction**: Systems are divided into layers using Kahn's Algorithm or a similar sorting method.
4.  **Injection**: Before each layer is triggered, all necessary references are written to the system fields.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Execution Graph** | A flow chart showing the dependency of jobs on each other. |
| **DAG (Directed Acyclic Graph)** | A workflow graph with a specific direction and no cycles. |
| **Sync Point** | A barrier that waits for all parallel jobs to finish and allows passage to the next stage. |
| **Waste Cycles** | The time interval during which the processor waits idle due to a dependency. |

---

## 5. Risks and Limits
- **Circular Dependency**: If System A is waiting for B and System B is waiting for A, the graph locks (Deadlock). The generator should detect this situation and throw an error.
- **Complexity**: Reconstructing the graph every frame in very large projects with hundreds of systems can be costly. Therefore, Nexus only reconstructs the graph when a system is added/removed (`RebuildSystemGraph`).

---

## 6. Usage Example
```csharp
// Create the graph while JobSystem is being initialized
public void Configure(JobSystem jobs)
{
    // After systems are added
    AutoSystemGenerator.RebuildSystemGraph(jobs);
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Core;

public static class AutoSystemGenerator
{
    public static void RebuildSystemGraph(JobSystem jobSystem)
    {
        // 1. Analyze all systems for Read/Write conflicts.
        // 2. Build an optimal execution graph.
        // 3. Inject dependencies via [Inject].
    }
}
```

---

## Nexus Optimization Tip: Explicit Ordering Optimization
If there are too many "Write" conflicts between your systems, AutoSystemGenerator is forced to arrange them vertically (sequentially). By breaking your data structures into smaller pieces (for example, more specific components instead of `Health` and `Position`), you can **ensure the generator puts more systems into the same parallel layer and increase performance by 50%.**
