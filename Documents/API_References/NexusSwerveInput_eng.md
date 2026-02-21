# Nexus Prime Architectural Manual: NexusSwerveInput (Swerve Input Reader)

## 1. Introduction
`NexusSwerveInput.cs` is a low-level input unit that converts player "Dragging" (Drag) movements on a touch screen or mouse into numerical data. Integrated from the HypeFire framework, this system forms the basis of game genres particularly where horizontal movement is at the forefront.

The reason for this unit's existence is to provide clean data to controllers by converting raw coordinate data ("My finger is now at (500, 300)") into a meaningful movement factor ("My finger swiped 10 units to the left").

---

## 2. Technical Analysis
Offers the following tools for input reading performance:

- **Move Factor Processing**: Calculates the distance the mouse or finger has traveled since the last frame (`_lastMouseX`) and scales it with the `Sensitivity` (Sensitivity) multiplier.
- **Screen-to-World Parity**: With the `GetHorizontalWorldPosition` method, reflects the touch point on the screen to the horizontal X coordinate in the game world according to the camera's depth.
- **Multi-Platform Consistency**: Conducts a logic compatible with both `GetMouseButton` and `Touch` systems on mobile platforms (via Unity's legacy input system).

---

## 3. Logical Flow
1.  **Down (Down)**: The first coordinate where the finger touches the screen is recorded.
2.  **Dragging (Drag)**: In each frame, the difference (`delta`) between the current position and the previous position is found.
3.  **Reset (Up)**: The movement factor is pulled to 0 when the finger is lifted from the screen.
4.  **Coordinate Conversion**: When necessary, the pixel value on the screen is converted to the world coordinate according to the camera's angle of view.

---

## 4. Usage Example
```csharp
public class MySwerveMover : MonoBehaviour {
    [SerializeField] private NexusSwerveInput _inputReader;

    void Update() {
        // Get the exact X position of the finger in the world
        float worldX = _inputReader.GetHorizontalWorldPosition();
        
        // Or get the swipe speed (delta)
        float deltaX = _inputReader.MoveFactorX;
    }
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity.Inputs;

public class NexusSwerveInput : MonoBehaviour
{
    public float Sensitivity = 1f;
    private float _lastMouseX;
    private float _moveFactorX;

    public float MoveFactorX => _moveFactorX;

    private void Update() {
        if (Input.GetMouseButtonDown(0)) _lastMouseX = Input.mousePosition.x;
        else if (Input.GetMouseButton(0)) {
            _moveFactorX = (Input.mousePosition.x - _lastMouseX) * Sensitivity;
            _lastMouseX = Input.mousePosition.x;
        }
        else _moveFactorX = 0f;
    }
}
```

---

## Nexus Optimization Tip: Delta Smoothing
On sensitive hardware (e.g., 120Hz screens), input data can sometimes come quite sharp or shaky (jitter). Normalizing the `MoveFactorX` value with `Time.deltaTime` or smoothing it with a small `Lerp` **significantly improves the player control feel (User Experience).**
