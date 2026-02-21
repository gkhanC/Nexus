# Nexus Prime Architectural Manual: LiveShaderBridge (Live Shader Streaming)

## 1. Introduction
`LiveShaderBridge.cs` is the "Superhighway" between Nexus Prime's high-performance data world and the GPU's parallel processing power. It coordinates the visualization process at the unmanaged level by transferring thousands of unmanaged component data (e.g., Position, Temperature, Flow rate) directly to shaders (GPU) via `ComputeBuffer` every frame.

The reason for this bridge's existence is to avoid the cost of individual `MaterialPropertyBlock` updates for each entity and to ensure that the GPU accesses all entity data via a single texture or buffer at O(1) cost.

---

## 2. Technical Analysis
Uses the following architecture for maximum GPU throughput (bandwidth):

- **ComputeBuffer Integration**: Presents data in its closest form to GPU registers, i.e., as a raw buffer. Defines how much space each entity takes on the GPU (e.g., float4 = 16 bytes) with the `STRIDE` parameter.
- **Bulk Data Gathering**: Packages data from the unmanaged `Registry` into a single bulk memory block and transmits it to the GPU.
- **Global Shader Properties**: Makes data accessible for shaders (VFX Graph, Custom Shaders) in the entire scene via the `Shader.SetGlobalBuffer` method.
- **Memory Management (IDisposable)**: Performs safe cleaning (Cleanup) with the `Dispose` pattern to prevent leaking the ComputeBuffer, which is an unmanaged GPU resource.

---

## 3. Logical Flow
1.  **Setup**: Space is opened on the GPU side (Buffer Allocation) according to the entity count and data width (STRIDE).
2.  **Data Collection**: Every frame, the Nexus Registry is scanned and up-to-date data is collected into a single array.
3.  **Streaming (Streaming)**: The prepared array is copied to GPU memory with `ComputeBuffer.SetData`.
4.  **Consumption**: Data is instantly accessed via the `StructuredBuffer<float4> _NexusEntityData` definition in shader files.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **ComputeBuffer** | Unity memory structure used to transfer large amounts of raw data from CPU to GPU. |
| **Stride** | The size of each element in the buffer in bytes. |
| **GPU Streaming** | Streaming data to the graphics processor continuously and refreshed every frame. |
| **Batching Efficiency** | The ability to draw or update thousands of objects in a single operation. |

---

## 5. Usage Example
```csharp
// Push data to VFX Graph or Custom Shader
var shaderBridge = new LiveShaderBridge(10000); // for 10k entities

void Update() {
    shaderBridge.UpdateBuffer(mainRegistry);
}

// Reading on the shader side:
// StructuredBuffer<float4> _NexusEntityData;
// float3 pos = _NexusEntityData[entityIndex].xyz;
```

---

## 6. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Bridge;

public class LiveShaderBridge : IDisposable
{
    private ComputeBuffer _dataBuffer;
    private const int STRIDE = 16; 

    public LiveShaderBridge(int entityCount) {
        _dataBuffer = new ComputeBuffer(entityCount, STRIDE);
    }

    public unsafe void UpdateBuffer(Registry.Registry registry) {
        // Gathering data logic...
        Shader.SetGlobalBuffer("_NexusEntityData", _dataBuffer);
    }

    public void Dispose() => _dataBuffer?.Release();
}
```

---

## Nexus Optimization Tip: Persistent Buffer
Create the buffer once (`Persistent`) instead of destroying and re-creating it every frame. Only update the data inside it with `SetData`. This **ensures performance stability by zeroing the memory allocation (Allocation) load.**
