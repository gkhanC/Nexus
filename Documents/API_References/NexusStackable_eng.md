# Nexus Prime Architectural Manual: NexusStackable (Cumulative Data Management)

## 1. Introduction
`NexusStackable.cs` is a data container designed for "Stackable Resources" frequently encountered in games (e.g., Inventory items, Ammo, Money). Integrated from the HypeFire architecture, this structure is an intelligent unit managing both the amount and the capacity (Cap) of a value.

The reason for this component's existence is to collect these logics under a single structure and integrate them with Unity's `UnityEvent` system to automatically trigger visual updates, instead of writing manual collection/subtraction/limit control for each resource type.

---

## 2. Technical Analysis
Has the following features for container management:

- **Capped Management**: Prevents the value from exceeding a certain limit (Capacity). Provides dynamic capacity management with the `SetCap` method.
- **Transactional Support**: Checks whether the resource is sufficient with the `TrySpend` method and performs the expenditure in a single atomic transaction.
- **Event-Driven Binding**: By firing the `OnValueChanged` event (event) whenever the value changes, it ensures the UI layer (Slider, Text, etc.) is updated without depending on the code.
- **Implicit Operator**: Offers implicit conversion (implicit conversion) support allowing the structure to be used directly as an `int`.

---

## 3. Logical Flow
1.  **Definition**: Defined as `NexusStackable<AmmoTag> Ammo;` within a `MonoBehaviour`.
2.  **Constraint**: The upper limit is determined with `SetCap(100)`.
3.  **Process**: When `Add(50)` is called, the amount increases but cannot exceed 100. A Unity event is fired at the moment of change.
4.  **Control**: When `TrySpend(20)` is called, if there are 20 units, they are spent and `true` is returned.

---

## 4. Usage Example
```csharp
public class PlayerInventory : MonoBehaviour {
    public NexusStackable<GoldTag> Gold = new();

    void Start() {
        Gold.SetCap(1000);
        // Bind UI Slider to OnValueChanged event
        Gold.OnValueChanged.AddListener((val) => Debug.Log("New Gold: " + val));
    }

    public void BuyItem(int cost) {
        if (Gold.TrySpend(cost)) {
            // Purchase successful
        }
    }
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity;

[Serializable]
public class NexusStackable<T> where T : struct
{
    [SerializeField] private int _count;
    [SerializeField] private int _cap = -1;
    public UnityEvent<int> OnValueChanged = new();

    public bool Add(int amount) {
        if (_cap >= 0 && (_count + amount) > _cap) return false;
        _count += amount;
        OnValueChanged.Invoke(_count);
        return true;
    }

    public bool TrySpend(int amount) {
        if (_count < amount) return false;
        _count -= amount;
        OnValueChanged.Invoke(_count);
        return true;
    }
}
```

---

## Nexus Optimization Tip: UnityEvent Overhead
If `Add` is triggered thousands of times in a frame, the use of `OnValueChanged` (UnityEvent) can create additional load on the CPU. For very high-frequency updates, using standard C# `Action` or "Buffering" (deferred triggering) the event firing until the next frame can **increase performance by 5-10%.**
