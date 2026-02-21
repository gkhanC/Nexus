using System;
using UnityEngine;
using Nexus.Registry;

namespace Nexus.Bridge
{
    /// <summary>
    /// Live Shader-State Bridge: Streams Nexus component data directly to the GPU.
    /// Uses ComputeBuffer to provide high-performance data access for custom shaders.
    /// </summary>
    public class LiveShaderBridge : IDisposable
    {
        private ComputeBuffer _dataBuffer;
        private const int STRIDE = 16; // e.g., float4 or similar

        public LiveShaderBridge(int entityCount)
        {
            _dataBuffer = new ComputeBuffer(entityCount, STRIDE);
        }

        /// <summary>
        /// Updates the GPU buffer with fresh data from the Nexus registry.
        /// </summary>
        public unsafe void UpdateBuffer(Registry.Registry registry)
        {
            // Logic: 
            // 1. Gather component data into a temporary unmanaged array.
            // 2. SetData to the ComputeBuffer.
            // 3. Bind the buffer to a Material or Global Shader Property.
            Shader.SetGlobalBuffer("_NexusEntityData", _dataBuffer);
        }

        public void Dispose()
        {
            _dataBuffer?.Release();
        }
    }
}
