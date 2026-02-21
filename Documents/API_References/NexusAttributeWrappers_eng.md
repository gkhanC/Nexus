# Nexus Prime Architectural Manual: NexusAttributeWrappers (Managed-Unmanaged Data Bridge)

## 1. Introduction
`NexusAttributeWrappers.cs` is a "Serialization Bridge" that connects Nexus Prime's unmanaged data structures to Unity's managed world and specifically to the `Inspector` panel. Unmanaged structs (e.g., `NexusAttribute`) cannot be edited directly by the Unity Inspector; this library removes this obstacle.

The reason for these wrappers' existence is to enable the developer to edit initial data (Health, Range, etc.) using the Unity Editor's comfortable interface, instead of manually coding the unmanaged memory structure.

---

## 2. Technical Analysis
Offers managed wrappers for two main data types:

- **NexusAttributeWrapper**: The managed version of the `NexusAttribute` (`Current/Max`) structure. With the `ToUnmanaged()` method, it converts data entered from the Inspector into a pure struct form that can be written to unmanaged memory addresses.
- **NexusMinMaxWrapper**: Wraps the `NexusMinMax<float>` structure. Allows designers to easily enter "Minimum" and "Maximum" numerical ranges from the Unity interface.
- **Bi-Directional Sync**: Thanks to `FromUnmanaged` methods, it ensures that unmanaged data changed during the game appears on the Inspector again (for Debug purposes).

---

## 3. Logical Flow
1.  **Definition**: Defined as `public NexusAttributeWrapper StartHealth;` within a `MonoBehaviour`.
2.  **Design**: The developer enters values (e.g., 100/100) via the Unity Inspector.
3.  **Conversion**: `ToUnmanaged()` is called when the object is created and data is copied to the unmanaged area within the Nexus Registry.
4.  **Feedback**: If unmanaged data changes, visual values in the Editor can be updated by calling `FromUnmanaged()`.

---

## 4. Usage Example
```csharp
public class CharacterConfig : MonoBehaviour {
    public NexusAttributeWrapper Health;

    public void ApplyToEntity(EntityId id, Registry registry) {
        // Transfer data from Inspector to unmanaged world
        registry.Set(id, Health.ToUnmanaged());
    }
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity;

[Serializable]
public class NexusAttributeWrapper
{
    public float CurrentValue;
    public float MaxValue;

    public NexusAttribute ToUnmanaged() => new NexusAttribute { Current = CurrentValue, Max = MaxValue };
    
    public void FromUnmanaged(NexusAttribute attr) {
        CurrentValue = attr.Current;
        MaxValue = attr.Max;
    }
}
```

---

## Nexus Optimization Tip: One-Way Initialization
For performance, use wrapper classes only for loading "Initial Data" (Initial Data). Trying to write unmanaged data back to the wrapper every frame during the game (Sync-back) **can create unnecessary CPU cost and Garbage (GC), especially in thousands of entities.** Keep bidirectional synchronization active only in debug mode.
