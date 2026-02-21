using UnityEngine;
using Nexus.Data;
using System;

namespace Nexus.Unity
{
    /// <summary>
    /// Managed wrapper for NexusAttribute to allow Inspector editing.
    /// Bridges Unity's serialization with unmanaged data.
    /// </summary>
    [Serializable]
    public class NexusAttributeWrapper
    {
        public float CurrentValue;
        public float MaxValue;

        public NexusAttribute ToUnmanaged() => new NexusAttribute { Current = CurrentValue, Max = MaxValue };
        
        public void FromUnmanaged(NexusAttribute attr)
        {
            CurrentValue = attr.Current;
            MaxValue = attr.Max;
        }
    }

    /// <summary>
    /// Managed wrapper for NexusMinMax to allow Inspector editing.
    /// </summary>
    [Serializable]
    public class NexusMinMaxWrapper
    {
        public float MinValue;
        public float MaxValue;

        public NexusMinMax<float> ToUnmanaged() => new NexusMinMax<float>(MinValue, MaxValue);
        
        public void FromUnmanaged(NexusMinMax<float> range)
        {
            MinValue = range.Min;
            MaxValue = range.Max;
        }
    }
}
