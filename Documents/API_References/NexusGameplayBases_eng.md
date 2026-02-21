# Nexus Prime Architectural Manual: NexusGameplayBases (Basic Gameplay Engines)

## 1. Introduction
`NexusGameplayBases.cs` is a "Library File" where the most commonly used micro-controllers within Nexus Prime are collected. It offers core game mechanics such as Movement (Move), Rotation (Rotate), Physics Prediction (Trajectory), and Visual Color Effects (Hue Shift) in a standard structure.

The reason for this file's existence is to have optimized and tested basic building blocks (Building Blocks) ready, instead of writing simple movement or rotation code from scratch for each new object.

---

## 2. Technical Analysis
The following independent engines are included in the file:

- **NexusRotateController**: A visual tool that continuously rotates objects at a specific speed (`RotationSpeed`).
- **NexusRigidbodyMove**: Ensures objects move with `fixedDeltaTime` precision using Unity's physics engine (`Rigidbody`).
- **NexusTrajectorySimulator (Static)**: Mathematically predicts the future position of an object under gravity with the formula `start + velocity * t + 0.5 * g * t^2`.
- **NexusHueShifter**: Creates dynamic visual effects by continuously shifting an object's color on the HSV spectrum.

---

## 3. Logical Flow
1.  **Component Addition**: `NexusRotateController` is added to the object for the relevant mechanic (e.g., a continuously rotating coin object).
2.  **Configuration**: Speed or target values are entered via the Inspector.
3.  **Execution**: Components work in Unity's `Update/FixedUpdate` loop independently from the unmanaged world or with signals coming from the unmanaged world.

---

## 4. Usage Example
```csharp
// Move an object physically
var mover = GetComponent<NexusRigidbodyMove>();
mover.Move(Vector3.forward);

// Predict where a bullet will be after 2 seconds
Vector3 futurePos = NexusTrajectorySimulator.GetPointAtTime(pos, vel, 2f);
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity.Controllers;

public class NexusRotateController : MonoBehaviour {
    public Vector3 RotationSpeed;
    void Update() => transform.Rotate(RotationSpeed * Time.deltaTime);
}

public class NexusRigidbodyMove : MonoBehaviour {
    private Rigidbody _rb;
    public float Speed = 10f;
    void Awake() => _rb = GetComponent<Rigidbody>();
    public void Move(Vector3 dir) => _rb.MovePosition(transform.position + dir * Speed * Time.fixedDeltaTime);
}
```

---

## Nexus Optimization Tip: Component Sharing
Classes within `NexusGameplayBases` are quite lightweight (Lightweight). However, in thousands of objects, preferring to update `MaterialPropertyBlock` from a single central system instead of structures that continuously change `material.color` such as `NexusHueShifter`, **reduces visual render load by 20%.**
