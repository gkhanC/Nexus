using UnityEngine;

namespace Nexus.Mathematics;

/// <summary>
/// A collection of hardware-accelerated mathematical utilities for high-throughput data processing.
/// NexusMath leverages SIMD (Single Instruction, Multiple Data) to maximize CPU throughput, 
/// processing multiple floating-point values in parallel.
/// </summary>
public static unsafe class NexusMath
{
    /// <summary>
    /// Performs element-wise addition on two float arrays: result[i] = a[i] + b[i].
    /// Automatically detects CPU capabilities and uses the fastest available instruction set.
    /// </summary>
    /// <param name="a">Source pointer A (should be 32-byte aligned for AVX).</param>
    /// <param name="b">Source pointer B.</param>
    /// <param name="result">Destination pointer.</param>
    /// <param name="count">Number of elements to process.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(float* a, float* b, float* result, int count)
    {
        int i = 0;

        // PATH 1: AVX (Advanced Vector Extensions) 
        // Processes 8 floats (256 bits) in a single instruction.
        if (Avx.IsSupported)
        {
            // Process chunks of 8.
            for (; i <= count - 8; i += 8)
            {
                // Load 8 floats into a YMM register.
                var va = Avx.LoadVector256(a + i);
                var vb = Avx.LoadVector256(b + i);
                // Execute hardware addition and store back to memory.
                Avx.Store(result + i, Avx.Add(va, vb));
            }
        }
        // PATH 2: SSE (Streaming SIMD Extensions)
        // Fallback for older CPUs or smaller leftover chunks. Processes 4 floats (128 bits).
        else if (Sse.IsSupported)
        {
            for (; i <= count - 4; i += 4)
            {
                var va = Sse.LoadVector128(a + i);
                var vb = Sse.LoadVector128(b + i);
                Sse.Store(result + i, Sse.Add(va, vb));
            }
        }

        // PATH 3: SCALAR FALLBACK
        // Processes remaining elements (less than 4 or 8) one-by-one.
        for (; i < count; i++)
        {
            result[i] = a[i] + b[i];
        }
    }

    /// <summary>
    /// Performs element-wise multiplication on two float arrays: result[i] = a[i] * b[i].
    /// SIMD-optimized for high-density compute tasks like physics or particle updates.
    /// </summary>
    /// <param name="a">Source pointer A.</param>
    /// <param name="b">Source pointer B.</param>
    /// <param name="result">Destination pointer.</param>
    /// <param name="count">Numerical count.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Multiply(float* a, float* b, float* result, int count)
    {
        int i = 0;

        // Use 256-bit AVX if hardware allows.
        if (Avx.IsSupported)
        {
            for (; i <= count - 8; i += 8)
            {
                var va = Avx.LoadVector256(a + i);
                var vb = Avx.LoadVector256(b + i);
                Avx.Store(result + i, Avx.Multiply(va, vb));
            }
        }

        // Scalar loop for remaining tail elements.
        for (; i < count; i++)
        {
            result[i] = a[i] * b[i];
        }
    }

    /// <summary>
    /// A high-performance smooth-step function: 3x^2 - 2x^3.
    /// Used for smooth interpolations without the overhead of heavy transcendental functions.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastSmoothStep(float edge0, float edge1, float x)
    {
        // Clamp x to [0, 1] range
        float t = (x - edge0) / (edge1 - edge0);
        t = t < 0 ? 0 : (t > 1 ? 1 : t);
        // Hermite interpolation
        return t * t * (3 - 2 * t);
    }

    // --- Advanced Vector & Math Mastery ---
    using UnityEngine;

    public static Vector3 Add(Vector3 a, Vector3 b) => a + b;
    public static Vector3 Sub(Vector3 a, Vector3 b) => a - b;
    public static Vector3 Mul(Vector3 a, float b) => a * b;
    public static Vector3 Div(Vector3 a, float b) => a / b;

