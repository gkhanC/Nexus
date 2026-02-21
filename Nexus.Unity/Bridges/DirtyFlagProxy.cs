using Nexus.Registry;

namespace Nexus.Bridge;

/// <summary>
/// Bridge component for synchronizing unmanaged data with engine proxies (Unity/Unreal).
/// Uses Dirty-Flags to only update changed data.
/// </summary>
public unsafe class DirtyFlagProxy<T> where T : unmanaged
{
    public delegate void SyncDelegate(EntityId entity, T* component);

    public static void Sync(Nexus.Registry.Registry registry, SyncDelegate syncCallback)
    {
        SparseSet<T> set = registry.GetSet<T>();
        int count = set.Count;
        if (count == 0) return;

        // 1. Get raw access to the dirty bitmask (1 bit per entity).
        uint* dirtyBits = (uint*)set.GetRawDirtyBits(out int bitCount);

        // 2. Iterate through the bitmask in 32-bit chunks.
        for (int i = 0; i < bitCount; i++)
        {
            uint mask = dirtyBits[i];
            
            // OPTIMIZATION: If the entire 32-bit block is 0, skip 32 entities at once.
            if (mask == 0) continue;

            // 3. Process only the entities in this specific 32-bit block.
            int baseIdx = i * 32;
            for (int bit = 0; bit < 32; bit++)
            {
                int currentIdx = baseIdx + bit;
                if (currentIdx >= count) break;

                // Check if the specific bit is set.
                if ((mask & (1u << bit)) != 0)
                {
                    syncCallback(set.GetEntity(currentIdx), set.GetComponent(currentIdx));
                    // Note: We don't ClearDirty here anymore, we do it globally at the end of the frame 
                    // or let the user decide when to flush if they need multiple sync passes.
                }
            }
            
            // Optional: Clear the entire mask after processing. 
            // In Nexus, we usually call set.ClearAllDirty() at the end of the frame.
            dirtyBits[i] = 0; 
        }
    }
}
