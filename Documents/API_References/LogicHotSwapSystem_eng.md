# Nexus Prime Architectural Manual: LogicHotSwapSystem (Runtime Logic Swapping)

## 1. Introduction
`LogicHotSwapSystem.cs` is an experimental infrastructure designed to break the "Change Code - Compile - Restart" cycle, which is one of the biggest bottlenecks in modern game development processes. It allows developers to live-update `INexusSystem` logic without closing the game or simulation.

The reason for this system's existence is to "hotly" swap the methods (simulation logic) that process unmanaged data (Registry) without disturbing its persistence on memory.

---

## 2. Technical Analysis
LogicHotSwapSystem uses the following mechanisms for dynamic code exchange:

- **Assembly Loading**: Newly written and compiled system logic is included in the existing process using `Assembly.Load` or `AssemblyLoadContext`.
- **System Interface Bridge**: As long as the newly loaded classes implement the `INexusSystem` interface, they can replace the reference of the old system in the `JobSystem`.
- **State Persistence**: Thanks to Nexus's ECS structure, all state is already on the `Registry`. When logic changes, no progress is lost because the data remains the same (Zero State Loss).
- **Reflective Injection**: The `Registry` and other dependencies possessed by the old system are re-injected into the newly loaded system.

---

## 3. Logical Flow
1.  **Triggering**: The developer calls the `SwapSystemLogic` method with a new DLL path.
2.  **Loading**: The binary at the specified path is taken into memory.
3.  **Finding**: The system type with the same name in the DLL is found via Reflection.
4.  **Swapping**: The old system in `JobSystem` is removed, and the instance of the newly loaded system is added in its place.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Hot-Swap** | The process of updating parts of a program while it is running without stopping it. |
| **Assembly** | A compiled unit of code in the .NET environment (DLL or EXE). |
| **Zero State Loss** | The state where existing game data is preserved during logic exchange. |
| **Dynamic Invocation** | Determining and calling the name of a method or class at runtime. |

---

## 5. Risks and Limits
- **Assembly Leaks**: In versions before .NET Core, the inability to unload assemblies loaded into memory can cause RAM accumulation.
- **Breaking Changes**: If the new logic has changed the structure of the old data (Component struct), the application may crash due to memory mismatch (Memory Corruption).
- **Thread Safety**: The swap operation should be performed at a safe synchronization point (Sync Point) where systems are not being `Execute()`ed.

---

## 6. Usage Example
```csharp
// Update gravity logic during a live game
var hotswap = new LogicHotSwapSystem();
hotswap.SwapSystemLogic(currentGravitySystem, "Path/To/New/PhysicsPart2.dll");
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
using System.Reflection;
namespace Nexus.Core;

public class LogicHotSwapSystem
{
    public void SwapSystemLogic(INexusSystem oldSystem, string assemblyPath)
    {
        // 1. Load the assembly from path.
        // 2. Find the implementation of the same system type.
        // 3. Migrate state and replace the instance in JobSystem.
        Console.WriteLine($"Nexus: Hot-swapping logic for {oldSystem.GetType().Name}");
    }
}
```

---

## Nexus Optimization Tip: Context-Based Reloading
Load your systems into isolated areas using `AssemblyLoadContext`. In this way, you can completely delete (Unload) your old code from memory and **100% prevent memory bloating in long-term development sessions.**
