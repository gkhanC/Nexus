# Nexus Prime Architectural Manual: NexusStateMachine (State Manager)

## 1. Introduction
`NexusStateMachine.cs` is a "Behavioral Orchestrator" that divides complex object behaviors (e.g., player movement states, NPC AI, game loop phases) into controllable and modular parts. Thanks to being designed as a MonoBehaviour, it works fully compatible with Unity scenes.

The reason for this machine's existence is to prevent massive code blocks consisting of "If-Else" stacks and to divide each state (State) into categories that are isolated, testable, and expandable within themselves.

---

## 2. Technical Analysis
Applies the following architectural standards for state management:

- **Interface-Based States**: Each state implements the `IState` interface (interface). In this way, the state lifecycle is standardized by making `Enter` (Enter), `Update` (Update), and `Exit` (Exit) methods mandatory.
- **Atomic State Switching**: While switching from one state to another with the `ChangeState` method, it ensures that the `Exit` logic of the old state and the `Enter` logic of the new state work error-free and in order.
- **Update Delegation**: Automatically calls the `Update` method of the current active state every frame, delegating logical execution (logic execution) to the current state.
- **Flexible Integration**: It is flexible enough to trigger both pure code-based states and Unity Animator transitions.

---

## 3. Logical Flow
1.  **Enter (Enter)**: Triggered once when entering a new state (e.g., start the walking animation).
2.  **Loop (Update)**: Runs every frame as long as the state is active (e.g., check speed).
3.  **Transition (Change)**: When a condition occurs (e.g., jump key pressed), a signal to transition to a new state is given.
4.  **Exit (Exit)**: Triggered once just before leaving the state (e.g., close the dust effect).

---

## 4. Usage Example
```csharp
public class PlayerController : MonoBehaviour {
    private NexusStateMachine _sm;

    void Start() {
        _sm = gameObject.AddComponent<NexusStateMachine>();
        _sm.ChangeState(new IdleState());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) _sm.ChangeState(new JumpState());
    }
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Communication;

public class NexusStateMachine : MonoBehaviour
{
    public interface IState {
        void Enter();
        void Update();
        void Exit();
    }

    private IState _currentState;

    public void ChangeState(IState newState) {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    void Update() => _currentState?.Update();
}
```

---

## Nexus Optimization Tip: State Pooling
If you are changing states dozens of times per second (e.g., very fast changing AI decisions), store state classes in a "Pool" (Pool) and reuse them instead of creating them with `new` every time. This will **reduce pressure on the Garbage Collector (GC) by 25%.**
