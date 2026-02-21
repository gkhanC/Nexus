# Nexus Prime Architectural Manual: NexusProgression (Level and Progress System)

## 1. Introduction
`NexusProgression.cs` is an unmanaged data structure designed for RPG and progression-based game mechanics. Inherited from the HypeFire architecture, this structure acts as an optimized "Accumulator" (Accumulator) to track situations such as experience points (XP), skill development, or quest progress.

The reason for this structure's existence is to keep the progression status of each entity (NPC, Player, etc.) directly in the memory block of the component (Component), instead of via managed objects, allowing the level-up control of thousands of units at the same time with zero GC cost.

---

## 2. Technical Analysis
NexusProgression manages the following fields for data integrity:

- **CurrentProgress**: The current accumulated amount (e.g., 250 XP).
- **Goal**: The target amount required to move to the next level (e.g., 1000 XP).
- **FillRatio**: Calculates the ratio of the current progress to the target (0.0 - 1.0) for the UI layer.
- **Auto-Leveling Logic**: Thanks to the `while` loop within the `Add` method, consecutive level ups (Multiple Level-ups) are correctly calculated in case a large amount of points comes at once (e.g., 5000 XP).

---

## 3. Logical Flow
1.  **Input**: New progress points enter the system via the `Add(amount)` method.
2.  **Control**: Whether the current amount exceeds the target (Goal) is checked.
3.  **Level Increase**: As long as the target is exceeded, `Level` is increased and the excess amount is preserved as the starting points of the next level within `CurrentProgress`.
4.  **Reset**: Progress is transferred by subtracting from the target with a "Modulo"-like logic.

---

## 4. Usage Example
```csharp
public struct PlayerLevel : INexusComponent {
    public NexusProgression XP;
}

// Usage
ref var xp = ref registry.Get<PlayerLevel>(e).XP;
xp.Goal = 1000;
xp.Add(2500); // Increases Level by 2, 500 XP remains.
float uiFill = xp.FillRatio; // 0.5f (500/1000)
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Data;

public struct NexusProgression
{
    public float CurrentProgress;
    public float Goal;
    public int Level;

    public float FillRatio => Goal > 0 ? CurrentProgress / Goal : 0;

    public void Add(float amount)
    {
        CurrentProgress += amount;
        while (CurrentProgress >= Goal && Goal > 0)
        {
            CurrentProgress -= Goal;
            Level++;
        }
    }
}
```

---

## Nexus Optimization Tip: Carry-over Precision
Using a `while` loop prevents experience points from being "lost" (Lost carry-over). Using this structure instead of a single `if` control **eliminates level calculation errors in high-point rewards by 100%.**
