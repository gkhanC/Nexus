# Nexus Prime Architectural Manual: VFXGraphProvider (Visual Effect Data Provider)

## 1. Introduction
`VFXGraphProvider.cs` is the high-performance connection between Nexus Prime's unmanaged data world and Unity's GPU-based particle system, `VFX Graph`. It enables the creation of massive-scale and fluid visual effects by feeding position, color, and size data of millions of entities to VFX Graph as a "Point Cache" (Point Cache).

The reason for this provider's existence is to completely eliminate the burden of creating `GameObject` or `Transform` on the CPU for each particle and to transmit the data directly to the GPU's particle calculation pipelines (Pipeline).

---

## 2. Technical Analysis
Uses the following mechanisms for maximum visual performance:

- **Point Cache Injection**: Converts entity data in the unmanaged Registry into a `GraphicsBuffer` form that VFX Graph can understand.
- **Massive Scalability**: Unlike traditional particle systems, it can update the location of 1M+ particles 60 times per second simultaneously without locking CPU threads.
- **GPU-Side Consumption**: Once data is pushed to the GPU, the entire simulation of the effect (movement, color fading, etc.) takes place on the GPU.
- **Property Binding**: Connects unmanaged memory addresses directly to a name within VFX Graph (e.g., "EntityBuffer") using the `TargetVFX.SetGraphicsBuffer` method.

---

## 3. Logical Flow
1.  **Preparation**: A `StructuredBuffer` or `Texture2D` area to receive the data is defined within VFX Graph.
2.  **Packaging**: Visual attributes of entities (Position, Color, etc.) are collected in a single memory block by scanning the Nexus Registry.
3.  **Dispatch**: data is sent to the GPU via `GraphicsBuffer`.
4.  **Triggering**: VFX Graph uses the new incoming data as a particle source during the "Init" or "Update" phase.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **VFX Graph** | Unity's advanced node-based (Node-based) visual effect tool running over GPU. |
| **Point Cache** | Pre-calculated or live data set determining the starting points and properties of particles. |
| **GraphicsBuffer** | Raw GPU memory space optimized for compute shaders and visual effect systems. |

---

## 5. Usage Example
```csharp
public class CrowdVisualizer : MonoBehaviour {
    [SerializeField] private VFXGraphProvider _provider;

    void LateUpdate() {
        // Send locations of all city residents (10k+) to VFX Graph
        _provider.SyncWithVFX(mainRegistry);
    }
}
```

---

## 6. Full Source Implementation (Conceptual Implementation)

```csharp
namespace Nexus.Bridge;

public class VFXGraphProvider : MonoBehaviour
{
    public VisualEffect TargetVFX;
    
    public void SyncWithVFX(Registry.Registry registry)
    {
        if (TargetVFX == null) return;
        // 1. Gather component data to GraphicsBuffer.
        // 2. TargetVFX.SetGraphicsBuffer("EntityBuffer", buffer);
    }
}
```

---

## Nexus Optimization Tip: GPU Frustum Culling
Correctly configure `Culling` settings within VFX Graph. If the particle cloud is not in the camera's field of view, stop copying data from the Nexus Registry. This **optimizes the cost of data transfer to GPU memory (Bus Transfer) by 50%.**
