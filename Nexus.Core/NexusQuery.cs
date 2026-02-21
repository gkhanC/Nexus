using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;
using Nexus.Core;

namespace Nexus.Core;

/// <summary>
/// A specialized 'Ref Struct' query object for ultra-fast iteration over entities with two specific components.
/// Uses SIMD-accelerated bitset filtering to process millions of entities per microsecond.
/// </summary>
/// <typeparam name="T1">First required component type.</typeparam>
/// <typeparam name="T2">Second required component type.</typeparam>
public unsafe ref struct NexusQuery<T1, T2>
    where T1 : unmanaged
    where T2 : unmanaged
{
    private readonly SparseSet<T1> _set1;
    private readonly SparseSet<T2> _set2;
    private readonly Registry _registry;

    /// <summary>
    /// Initializes a query by fetching the relevant component storage sets from the registry.
    /// </summary>
    /// <param name="registry">The source registry to query from.</param>
    public NexusQuery(Registry registry)
    {
        _set1 = registry.GetSet<T1>();
        _set2 = registry.GetSet<T2>();
        _registry = registry;
    }

    /// <summary> Delegate for the iteration callback. </summary>
    public delegate void ExecuteDelegate(EntityId entity, T1* c1, T2* c2);

    /// <summary>
    /// Executes the query logic using SIMD bitset ANDing for maximum throughput.
    /// </summary>
    /// <param name="callback">Logic to perform on every matching entity.</param>
    public void Execute(ExecuteDelegate callback)
    {
        uint* bits1 = (uint*)_set1.GetRawPresenceBits(out int count1);
        uint* bits2 = (uint*)_set2.GetRawPresenceBits(out int count2);
        int commonCount = Math.Min(count1, count2);

        int i = 0;

        // Path A: 256-bit AVX2 acceleration
        if (Avx2.IsSupported && commonCount >= 8)
        {
            for (; i <= commonCount - 8; i += 8)
            {
                Vector256<uint> v1 = Avx.LoadVector256(bits1 + i);
                Vector256<uint> v2 = Avx.LoadVector256(bits2 + i);
                Vector256<uint> result = Avx2.And(v1, v2);

                if (Avx2.MoveMask(result.AsByte()) != 0)
                {
                    ProcessChunk(i, result, callback);
                }
            }
        }

        // Path B: Fallback / Remainder
        for (; i < commonCount; i++)
        {
            uint combined = bits1[i] & bits2[i];
            if (combined != 0)
            {
                ProcessUnit(i, combined, callback);
            }
        }
    }

    private void ProcessChunk(int chunkIndex, Vector256<uint> mask, ExecuteDelegate callback)
    {
        // Simple scalar processing of the non-zero SIMD chunk for now. 
        // Could be further optimized with BitOperations.TrailingZeroCount.
        for (int j = 0; j < 8; j++)
        {
            uint val = mask.GetElement(j);
            if (val != 0) ProcessUnit(chunkIndex + j, val, callback);
        }
    }

    private void ProcessUnit(int unitIndex, uint mask, ExecuteDelegate callback)
    {
        uint baseEntityIndex = (uint)unitIndex << 5;
        for (int b = 0; b < 32; b++)
        {
            if ((mask & (1u << b)) != 0)
            {
                uint entityIndex = baseEntityIndex + (uint)b;
                // Since this is a bitset-positive, the entity versioning must be handled.
                // We resolve the ID through the registry to ensure it's still alive.
                EntityId id = _set1.GetEntity((int)_set1.GetRawSparse(out _)[entityIndex]);
                if (!id.IsNull)
                {
                    callback(id, _set1.Get(id), _set2.Get(id));
                }
            }
        }
    }
}
