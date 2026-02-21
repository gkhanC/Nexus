using UnityEngine;
using System;

namespace Nexus.Unity.Core
{
    /// <summary>
    /// NexusMathExtensions: Optimized math utilities for Unity types.
    /// </summary>
    public static class NexusMathExtensions
    {
        public static Vector3 SmoothStep(this Vector3 current, Vector3 target, float t)
        {
            return Vector3.Lerp(current, target, t * t * (3f - 2f * t));
        }

        public static float InverseLerp(this Vector2 range, float value)
        {
            return Mathf.InverseLerp(range.x, range.y, value);
        }
    }

    /// <summary>
    /// NexusStringExtensions: GC-friendly string operations.
    /// </summary>
    public static class NexusStringExtensions
    {
        public static bool FastEquals(this string s1, string s2)
        {
            if (ReferenceEquals(s1, s2)) return true;
            if (s1 == null || s2 == null) return false;
            return s1.Length == s2.Length && s1 == s2;
        }
    }

    /// <summary>
    /// NexusClassExtensions: Generic class utilities.
    /// </summary>
    public static class NexusClassExtensions
    {
        public static T EnsureComponent<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            return comp != null ? comp : go.AddComponent<T>();
        }
    }
}
