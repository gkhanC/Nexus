# Nexus Prime Architectural Manual: INexusRegistry (Entity & Component Management Contract)

## 1. Introduction
`INexusRegistry.cs` is the central management interface at the heart of the Nexus Prime ECS architecture. It defines the rule set that controls the lifecycle of all entities and determines how components will be stored in SparseSet repositories.

The reason for this interface's existence is to provide a consistent, high-performance, and hardware-friendly API to the rest of the system (Systems, Bridges, UI). By implementing this contract, the `Registry` class guarantees the placement of data on raw memory (RAM) and its access speed.

---

## 2. Technical Analysis
INexusRegistry mandates the following critical functions for ECS operations:

- **Entity Lifecycle**: Creation and deletion of entities are managed with `Create()` and `Destroy()` methods. By using a "Versioned ID" structure here, it is prevented that a new entity coming in place of a deleted one mixes with old data (dangling pointer).
- **Pointer-Based Access**: The `Get<T>` and `Add<T>` methods return a raw pointer (`T*`), which is the peak of performance, instead of a safe reference to the component. This reduces the C# copying cost to zero.
- **Type-Erased Storage**: Through the `ComponentSets` property, non-generic (`ISparseSet`) interface access is provided to all specialized component repositories. This makes it easy to perform batch processing across the system.
- **O(1) Access**: All Has, Get, Add, and Remove operations occur in constant time (O(1)) thanks to SparseSet math.

---

## 3. Logical Flow
1.  **Entity Creation**: When `Create()` is called, the `Registry` allocates an unmanaged index and increments the version number.
2.  **Component Addition**: When `Add<T>` is called, the corresponding `SparseSet<T>` is found and unmanaged memory area is allocated through the entity's index.
3.  **Validation**: Whether an `EntityId` still represents a valid object or a deleted waste is checked with `IsValid()`.
4.  **Cleaning**: When `Dispose()` is called, all SparseSets and unmanaged memory areas connected to the Registry are cleared from RAM.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Registry** | Central register of entities and components attached to them. |
| **Dangling Pointer** | A dangerous reference pointing to an invalid memory address. |
| **Versioned ID** | A counter tracking how many times an index has been recycled. |
| **Typed Storage** | Separate, optimized memory blocks for each component type. |

---

## 5. Risks and Limits
- **Manual Disposal**: `INexusRegistry` is an `IDisposable` object. If `Dispose()` is not called at the end of the program, an unmanaged memory leak occurs.
- **Pointer Safety**: When operating on data obtained via `T*`, this pointer remains invalid if the entity is deleted. Therefore, `IsValid` control is critical in intensive operations.

---

## 6. Usage Example
```csharp
public unsafe void ProcessCombat(INexusRegistry registry, EntityId player, EntityId enemy)
{
    if (registry.IsValid(player) && registry.Has<Health>(player))
    {
        Health* hp = registry.Get<Health>(player);
        hp->Amount -= 10;
        
        if (hp->Amount <= 0)
            registry.Destroy(player);
    }
}
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Core;

public unsafe interface INexusRegistry : IDisposable
{
    EntityId Create();
    void Destroy(EntityId entity);
    bool IsValid(EntityId entity);
    
    T* Add<T>(EntityId entity, T component = default) where T : unmanaged;
    T* Get<T>(EntityId entity) where T : unmanaged;
    bool Has<T>(EntityId entity) where T : unmanaged;
    void Remove<T>(EntityId entity) where T : unmanaged;
    
    SparseSet<T> GetSet<T>() where T : unmanaged;
    IEnumerable<ISparseSet> ComponentSets { get; }
}
```

---

## Nexus Optimization Tip: Handle Reuse Safety
The 32-bit version field within the `EntityId` structure allows an ID to be recycled **4 billion times**. Even if billions of entities are deleted in your game, this prevents old systems from accidentally writing data over new entities (Corruption) at the hardware level.
