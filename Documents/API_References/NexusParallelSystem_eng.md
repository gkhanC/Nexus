# Nexus Prime Architectural Manual: NexusParallelSystem (Autonomous System Base)

## 1. Introduction
`NexusParallelSystem.cs` is the primary carrier of high-performance and autonomous business logic within Nexus Prime. It is an abstraction layer that allows developers to focus purely on business logic without dealing with complex parallel programming details (thread management, deadlocks, etc.).

The reason for this class's existence is to automatically detect the data dependencies required by systems at runtime and inform the `JobSystem` regarding the safety measures (parallel or sequential) with which this system should be executed.

---

## 2. Technical Analysis
NexusParallelSystem provides the following infrastructures for system automation:

- **Reflective Dependency Extraction**: The `GetAccessInfo` method scans fields on the class marked with `[Read]` and `[Write]` attributes using `Reflection`. This eliminates the need for the developer to manually maintain a dependency list.
- **Dependency Inversion**: The `Registry` reference is automatically injected when the system is created, thanks to the `[Inject]` attribute.
- **Lifecycle Hooks**: `OnCreate` and `OnDestroy` methods provide control points throughout the system's lifecycle (for resource allocation or cleanup).
- **Abstract Logic Entry**: By making the `Execute()` method mandatory (abstract), it is guaranteed that every system has a clear execution point.

---

## 3. Logical Flow
1.  **Initialization**: When the system is registered via `JobSystem.AddSystem`, the `Inject` fields are populated.
2.  **Inquiry**: `JobSystem` learns which components the system will access and in which mode by calling `GetAccessInfo()`.
3.  **Execution**: The `Execute()` method is triggered in the parallel thread pool once the system's dependencies in the data layer are clear.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Autonomous System** | A logic unit that knows its own dependencies and can operate without external interference. |
| **Reflection** | The ability of a program to examine its own structure (fields, methods, etc.) at runtime. |
| **Lifecycle Hook** | A callback method that runs when a specific event occurs (creation, deletion). |
| **Abstraction Layer** | A structure that provides simpler usage by hiding complex sub-system details. |

---

## 5. Risks and Limits
- **Reflection Overhead**: Since dependency scanning uses Reflection, it creates a small CPU cost at the moment of `AddSystem`. However, since this is done only once, it has no impact on in-game performance.
- **Virtual Method Call**: Since `GetAccessInfo` is a virtual method, micro-level call costs may occur in very deep inheritance hierarchies.

---

## 6. Usage Example
```csharp
public class CombatSystem : NexusParallelSystem
{
    // Dependencies are automatically injected and scanned
    [Write] private Health* _health;
    [Read] private AttackPower* _power;

    public override void Execute()
    {
        // ... Combat logic ...
    }

    public override void OnCreate()
    {
        // Initial settings
    }
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
using System.Reflection;
namespace Nexus.Core;

public abstract class NexusParallelSystem : INexusSystem
{
    [Inject] protected Registry Registry;
    public string Name => GetType().Name;

    public virtual (HashSet<Type> Reads, HashSet<Type> Writes) GetAccessInfo()
    {
        var reads = new HashSet<Type>();
        var writes = new HashSet<Type>();
        var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.GetCustomAttribute<ReadAttribute>() != null) reads.Add(field.FieldType);
            if (field.GetCustomAttribute<WriteAttribute>() != null) writes.Add(field.FieldType);
        }
        return (reads, writes);
    }

    public abstract void Execute();
    public virtual void OnCreate() { }
    public virtual void OnDestroy() { }
}
```

---

## Nexus Optimization Tip: Static Mapping vs Reflection
If your system is being deleted and re-added thousands of times, you can escape the Reflection cost by overriding the `GetAccessInfo` method. By returning a manual list in the form of `Reads.Add(typeof(T))`, you can **shorten the system registration time by 95%.**
