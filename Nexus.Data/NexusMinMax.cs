using System;
using System.Runtime.CompilerServices;

namespace Nexus.Data
{
    /// <summary>
    /// NexusMinMax: An unmanaged structure representing a range between two values.
    /// Provides zero-allocation randomization and clamping.
    /// </summary>
    /// <typeparam name="T">Unmanaged numeric type.</typeparam>
    public struct NexusMinMax<T> where T : unmanaged, IComparable<T>
    {
        public T Min;
        public T Max;

        public NexusMinMax(T min, T max)
        {
            Min = min;
            Max = max;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInRange(T value) => value.CompareTo(Min) >= 0 && value.CompareTo(Max) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Clamp(T value)
        {
            if (value.CompareTo(Min) < 0) return Min;
            if (value.CompareTo(Max) > 0) return Max;
            return value;
        }
    }

    // Specialized extensions for Float/Int for randomization
    public static class NexusMinMaxExtensions
    {
        public static float Random(this NexusMinMax<float> range) => UnityEngine.Random.Range(range.Min, range.Max);
        public static int Random(this NexusMinMax<int> range) => UnityEngine.Random.Range(range.Min, range.Max);
        
        public static float Lerp(this NexusMinMax<float> range, float t) => range.Min + (range.Max - range.Min) * t;
    }
}
