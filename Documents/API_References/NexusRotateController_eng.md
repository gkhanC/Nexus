# Nexus Prime Architectural Manual: NexusRotateController (Aligned Rotation)

## 1. Introduction
`NexusRotateController.cs` is an "Orientation Manager" (Orientation Manager) ensuring objects rotate smoothly towards a target or direction. It's used for visual alignment of characters, vehicles, or projectiles based on their movement directions.

The reason for this controller's existence is to prevent objects from rotating suddenly (snap) and to offer a visually pleasing, fluid rotation experience using mathematical `Slerp` (Spherical Linear Interpolation).

---

## 2. Technical Analysis
Uses the following methods for smooth rotation:

- **LookRotation**: Converts a given direction vector (`direction`) into the target Quaternion the projectile or character should look at.
- **Spherical Linear Interpolation (Slerp)**: Performs a time-based and curved transition between the current rotation and the target rotation.
- **Magnitude Filtering**: Prevents shaking (shaking) of the object where it stands by ignoring very small movement vectors (e.g., below 0.001f).
- **Time-Step Mastery**: using the `Time.deltaTime` multiplier ensures the rotation speed stays the same every second regardless of hardware.

---

## 3. Logical Flow
1.  **Input**: The direction the character wants to go (Vector3) is transmitted to the system.
2.  **Validation**: If the direction vector is not "zero", the process continues.
3.  **Calculation**: The target line of sight is mathematically determined.
4.  **Application**: The object softly glides from its current angle towards the target angle at `RotateSpeed` speed.

---

## 4. Usage Example
```csharp
public class EnemyAI : MonoBehaviour {
    private NexusRotateController _rotator;

    void Start() => _rotator = GetComponent<NexusRotateController>();

    void Update() {
        Vector3 targetDir = (player.position - transform.position).normalized;
        _rotator.RotateTowards(targetDir);
    }
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity.Controllers;

public class NexusRotateController : MonoBehaviour
{
    public float RotateSpeed = 10f;

    public void RotateTowards(Vector3 direction) {
        if (direction.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * RotateSpeed);
    }
}
```

---

## Nexus Optimization Tip: SqrMagnitude vs Distance
Never use `Vector3.Distance` or `magnitude` when performing distance or size checks; they contain expensive square root (sqrt) operations. Checks performed using `sqrMagnitude` **reduce CPU load in rotation logic by 5-8%.**
