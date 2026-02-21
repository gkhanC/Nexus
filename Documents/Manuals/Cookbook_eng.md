# Nexus Prime Architectural Manual: Development Cookbook

## 1. Introduction: The ECS Mindset
When developing with Nexus Prime, you must think in a "Data-Oriented" way, not an "Object-Oriented" one. An entity is not an object; it is simply a hook onto which components are attached.

This Cookbook provides ready-made solutions in "Deep-Depth" standards for the scenarios you will most frequently encounter while building your system.

---

## 2. Recipe 1: High-Density Movement System
The most efficient method for moving 10,000 bullets or 5,000 units simultaneously.

### Data Structure
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct Velocity : unmanaged { public float3 Value; }

[StructLayout(LayoutKind.Sequential)]
public struct Position : unmanaged { public float3 Value; }
```

### System Logic
```csharp
public class MovementSystem : NexusParallelSystem
{
    [Read] private float _deltaTime;
    
    public override void Execute()
    {
        // Filter with Query and process in parallel
        var query = registry.Query<Position, Velocity>();
        query.Execute((EntityId id, Position* pos, Velocity* vel) => {
            pos->Value += vel->Value * _deltaTime;
        });
    }
}
```

---

## 3. Recipe 2: State Snapshot & Rewind
To record the game state and go back 5 seconds.

```csharp
// 1. Initialize Snapshot Manager
var snapshotMgr = new SnapshotManager();

// 2. Record every frame (or at specific intervals)
void Update() {
    snapshotMgr.RecordFrame(registry, deltaOnly: true);
}

// 3. When rewind is requested
void OnRewindRequested() {
    var pastFrame = snapshotMgr.History.First.Value;
    snapshotMgr.LoadSnapshot(registry, pastFrame);
}
```

---

## 4. Recipe 3: Unity GameObject Synchronization
Transferring physical data within Nexus to the Unity Gfx (Visual) layer.

```csharp
// BridgeHub registration
bridgeHub.Register<Position>(
    push: (id, nexusPos) => {
        // Nexus -> Unity
        var transform = GetUnityTransform(id);
        transform.position = nexusPos->Value;
    },
    pull: (id, nexusPos) => {
        // Unity -> Nexus (If coming from Unity)
        var transform = GetUnityTransform(id);
        nexusPos->Value = transform.position;
    }
);
```

---

## 5. Advanced Tip: Memory Pooling
Use `EntityCommandBuffer` to prevent costs that occur when adding/removing components:

```csharp
public void OnProjectileHit(EntityId projectile, EntityId target) {
    // Do not destroy immediately, queue it!
    ecb.DestroyEntity(projectile);
    ecb.AddComponent(target, new DamageEffect { Amount = 50 });
}
```

---

## 6. Frequently Asked Questions (Best Practices)
- **Q: When should I use a Query?**
    - A: When you want to batch process all entities that possess a group of components.
- **Q: When should I use Direct Access (registry.Get)?**
    - A: When you want to reach a single entity's data in a specific situation (at O(1) speed).
- **Q: Can I use strings?**
    - A: Only use unmanaged alternatives like `NexusString32/64/128`.

---

**Nexus Prime Engineering Note**: 
In the ECS world, "Early Optimization" is not a mistake; it is a necessity. By following the patterns in this Cookbook, you can build the foundations of your project in a way that will not waste 100 million clock cycles.
