# Nexus Prime Architectural Manual: BridgeHub (Central Bridge Hub)

## 1. Introduction
`BridgeHub.cs` is the "Central Exchange" that manages all data traffic between Nexus Prime's unmanaged simulation world and external engines (Unity, Unreal, etc.). It collects synchronization logics defined for different component types in one place and orchestrates the execution of these logics in the correct order every frame (frame).

The reason for this hub's existence is to prevent each system from performing its own synchronization and leading to confusion (Race Condition) and to standardize the data flow by dividing it into two main disciplines: "Pull" (from Engine to Nexus) and "Push" (from Nexus to Engine).

---

## 2. Technical Analysis
BridgeHub manages data flow via these two main channels:

- **Pull (Engine -> Nexus)**: Called at the start of the frame (Start of Frame). Pulls input or physics changes from the Unity side to the unmanaged `Registry`.
- **Push (Nexus -> Engine)**: Called at the end of the frame (End of Frame). Pushes unmanaged data changed as a result of simulation (e.g., positions resulting from AI) to Unity visual objects.
- **Action Decoupling**: Stores synchronization logics in `Action` lists, providing a generic execution without knowing the internal structure of the Registry.
- **Dirty Check Integration**: Offers optional "Dirty Check" (Dirty Check) support for each component registered, ensuring only changed data is processed.

---

## 3. Logical Flow
1.  **Registration (Register)**: How which components will be synchronized is reported to the Hub as the application starts.
2.  **Input Processing (PullAll)**: All updates in the Unity world are transferred to Nexus in a single pass.
3.  **Simulation**: Nexus unmanaged systems process the data.
4.  **Visualization (PushAll)**: Changed results are sprayed back into the Unity world (Transform, VFX, etc.).

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Push/Pull Discipline** | Rule set ensuring data flow is unidirectional and performed in specific time periods (Phase). |
| **Sync Logic Registration** | Pre-defining how a component type will be copied to the Hub. |
| **Orchestration** | Running multiple independent systems in order within a certain harmony. |

---

## 5. Usage Example
```csharp
var hub = new BridgeHub(registry);

// Register logic to push position data from Nexus to Unity
hub.Register<Vector3>(
    push: (id, ptr) => NexusSyncManager.SyncEntity(registry, id, GetGO(id)),
    pull: (id, ptr) => *ptr = GetGO(id).transform.position
);

// Frame Loop
void Update() {
    hub.PullAll(); // Unity -> Nexus
    // ... Simulation ...
    hub.PushAll(); // Nexus -> Unity
}
```

---

## 6. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Bridge;

public class BridgeHub
{
    private readonly List<Action> _pushActions = new();
    private readonly List<Action> _pullActions = new();

    public void Register<T>(PushDelegate push, PullDelegate pull) where T : unmanaged {
        if (push != null) _pushActions.Add(() => NexusBridge<T>.Push(_registry, push));
        if (pull != null) _pullActions.Add(() => NexusBridge<T>.Pull(_registry, pull));
    }

    public void PullAll() { foreach (var a in _pullActions) a(); }
    public void PushAll() { foreach (var a in _pushActions) a(); }
}
```

---

## Nexus Optimization Tip: Batch Registration
Register frequently used components (Transform, Health, etc.) with the Hub via a single bulk registration system (Batch). This prepares the infrastructure needed to process larger memory blocks at once, instead of calling hundreds of `Action` delegates individually every frame.