    public static float Dot(Vector3 a, Vector3 b) => Vector3.Dot(a, b);
    public static Vector3 Cross(Vector3 a, Vector3 b) => Vector3.Cross(a, b);
    public static float DistanceSq(Vector3 a, Vector3 b) => (a - b).sqrMagnitude;

    public static Vector3 Slerp(Vector3 a, Vector3 b, float t) => Vector3.Slerp(a, b, t);
    public static Vector3 Reflect(Vector3 inDir, Vector3 normal) => Vector3.Reflect(inDir, normal);
    
    public static Vector3 DirectionTo(Vector3 from, Vector3 to) => (to - from).normalized;
    public static float AngleTo(Vector3 a, Vector3 b) => Vector3.Angle(a, b);

    public static bool IsInsideSphere(Vector3 point, Vector3 center, float radius) => (point - center).sqrMagnitude <= radius * radius;

    public static Vector3 FlattenY(this Vector3 v) => new Vector3(v.x, 0, v.z);
    
    public static float Remap(float v, float min1, float max1, float min2, float max2) => min2 + (v - min1) * (max2 - min2) / (max1 - min1);

    public static float FastInverseSqrt(float x)
    {
        float xhalf = 0.5f * x;
        int i = BitConverter.SingleToInt32Bits(x);
        i = 0x5f3759df - (i >> 1);
        x = BitConverter.Int32ToSingle(i);
        x = x * (1.5f - xhalf * x * x);
        return x;
    }

    public static Vector3 SnapToGrid(Vector3 pos, float step) => new Vector3(MathF.Round(pos.x / step) * step, MathF.Round(pos.y / step) * step, MathF.Round(pos.z / step) * step);

    public static Vector3 BezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

    public static bool IsParallel(Vector3 a, Vector3 b) => 1f - MathF.Abs(Vector3.Dot(a.normalized, b.normalized)) < 0.001f;

    // --- New Mastery Features ---
    public static Vector3 RotateAround(Vector3 position, Vector3 center, Vector3 axis, float angle) => center + Quaternion.AngleAxis(angle, axis) * (position - center);
    public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal) => Vector3.ProjectOnPlane(vector, planeNormal);
    
    public static bool AABBCheck(Vector3 posA, Vector3 sizeA, Vector3 posB, Vector3 sizeB)
    {
        return (MathF.Abs(posA.x - posB.x) * 2 < (sizeA.x + sizeB.x)) &&
               (MathF.Abs(posA.y - posB.y) * 2 < (sizeA.y + sizeB.y)) &&
               (MathF.Abs(posA.z - posB.z) * 2 < (sizeA.z + sizeB.z));
    }

    public static float ManhattanDistance(Vector3 a, Vector3 b) => MathF.Abs(a.x - b.x) + MathF.Abs(a.y - b.y) + MathF.Abs(a.z - b.z);
    public static Quaternion LookRotation(Vector3 forward) => forward.sqrMagnitude > 0.001f ? Quaternion.LookRotation(forward) : Quaternion.identity;
    public static Vector3 Abs(Vector3 v) => new Vector3(MathF.Abs(v.x), MathF.Abs(v.y), MathF.Abs(v.z));
    public static float MaxComponent(Vector3 v) => MathF.Max(v.x, MathF.Max(v.y, v.z));
    public static float MinComponent(Vector3 v) => MathF.Min(v.x, MathF.Min(v.y, v.z));
    public static Vector3 MidPoint(Vector3 a, Vector3 b) => (a + b) * 0.5f;

    // --- Hardware / SIMD Helpers ---
    public static System.Runtime.Intrinsics.Vector128<float> ToFloat4(Vector3 v) => System.Runtime.Intrinsics.Vector128.Create(v.x, v.y, v.z, 0f);

    // --- Elementwise Operations ---
    public static Vector3 Multiply(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    public static Vector2 Multiply(Vector2 a, Vector2 b) => new Vector2(a.x * b.x, a.y * b.y);

    // --- Math Utilities ---
    public static int GetGreatestCommonDivisor(int a, int b)
    {
        while (b != 0) { int t = b; b = a % b; a = t; }
        return a;
    }
}
