using System.Threading;

namespace Nexus.Core;

/// <summary>
/// Provides high-speed, monotonically increasing unique IDs for every component type in the system.
/// This system enables O(1) array-based lookups for component storage instead of dictionary lookups.
/// </summary>
public static class ComponentTypeManager
{
    private static int _nextId = 0;

    /// <summary>
    /// Retrieves the unique integer ID for a component type.
    /// This method is virtually zero-cost after the first call due to static generic caching.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <returns>A unique integer ID (0 to MaxTypes).</returns>
    public static int GetId<T>() where T : unmanaged
    {
        // Logic: Access a static field on a generic class. 
        // The CLR initializes this field exactly once for every unique type 'T'.
        return TypeIdHolder<T>.Value;
    }

    /// <summary>
    /// Internal generic helper to cache the type ID.
    /// </summary>
    private static class TypeIdHolder<T> where T : unmanaged
    {
        // This is executed only once per component type T.
        // We use Interlocked for thread-safety during the initial ID assignment.
        public static readonly int Value = Interlocked.Increment(ref _nextId) - 1;
    }

    /// <summary> Returns the total count of unique component types registered in the system. </summary>
    public static int MaxTypes => _nextId;
}
