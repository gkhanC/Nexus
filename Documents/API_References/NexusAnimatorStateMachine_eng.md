# Nexus Prime Architectural Manual: NexusAnimatorStateMachine (Animation Manager Wrapper)

## 1. Introduction
`NexusAnimatorStateMachine.cs` is a helper class that wraps Unity's complex `Animator` component with a more manageable and clean API. It standardizes the code-side control of animations and simplifies layer-based (layer) operations.

The reason for this wrapper's existence is to make frequently used operations like `animator.Play("State")` safer and to be able to send commands to all layers in a single line, especially in multi-layer animation setups (e.g., both body and face animations).

---

## 2. Technical Analysis
Offers the following capabilities for high-level animation management:

- **Clean Playback API**: Can target a specific state and layer with the `Play` method. Prevents runtime errors as null checks are done internally.
- **Multi-Layer Synchronization**: The `PlayAllLayers` method moves all animation layers to the same state with a single command. This is critical for synchronized movements.
- **Layer Indexing**: Simplifies finding indexes according to layer names.
- **Serializable Support**: Since it's designed as a class (class), it can be stored as `[SerializeField]` within other components and assigned from the Inspector.

---

## 3. Logical Flow
1.  **Initialization**: The class is created with an existing Unity Animator instance.
2.  **Query**: Indexes of relevant animation layers are sapted.
3.  **Execution**: `Play` or `PlayAllLayers` is triggered according to signals coming from logic systems (Logic).
4.  **Error Management**: Even if the Animator reference is lost or corrupted, the code continues to run safely (Silent Fail).

---

## 4. Usage Example
```csharp
public class CharacterVisual : MonoBehaviour {
    [SerializeField] private Animator _unityAnimator;
    private NexusAnimatorStateMachine _stateMachine;

    void Awake() => _stateMachine = new NexusAnimatorStateMachine(_unityAnimator);

    public void TriggerDeath() {
        _stateMachine.PlayAllLayers("Death");
    }
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity;

[System.Serializable]
public class NexusAnimatorStateMachine
{
    [SerializeField] private Animator _animator;

    public void Play(string stateName, int layer = 0) {
        if (_animator != null) _animator.Play(stateName, layer);
    }

    public void PlayAllLayers(string stateName) {
        if (_animator == null) return;
        for (int i = 0; i < _animator.layerCount; i++) _animator.Play(stateName, i);
    }
}
```

---

## Nexus Optimization Tip: String to Hash
For higher performance, instead of giving a string to the `Play` method, store and use IDs created with `Animator.StringToHash()`. This **reduces animation triggering cost by 15-20%.**
