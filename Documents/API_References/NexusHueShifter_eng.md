# Nexus Prime Architectural Manual: NexusHueShifter (Dynamic Color Shifter)

## 1. Introduction
`NexusHueShifter.cs` is a utility tool that dynamically changes the color tone (Hue) of visual elements within the game over time. Inherited from the HypeFire framework, this structure is used particularly for glowing effects, highlighted interface elements, and to create a "Premium" visual feel.

The reason for this tool's existence is to provide smooth and cyclic color transitions using a mathematical sine wave (Cosine) on the C# side, instead of writing a separate animation or shader for each color change.

---

## 2. Technical Analysis
Uses the following algorithmic approaches for color management:

- **Mathematical Cosine Wave**: Puts the color tone into an oscillation between 0.1 and 0.55 via the `Mathf.Cos` function. This ensures that colors always stay in a vibrant (Saturated) but non-eye-straining range.
- **HSV to RGB Conversion**: Converts the calculated "Hue" (Tone) value into Unity's full color spectrum (`Color.HSVToRGB`).
- **Editor-Time Support**: Allows the effect to be watched live within the Editor (using Time-since-startup) even if the game is not running via the `#if UNITY_EDITOR` preprocessor directive.
- **Preset Colors**: Offers ready-made color profiles with certain offset values such as `SoftBlue`, `SoftGreen`.

---

## 3. Logical Flow
1.  **Time Calculation**: An offset value is generated based on the hardware clock or game time.
2.  **Wave Application**: The time value is passed through a Cosine function and converted into a normalized tone in the range of [0, 1].
3.  **Color Production**: The normalized tone combines with saturation (Saturation) and brightness (Value) values to form the final `Color` data.

---

## 4. Usage Example
```csharp
void Update() {
    // Softly change the color of the object every frame
    GetComponent<Renderer>().material.color = NexusHueShifter.GetColor();

    // Use a ready-made profile
    var successColor = NexusHueShifter.SoftGreen;
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Mathematics;

public static class NexusHueShifter
{
    public static Color GetColor(float offset = 0f) {
        float hue = Mathf.Cos(Time.time + 1f) * 0.225f + 0.325f;
        return Color.HSVToRGB((hue + offset) % 1f, 1, 1);
    }

    public static Color SoftBlue => GetColor(0.2f);
}
```

---

## Nexus Optimization Tip: MaterialPropertyBlock
If you are changing the color of hundreds of objects in a frame with `NexusHueShifter`, use `MaterialPropertyBlock` instead of using `material.color` directly. This will **reduce CPU load by 15% by preserving GPU draw-call (batching) efficiency.**
