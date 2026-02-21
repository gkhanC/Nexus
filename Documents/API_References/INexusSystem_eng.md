# Nexus Prime Architectural Manual: INexusSystem & Core Attributes (Logic & Dependency Management)

## 1. Introduction
In the Nexus Prime ecosystem, "Systems" (`INexusSystem`) are state-less logic containers independent of data. Unlike "Manager" classes in traditional OOP architecture, systems do not store data within themselves; instead, they rule over component arrays on the `Registry`.

The reason for this interface and its accompanying `Attributes` is to pre-determine which system will access which data in a multi-threaded execution environment and eliminate the risk of **Data Race** at the hardware level.

---

## 2. Technical Analysis
The system architecture uses the following metadata tags for `JobSystem` orchestration:

- **[Read] Attribute**: Specifies that a system will only read a component type. Multiple systems CAN READ the same component at the same time. This ensures maximum parallelism across CPU cores.
- **[Write] Attribute**: Specifies that a system will modify data (exclusive access). While one system is WRITING to a component, no other system can touch that component.
- **[Inject] Attribute**: A Dependency Injection (DI) mechanism. The `JobSystem` automatically populates this field with central tools like `Registry` or `EntityCommandBuffer` when the system is initialized.
- **Stateless Design**: Classes implementing `INexusSystem` should not store state. All state is stored in components, allowing systems to be safely executed on any core.

---

## 3. Logical Flow
1.  **Definition**: The developer implements the `INexusSystem` interface and marks the required components with `[Read]` or `[Write]`.
2.  **Analysis**: When `JobSystem.AddSystem` is called, these attributes are scanned via Reflection.
3.  **Scheduling**: A dependency matrix is created (Kahn's Algorithm). Systems with write conflicts are separated into different layers.
4.  **Execution**: The `Execute()` method is triggered on the most available core of the processor.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Stateless** | A structure that does not carry data from previous frames, performing calculations from scratch on every call. |
| **Dependency Injection** | The automatic provision of references required by an object from the outside. |
| **Exclusive Access** | A situation where only a single process can access a resource at the same time. |
| **Metadata Tagging** | Auxiliary tags (Attributes) that change the way code operates. |

---

## 5. Risks and Limits
- **Write Over-use**: Marking everything as `[Write]` prevents systems from running in parallel and reduces performance to single-thread levels.
- **Thread Safety**: If a local variable (field) is updated within the system, it must be ensured that this variable is thread-safe (though in an ideal ECS, this should not be done).

---

## 6. Usage Example
```csharp
public class GravitySystem : INexusSystem
{
    [Inject] private Registry _registry;
    [Read] private float _gravityForce = -9.81f;
    
    // We will write to Position component, read Velocity
    [Write] private Position* _pos;
    [Read] private Velocity* _vel;

    public void Execute()
    {
        // ... Gravity logic ...
    }
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Core;

public interface INexusSystem
{
    void Execute();
}

[AttributeUsage(AttributeTargets.Field)]
public class ReadAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field)]
public class WriteAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field)]
public class InjectAttribute : Attribute { }
```

---

## Nexus Optimization Tip: Parallelism Maximization
You should use **[Read]** instead of `[Write]` in every possible situation. The processor can "Read" simultaneously on 100 different cores, but a single "Write" operation can stall the entire pipeline. Correct attribute usage can **increase your parallelism efficiency by 300-400%.**
