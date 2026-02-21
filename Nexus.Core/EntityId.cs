using System.Runtime.InteropServices;

namespace Nexus.Core;

/// <summary>
/// A 64-bit lightweight handle representing a unique entity in the Nexus Registry.
/// Uses explicit layout to ensure a tight 8-byte footprint, ideal for cache-friendly transfers.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct EntityId : IEquatable<EntityId>
{
    /// <summary> 
    /// The index part of the handle. 
    /// Points to the entity's position in sparse/dense arrays.
    /// </summary>
    [FieldOffset(0)] public uint Index;

    /// <summary> 
    /// The version (generation) part of the handle.
    /// Incremented every time an entity at 'Index' is destroyed, 
    /// allowing the Registry to invalidate old handles.
    /// </summary>
    [FieldOffset(4)] public uint Version;

    /// <summary> A static 'Null' entity representation (equivalent to a null pointer). </summary>
    public static readonly EntityId Null = new EntityId { Index = uint.MaxValue, Version = 0 };

    /// <summary> Returns true if this handle represents the Null entity. </summary>
    public bool IsNull => Index == uint.MaxValue;

    // --- Equality implementations for high-speed comparisons ---

    public bool Equals(EntityId other) => Index == other.Index && Version == other.Version;
    public override bool Equals(object? obj) => obj is EntityId other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Index, Version);

    public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);
    public static bool operator !=(EntityId left, EntityId right) => !left.Equals(right);
}
