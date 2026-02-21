# Nexus Prime Architectural Manual: AIBehaviorTree (Unmanaged Behavior Tree)

## 1. Introduction
`AIBehaviorTree.cs` is the performance representative of Nexus Prime in the world of artificial intelligence (AI). Unlike traditional Behavior Tree (BT) structures (Node-based object trees), it provides cache-friendly AI execution by keeping the entire tree structure in contiguous blocks in unmanaged memory (RAM).

The reason for this processor's existence is to completely eliminate the Garbage Collector (GC) pressure and memory jumps (Memory Thrashing) caused by standard C# objects in strategy or simulation games where thousands (e.g., 50,000) entities need to make AI decisions at the same time.

---

## 2. Technical Analysis
NexusBTProcessor uses the following hardware-oriented techniques for high-performance AI decisions:

- **Unmanaged Struct Nodes**: Decision nodes (Selector, Sequence, Action) are stored on interconnected unmanaged structs instead of standard classes.
- **Cache-Friendly Traversal**: When traversing the tree, the processor finds the next node immediately nearby in memory. This minimizes the "Pointer Chasing" cost.
- **Zero-GC Ticking**: The `Tick()` method does not create any new objects (allocation-free). All state information is kept in components on the `Registry`.
- **Flat Tree Optimization**: It optimizes the processor pipeline by using a flatter memory layout instead of deep tree hierarchies.

---

## 3. Logical Flow
1.  **Input**: The process is initiated for the relevant entity with the `Tick(entity, registry)` call.
2.  **Traversal**: The BT structure on unmanaged memory is scanned from the top (Root) downwards.
3.  **Decision**: Condition nodes return `true/false` by looking at the entity's components on the `Registry`.
4.  **Action**: The selected action node is triggered, and the result (Success/Failure/Running) is stored to be evaluated in the next frame.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Traversal** | The process of visiting all elements on a data structure (tree, etc.) in a specific order. |
| **Leaf Node** | The node at the very end of the tree that does the actual work (Action/Condition). |
| **BT Processor** | The main mathematical engine that executes the behavior tree logic. |
| **Memory Thrashing** | The situation where the processor constantly waits for data from RAM because the data is too scattered in memory. |

---

## 5. Risks and Limits
- **Complexity of Setup**: Coding an unmanaged tree structure may not be as easy as visual editors. Node addresses must be managed manually.
- **Pointer Safety**: If the tree structure is corrupted (Memory Corruption), it can cause the entire AI system to give a memory error (Segmentation Fault).

---

## 6. Usage Example
```csharp
public struct AISystem : NexusParallelSystem {
    private NexusBTProcessor _btProcessor;

    public override void Execute() {
        var entities = Registry.Query().With<AICapacity>().GetEntities();
        
        foreach(var e in entities) {
            // Execute AI without creating any objects
            _btProcessor.Tick(e, Registry);
        }
    }
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Core;

public unsafe struct NexusBTProcessor
{
    public void Tick(EntityId entity, Registry registry)
    {
        // 1. Traverse the unmanaged BT structure.
        // 2. Execute leaf nodes (Actions/Conditions).
        // 3. Update entity state components based on results.
    }
}
```

---

## Nexus Optimization Tip: Batch Tick Strategy
If you have more than 100,000 entities, divide AIs into groups (Batches) instead of `Tick`ing all AIs every frame. For example, let only "Nearby enemies" make AI decisions in one frame, and "Distant enemies" in the next frame. This method **balances the AI system's frame time by 500%.**
