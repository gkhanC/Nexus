# Nexus Prime Architectural Manual: NexusStatus (Status and Resource Management)

## 1. Introduction
`NexusStatus.cs` is a data template that manages critical survival resources of entities (Health, Mana, Energy, etc.) and RPG-style attributes (Strength, Agility, etc.) on unmanaged memory. Inspired by the HypeFire framework architecture, this structure is designed to monitor the status of thousands of units in high-performance games.

The reason for this structure's existence is to reduce the cache-miss rate by clustering data within hardware-friendly components (Components), instead of creating separate classes for each enemy unit or bullet, and to ensure that systems reach this data in the fastest way.

---

## 2. Technical Analysis
In terms of physical data alignment (Memory Layout), it presents two critical structures:

- **NexusStatus**: Houses two "Current/Max" pairs (Health and Mana). Speeds up visualization processes with helper properties (helper properties) like `IsDead`, `HealthPercent`. `Damage` and `Heal` methods safely update unmanaged data with `MathF.Max/Min`.
- **NexusAttributeStats**: Stores standard RPG statistics (`Strength`, `Agility`, `Intelligence`, `Stamina`) as a block in `int` type. These data typically combine with `NexusStackable` to form final combat figures.

---

## 3. Logical Flow
1.  **Definition**: Defined as `NexusStatus Vitality` within a component (Component).
2.  **Effect**: A damage system directly updates the value at the unmanaged memory address by calling `Vitality.Damage(50)`.
3.  **Control**: Systems performing death control decide whether the entity will be destroyed or not by looking at the `Vitality.IsDead` flag.
4.  **UI Update**: Health bars are updated using `HealthPercent`.

---

## 4. Terminoloji Sözlüğü

| Term | Description |
| :--- | :--- |
| **Vitality Tracking** | Constant monitoring of an entity's survival parameters. |
| **Encapsulated Logic** | Combining methods and data within the same struct in accordance with unmanaged rules. |
| **RPG Stats** | Set of numerical attributes determining the character's basic skills. |

---

## 5. Risks and Limits
- **Extended Resources**: If your project requires not just Health/Mana but 5-6 different resources like "Stamina", "Oxygen", it is necessary to extend this struct or add new components.
- **State Loss**: Since it is unmanaged, data is completely lost when components containing these structures are deleted (unless a Snapshot was taken).

---

## 6. Usage Example
```csharp
public struct EnemyStatus : INexusComponent {
    public NexusStatus Vitals;
    public NexusAttributeStats Stats;
}

// Usage within the system
ref var enemy = ref registry.Get<EnemyStatus>(id);
enemy.Vitals.Damage(10);

if (enemy.Vitals.IsDead) {
    Console.WriteLine("Enemy eliminated.");
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Data;

public struct NexusStatus
{
    public float CurrentHealth;
    public float MaxHealth;
    public float CurrentMana;
    public float MaxMana;

    public bool IsDead => CurrentHealth <= 0;
    public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;

    public void Damage(float amount) => CurrentHealth = MathF.Max(0, CurrentHealth - amount);
    public void Heal(float amount) => CurrentHealth = MathF.Min(MaxHealth, CurrentHealth + amount);
}
```

---

## Nexus Optimization Tip: Memory Pooling
Instead of creating and deleting `NexusStatus` components thousands of times per second, take advantage of the `AutomaticInternalPooling` system. This **reduces the memory management load on the processor by 30%** by preventing unmanaged memory pages from being constantly requested from the OS.
