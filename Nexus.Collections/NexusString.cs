using System.Runtime.InteropServices;
using System.Text;

namespace Nexus.Collections;

/// <summary>
/// A memory-fixed, 32-byte string optimized for inclusion in unmanaged ECS components.
/// Standard C# strings are managed objects (heap), which cannot be stored in unmanaged buffers. 
/// NexusString32 solves this by using a 'fixed byte' buffer of internal memory.
/// </summary>
public unsafe struct NexusString32
{
    /// <summary> The raw memory block holding UTF8-encoded characters. </summary>
    private fixed byte _data[32];
    /// <summary> Tracked length of the active string data (max 31 bytes + 1 byte for length). </summary>
    private byte _length;

    /// <summary>
    /// Initializes a new fixed string from a managed C# string.
    /// Performs UTF8 encoding into the internal fixed buffer.
    /// </summary>
    /// <param name="value">The source managed string.</param>
    public NexusString32(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            _length = 0;
            return;
        }

        // Logic: Convert managed string to a UTF8 byte span.
        ReadOnlySpan<byte> source = Encoding.UTF8.GetBytes(value);
        // Cap the length at 31 bytes to fit the internal buffer safely.
        _length = (byte)Math.Min(source.Length, 31); 
        
        fixed (byte* ptr = _data)
        {
            // Direct memory copy from stack-provided span to the fixed unmanaged buffer.
            source.Slice(0, _length).CopyTo(new Span<byte>(ptr, 31));
        }
    }

    /// <summary>
    /// Decodes the internal unmanaged bytes back into a managed C# string.
    /// Used primarily for UI rendering or logging.
    /// </summary>
    /// <returns>A new managed string instance.</returns>
    public override string ToString()
    {
        if (_length == 0) return string.Empty;
        fixed (byte* ptr = _data)
        {
            // UTF8 Decode from a raw memory pointer.
            return Encoding.UTF8.GetString(ptr, _length);
        }
    }

    // --- High-Performance Implicit Casters ---
    public static implicit operator NexusString32(string value) => new NexusString32(value);
    public static implicit operator string(NexusString32 value) => value.ToString();
}

/// <summary>
/// A memory-fixed, 64-byte alternative for slightly longer labels or identifiers.
/// </summary>
public unsafe struct NexusString64
{
    private fixed byte _data[64];
    private byte _length;

    public NexusString64(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            _length = 0;
            return;
        }

        ReadOnlySpan<byte> source = Encoding.UTF8.GetBytes(value);
        _length = (byte)Math.Min(source.Length, 63);
        fixed (byte* ptr = _data)
        {
            source.Slice(0, _length).CopyTo(new Span<byte>(ptr, 63));
        }
    }

    public override string ToString()
    {
        if (_length == 0) return string.Empty;
        fixed (byte* ptr = _data)
        {
            return Encoding.UTF8.GetString(ptr, _length);
        }
    }

    public static implicit operator NexusString64(string value) => new NexusString64(value);
    public static implicit operator string(NexusString64 value) => value.ToString();
}

/// <summary>
/// A memory-fixed, 128-byte string for larger text payloads.
/// </summary>
public unsafe struct NexusString128
{
    private fixed byte _data[128];
    private byte _length;

    public NexusString128(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            _length = 0;
            return;
        }

        ReadOnlySpan<byte> source = Encoding.UTF8.GetBytes(value);
        _length = (byte)Math.Min(source.Length, 127);
        fixed (byte* ptr = _data)
        {
            source.Slice(0, _length).CopyTo(new Span<byte>(ptr, 127));
        }
    }

    public override string ToString()
    {
        if (_length == 0) return string.Empty;
        fixed (byte* ptr = _data)
        {
            return Encoding.UTF8.GetString(ptr, _length);
        }
    }

    public static implicit operator NexusString128(string value) => new NexusString128(value);
    public static implicit operator string(NexusString128 value) => value.ToString();
}
