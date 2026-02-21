# API Reference: NexusObjectPool (Object Pooling)

## Introduction
`NexusObjectPool.cs` is a high-performance pooling (pooling) system developed as an alternative to Unity's expensive `Instantiate` and `Destroy` operations. Instead of deleting entities that are frequently created and destroyed during the game (bullets, effects, etc.) from memory, it deactivates them and keeps them waiting in a queue. This prevents frame rate fluctuations (lag spikes) by removing the load on the Garbage Collector (GC).

---

## Technical Analysis
The pooling system operates using the following mechanisms:
- **Queue-Based Storage**: Provides O(1) speed access by maintaining a separate `Queue<GameObject>` for each prefab type.
- **INexusPoolable Interface**: Offers a lifecycle interface for objects appearing from or entering the pool so they can reset their states (`OnSpawn`/`OnDespawn`).
- **Dynamic Growth**: If there is no free object in the pool, the system automatically creates a new one (`Instantiate`).
- **Name-Key Mapping**: Organizes different types of objects using prefab names as keys (key).

---

## Logical Flow
1. **Request**: When the `Spawn` method is called, the queue belonging to the relevant prefab is checked.
2. **Re-use**: If an object is in the queue, it is activated, its position is set, and `OnSpawn` is triggered.
3. **Eviction**: When `Despawn` is called, `OnDespawn` is triggered, the object is deactivated, and it returns to the queue it belongs to.
4. **Cleanup**: All pools are released when the scene is loaded or the application is closed.

---

## Terminology Glossary
- **Object Pooling**: The technique of retaining objects for re-use instead of destroying them.
- **Lag Spike**: A delay in the processor's frame production due to a heavy process (e.g., GC).
- **Active/Inactive State**: The visibility and operation status of an object within the Unity hierarchy.
- **Lifecycle Hook**: Code snippets that run during specific stages (creation, destruction) of an object.

---

## Risks and Limits
- **State Reset**: If the object's state (speed, health, visual effects) is not manually reset within `OnDespawn`, it may appear with old data when spawned again.

---

## Usage Example
```csharp
GameObject bullet = NexusObjectPool.Spawn(bulletPrefab, pos, rot);
// ... when bullet is done
NexusObjectPool.Despawn(bullet);
```

---

## Nexus Optimization Tip: Pre-Warming Pools
At the start of critical scenes (on the loading screen), "warm up" the pool by pre-spawning and immediately de-spawning frequently used objects. This **allows you to pre-pay the initial Instantiate cost that might occur during the peak of battle.**

---

## Original Source
[NexusDevelopmentTools.cs Source Code](file:///home/gokhanc/Development/Nexus/Nexus.Unity/Core/NexusDevelopmentTools.cs)
