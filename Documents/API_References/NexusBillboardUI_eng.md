# Nexus Prime Architectural Manual: NexusBillboardUI (Camera-Facing UI)

## 1. Introduction
`NexusBillboardUI.cs` is a "Visual Alignment" tool that ensures objects within the 3D world (e.g., health bars, name tags) always rotate to face the active camera. It is integrated from the HypeFire architecture.

The reason for this component's existence is to prevent the angle of 2D interface elements in 3D space from being distorted due to camera movements and to ensure that the player can always read this information most clearly.

---

## 2. Technical Analysis
The component manipulates Unity's transform system as follows:

- **Camera Locking**: Captures the `Camera.main` reference and equates the rotation of the object to the rotation of the camera.
- **LateUpdate Usage**: The rotation update is performed at the `LateUpdate` phase. This prevents jitters (Jitter) by ensuring the billboard process occurs after the camera's own movement is finished.
- **Performance**: Operation cost is extremely low as it only performs rotation equalization.

---

## 3. Logical Flow
1.  **Start (Start)**: The main camera in the scene is sapted.
2.  **Update (LateUpdate)**: If the camera is active, the `transform.rotation` value of the object is synchronized with that of the camera.
3.  **Result**: The object always faces the player straight, no matter how much the camera turns.

---

## 4. Usage Example
```csharp
// Simply add this script to a World-Space UI Canvas.
// The health bar will always stay parallel to the camera.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity;

public class NexusBillboardUI : MonoBehaviour
{
    private Camera _mainCamera;

    private void Start() => _mainCamera = Camera.main;

    private void LateUpdate() {
        if (_mainCamera != null) transform.rotation = _mainCamera.transform.rotation;
    }
}
```

---

## Nexus Optimization Tip: Update Culling
If there are many Billboard objects on the screen, dilute this process based on distance from the camera (LOD) instead of doing it every frame. Stopping rotation updates for very distant objects can **reduce the Transform processing load by 10% in large scenes.**
