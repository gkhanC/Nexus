# Nexus Prime Architectural Manual: NexusSwerveController (Swerve/Swipe Controller)

## 1. Introduction
`NexusSwerveController.cs` is a specialized controller managing the "Horizontal Swipe" (Swerve) mechanic frequently encountered particularly in mobile and hyper-casual game genres. It converts `NexusSwerveInput` data, formed by the player dragging their finger or mouse left-right, into smooth horizontal movement of the object on the world.

The reason for this controller's existence is to reduce precise touch data on mobile platforms to a smooth, vibration-free, and bounded movement in the game world.

---

## 2. Technical Analysis
Uses the following techniques for the swipe mechanic:

- **Input Integration**: Takes the `MoveFactorX` value (how much the finger has swiped since the last frame) coming from the `NexusSwerveInput` component as the basic input.
- **Dynamic Clamping**: Prevents the object from going outside a specific horizontal corridor in the scene with `Mathf.Clamp` (`MaxSwerveAmount`).
- **Speed Scaling**: Multiplies movement speed by `SwerveSpeed` and `Time.deltaTime`, ensuring a consistent swipe speed at any touch screen speed.
- **Local Space Movement**: By performing the movement via `localPosition`, it facilitates the object going only left-right while staying attached to a higher hierarchy (e.g., a path parent continuously going forward).

---

## 3. Logical Flow
1.  **Input Reading**: The amount of horizontal change of the finger (or mouse) is taken.
2.  **Calculation**: "Potential New X" is found by adding the swipe amount to the current X position.
3.  **Constraint**: Potential X is forced within the limits allowed by the game (e.g., between -2 and +2).
4.  **Application**: The new smooth position is assigned to the object.

---

## 4. Usage Example
```csharp
// For character left-right swiping in a Runner type game:
// 1. Add [NexusSwerveInput] to the object.
// 2. Add [NexusSwerveController] to the object.
// 3. Set SwerveSpeed = 10, MaxSwerveAmount = 3.5.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity.Controllers;

public class NexusSwerveController : MonoBehaviour
{
    public float SwerveSpeed = 5f;
    public float MaxSwerveAmount = 2f;
    [SerializeField] private NexusSwerveInput _input;

    private void Update() {
        float swerveAmount = Time.deltaTime * SwerveSpeed * _input.MoveFactorX;
        float targetX = Mathf.Clamp(transform.localPosition.x + swerveAmount, -MaxSwerveAmount, MaxSwerveAmount);
        transform.localPosition = new Vector3(targetX, transform.localPosition.y, transform.localPosition.z);
    }
}
```

---

## Nexus Optimization Tip: Input Smoothing
Instead of using the raw data in `NexusSwerveInput` directly, pass it through a `Lerp` or `SmoothDamp` layer within `SwerveController`. This **prevents hard stops that may occur if the player pulls their finger suddenly and provides a "Premium" feel.**
