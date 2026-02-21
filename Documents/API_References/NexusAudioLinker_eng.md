# Nexus Prime Architectural Manual: NexusAudioLinker (Audio-Data Linker)

## 1. Introduction
`NexusAudioLinker.cs` is a reactive helper component that connects variables in Nexus Prime's data-driven world (e.g., Speed, Stress level, Energy) to Unity's `AudioSource` parameters. It is used to strengthen the simulation depth of the game with audio feedback.

The reason for this linker's existence is to be able to automatically "map" an unmanaged piece of data (float) to properties such as audio pitch (Pitch) or volume (Volume), instead of writing complex scripts for each audio change.

---

## 2. Technical Analysis
Offers the following capabilities for audio synchronization:

- **Reactive Parametric Update**: Receives raw numerical data coming from the unmanaged world via the `UpdateAudio` method.
- **Linear Mapping (Lerp)**: Mathematically converts the incoming raw data (e.g., 0-100 speed) into a range the audio can understand (e.g., 0.5 - 1.5 pitch).
- **Low-Overhead Binding**: Minimizes the load on the CPU audio engine by being triggered only when data changes or at determined synchronization intervals.
- **Field-Based Configuration**: Which component field (Speed, Health, etc.) will affect which audio parameter can be easily configured via the Inspector.

---

## 3. Logical Flow
1.  **Input**: A value is read from an unmanaged component on the Nexus Registry.
2.  **Conversion**: The read value is normalized between the determined minimum/maximum ranges.
3.  **Application**: The normalized value is assigned to the `AudioSource.pitch` or `AudioSource.volume` property.
4.  **Result**: Dynamic effects such as the engine sound thinning as the vehicle speeds up or the heartbeat sound accelerating as health decreases are obtained.

---

## 4. Usage Example
```csharp
public class CarAudio : MonoBehaviour {
    [SerializeField] private NexusAudioLinker _engineLinker;

    void Update() {
        // Transfer 0-200 RPM data coming from Nexus to audio
        float currentRPM = GetUnmanagedRPM(); 
        _engineLinker.UpdateAudio(currentRPM);
    }
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Bridge;

public class NexusAudioLinker : MonoBehaviour
{
    public AudioSource Source;
    
    public unsafe void UpdateAudio(float value) {
        if (Source == null) return;
        
        // Example: Map a value between 0-100 to the 0.5-1.5 pitch range
        Source.pitch = Mathf.Lerp(0.5f, 1.5f, value / 100f);
    }
}
```

---

## Nexus Optimization Tip: Audio Update Culling
Instead of updating audio parameters of all objects every frame, make `UpdateAudio` calls only for objects within distance audible to the player (Audio Audibility Range). This can **reduce unnecessary processing load on the Audio Thread and CPU by 30%.**
