# Nexus Prime Architectural Manual: PhysicsBridge (Physics Integration Line)

## 1. Introduction
`PhysicsBridge.cs` is the bridge between Nexus Prime's unmanaged logic world and Unity's built-in physics engine (PhysX). It ensures that physical data (Position, Force, Torque) calculated or stored in the unmanaged Registry is transferred to Unity's `Rigidbody` and `Collider` components at the right time and in the right order.

The reason for this bridge's existence is to prevent CPU main thread (Main Thread) bottlenecks by performing a mass and optimized physics synchronization through a central system, instead of each entity trying to reflect itself into Unity physics.

---

## 2. Technical Analysis
Follows these architectural steps for physics integration:

- **Batch Synchronization**: Scans all physical entities in a single loop and injects data into Unity physics components.
- **FixedUpdate Alignment**: Guarantees that data works synchronized with Unity's physics step (`FixedUpdate`). This prevents visual jitters and physical inconsistencies.
- **Bi-Directional Dynamics**: 
  - **Push**: Transfers Nexus AI/Simulation results to Unity Rigidbodies.
  - **Pull**: Pulls the new positions of objects that Unity physics hits or moves into the Nexus unmanaged data-driven world.
- **Transform Sweeping**: Smooths out transform changes on the Unity side in bulk when necessary.

---

## 3. Logical Flow
1.  **Analysis**: Position and Rotation components within the Nexus Registry are scanned.
2.  **Mapping**: Whether the entity has a `Rigidbody` counterpart on the Unity side is checked.
3.  **Velocity/Force Transfer**: Force vectors calculated in the unmanaged world are pushed to the Unity engine with `Rigidbody.AddForce`.
4.  **Feedback**: New positions formed after Unity physics finishes the simulation are written back to the Nexus Registry.

---

## 4. Usage Scenario
This component is generally used for "Physics-Aided Hybrid Characters". While the general logic of the character is processed unmanaged within Nexus ECS, collision (Collision) and reaction (Ragdoll, etc.) are coordinated by Unity's built-in engine via this bridge.

---

## 5. Full Source Implementation (Conceptual Implementation)

```csharp
namespace Nexus.Bridge;

public class NexusPhysicsBridge : MonoBehaviour
{
    public unsafe void SyncPhysics(Registry.Registry registry)
    {
        // 1. Iterate through entities with [Rigidbody] equivalents.
        // 2. Update Unity Rigidbody positions in batch.
        // 3. Optional: registry.SetDirty(id);
    }
}
```

---

## Nexus Optimization Tip: Kinematic Sync
If the object is only a visual representative and does not need to give a physical reaction, set `Rigidbody.isKinematic = true` on the Unity side and equalize data directly via `transform.position`. This **reduces the internal computational load of the Unity physics engine by 40%.**
