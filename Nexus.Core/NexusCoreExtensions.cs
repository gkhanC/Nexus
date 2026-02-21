using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nexus.Core;

namespace Nexus.Core
{
    public static unsafe class NexusCoreExtensions
    {
        // 1. IsNull/IsNotNull
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull<T>(this T* ptr) where T : unmanaged => ptr == null;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<T>(this T* ptr) where T : unmanaged => ptr != null;

        // 2. ThrowIfNull
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull<T>(this T* ptr, string msg = "Pointer is null") where T : unmanaged
        {
            if (ptr == null) throw new NullReferenceException(msg);
        }

        // 3. AsRef
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(this T* ptr) where T : unmanaged => ref Unsafe.AsRef<T>(ptr);

        // 4. IsBlittable
        public static bool IsBlittable<T>() => !RuntimeHelpers.IsReferenceOrContainsReferences<T>();

        // 5. GetAddress
        public static IntPtr GetAddress<T>(ref T value) where T : unmanaged => (IntPtr)Unsafe.AsPointer(ref value);

        // 6. IsInRange
        public static bool IsInRange(this int value, int min, int max) => value >= min && value <= max;

        // 7. BitCast
        public static T BitCast<T, U>(U value) where T : unmanaged where U : unmanaged => Unsafe.As<U, T>(ref value);

        // 8. IsAligned
        public static bool IsAligned(void* ptr, int alignment) => ((long)ptr % alignment) == 0;

        // 9. ClearMemory
        public static void ClearMemory<T>(this T* ptr) where T : unmanaged => Unsafe.InitBlock(ptr, 0, (uint)sizeof(T));

        // 10. IsZero
        public static bool IsZero(this float value) => MathF.Abs(value) < float.Epsilon;

        // 11. FastHash
        public static int FastHash<T>(this T value) where T : unmanaged
        {
            byte* p = (byte*)&value;
            int hash = 17;
            for (int i = 0; i < sizeof(T); i++) hash = hash * 31 + p[i];
            return hash;
        }

        // 12. Toggle
        public static void Toggle(this ref bool value) => value = !value;

        // 13. IsPowerOfTwo
        public static bool IsPowerOfTwo(this int x) => x > 0 && (x & (x - 1)) == 0;

        // 14. ByteSize
        public static int ByteSize<T>() where T : unmanaged => sizeof(T);

        // 15. ToSpan
        public static Span<T> ToSpan<T>(this T* ptr, int length) where T : unmanaged => new Span<T>(ptr, length);

        // 16. WrapAngle
        public static float WrapAngle(this float angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }

        // 17. Bit Manipulation
        public static bool GetBit(this int value, int bit) => (value & (1 << bit)) != 0;
        public static int SetBit(this int value, int bit, bool state) => state ? (value | (1 << bit)) : (value & ~(1 << bit));

        // 18. Entity Quality of Life
        public static bool IsValidEntity(this EntityId id, Registry registry) => registry.IsValid(id);

        // 19. DeepClone
        public static T* DeepClone<T>(this T* source) where T : unmanaged
        {
            T* clone = (T*)NativeMemory.Alloc((nuint)sizeof(T));
            Unsafe.Copy(clone, source);
            return clone;
        }

        // 20. ToBinaryString
        public static string ToBinaryString(this int value) => Convert.ToString(value, 2).PadLeft(32, '0');

        // 21. DefaultIfNull
        public static T DefaultIfNull<T>(this T* ptr) where T : unmanaged => ptr == null ? default : *ptr;

        // 22. Increment/Decrement (Atomic style but unmanaged)
        public static void Increment(this int* ptr) => (*ptr)++;
        public static void Decrement(this int* ptr) => (*ptr)--;

        // 23. IsEmpty (Spans)
        public static bool IsEmpty<T>(this Span<T> span) => span.Length == 0;

        // 24. CopyFrom (Bitwise)
        public static void CopyFrom<T>(this T* dest, T* src) where T : unmanaged => Unsafe.Copy(dest, src);

        // 25. ReferenceEquals (Pointer)
        public static bool ReferenceEquals<T>(T* a, T* b) where T : unmanaged => a == b;

        // 26. IsNormalized (Vector approximation)
        public static bool IsNormalized(this System.Numerics.Vector3 v) => MathF.Abs(v.LengthSquared() - 1f) < 0.001f;

        // 27. Approximately (Float)
        public static bool Approximately(this float a, float b) => MathF.Abs(a - b) < 0.001f;

        // 28. ClampMagnitude (Manual SIMD-ready)
        public static void ClampMagnitude(this ref System.Numerics.Vector3 v, float max)
        {
            float sqrMag = v.LengthSquared();
            if (sqrMag > max * max) v = v / MathF.Sqrt(sqrMag) * max;
        }

        // 29. IsZero (Vector3)
        public static bool IsZero(this System.Numerics.Vector3 v) => v.LengthSquared() < float.Epsilon;

        // 30. ToNexusString (Bridge)
        // Note: Needs NexusString namespace
        public static Nexus.Collections.NexusString32 ToNexusString(this string s) => new Nexus.Collections.NexusString32(s);
    }
}
