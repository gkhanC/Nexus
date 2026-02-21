# Nexus Prime Architectural Manual: NexusRigidbodyMove (Physics Movement Engine)

## 1. Introduction
`NexusRigidbodyMove.cs` is a "Heavy Duty" movement engine that ensures objects move in accordance with physics rules using Unity's `Rigidbody` component. Refined from the HypeFire architecture.

The reason for this controller's existence is to ensure objects perform a smooth travel, preserving physics effects such as collision detection and friction, instead of just teleporting (teleport) their positions.

---

## 2. Technical Analysis
Uses the following mechanisms for physical movement:

- **MovePosition Integration**: Using the `Rigidbody.MovePosition` method, it ensures the object "flows" physically from one point to another. This is much more stable than changing `transform.position` directly and prevents physics jitters (jitter).
- **Velocity Control**: Can give the object immediate instantaneous speed with the `SetVelocity` method. This is ideal for bullets or launched objects.
- **FixedUpdate Dependency**: Physics calculations are dependent on Unity's `FixedUpdate` loop, which offers a consistent movement speed on all hardware.
- **Required Component**: Reduces error margin by preventing the object from working without physics capability with the `[RequireComponent(typeof(Rigidbody))]` attribute.

---

## 3. Logical Flow
1.  **Preparation**: At the `Awake` moment, the `Rigidbody` reference on the object is cached (Cache).
2.  **Command**: A `Move(direction)` or `SetVelocity(vel)` command comes from outside.
3.  **Physics Simulation**: The command is transmitted to the engine to be processed in Unity's next physics step.
4.  **Result**: The object advances towards the target by bouncing off or pushing the obstacles it hits.

---

## 4. Usage Example
```csharp
public class PlayerInput : MonoBehaviour {
    private NexusRigidbodyMove _mover;

    void Start() => _mover = GetComponent<NexusRigidbodyMove>();

    void FixedUpdate() {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _mover.Move(input);
    }
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity.Controllers;

[RequireComponent(typeof(Rigidbody))]
public class NexusRigidbodyMove : MonoBehaviour
{
    public float Speed = 5f;
    private Rigidbody _rb;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    public void Move(Vector3 direction) {
        _rb.MovePosition(transform.position + direction * Speed * Time.fixedDeltaTime);
    }

    public void SetVelocity(Vector3 velocity) {
        _rb.linearVelocity = velocity;
    }
}
```

---

## Nexus Optimization Tip: Interpolation Mode
If the camera is following this object, mark the `Interpolation` setting on the Rigidbody as "Interpolate". This setting combined with `NexusRigidbodyMove` **ensures object movement looks smooth as oil even at low frame rates.**
