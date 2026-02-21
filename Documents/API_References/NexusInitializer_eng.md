# Nexus Prime Architectural Manual: NexusInitializer (Engine Initializer)

## 1. Introduction
`NexusInitializer.cs` is the "Command Center" and main entry point of the Nexus Prime framework. It ensures the automatic startup of the ECS world (`Registry`), the thread manager (`JobSystem`), and the memory backup engine (`SnapshotManager`) within Unity scenes.

The reason for this initializer's existence is to offer "One-Click" integration to Unity developers by simplifying the complex structures of the unmanaged engine and to align the engine's lifecycle (Lifecycle) with scene transitions.

---

## 2. Technical Analysis
NexusInitializer assumes the following critical roles for engine management:

- **Core Bootstrapping**: Creates Registry, JobSystem, and SnapshotManager instances at the `Awake` phase.
- **Orchestration**: Ensures unmanaged systems run every frame (frame) by calling `JobSystem.Execute()` in the `Update` loop.
- **Health Monitoring**: Periodically (default 60 frames) audits unmanaged memory status using `NexusIntegrityChecker` and reports errors to the Unity console.
- **Resource Management**: Returns unmanaged memory (Dispose) with `OnDestroy` when the scene closes or the object is deleted.

---

## 3. Logical Flow
1.  **Initialization**: The heart of the engine, `Registry` and `JobSystem`, is allocated in RAM when the scene is loaded.
2.  **Execution**: Every frame, systems process unmanaged data in the determined order.
3.  **Audit**: Memory integrity and alignment (Alignment) checks are performed in the background.
4.  **Cleanup**: All unmanaged resources are manually cleaned when the application closes.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Bootstrapping** | Sequential preparation of basic components required for software to run. |
| **Runtime Integrity** | Constant monitoring of whether memory structures are corrupted while the application is running. |
| **Lifecycle Hook** | Unity's automatically called lifecycle methods such as `Awake`, `Update`, `OnDestroy`. |
| **Engine Configuration** | Adjusting performance parameters of the engine such as maximum history frame count. |

---

## 5. Risks and Limits
- **Global Dependency**: Multiple `NexusInitializer`s must not be in the scene; otherwise, conflicting Registries occur.
- **Disposal Negligence**: Massive unmanaged memory leaks can occur if the object is not destroyed in the scene or if `OnDestroy` does not work correctly.

---

## 6. Usage Example
```csharp
// Create an object in the scene and add this script.
// JobSystem and Registry will be ready automatically.

void Start() {
    var registry = FindObjectOfType<NexusInitializer>().Registry;
    registry.Create(); // ECS world is ready now!
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity;

public class NexusInitializer : MonoBehaviour
{
    public int MaxHistoryFrames = 100;
    private Registry _registry;
    private JobSystem _jobSystem;

    private void Awake() {
        _registry = new Registry();
        _jobSystem = new JobSystem(_registry);
    }

    private void Update() {
        _jobSystem.Execute();
    }

    private void OnDestroy() {
        _registry?.Dispose();
    }
}
```

---

## Nexus Optimization Tip: Integrity Performance
Keep the `PerformRuntimeIntegrityChecks` setting on during development (Development) but turn it off in the "Final Release" version. This **provides a small performance gain by zeroing out the audit cost performed every 60 frames.**
