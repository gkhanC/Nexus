using Nexus.Registry;

namespace Nexus.Bridge;

/// <summary>
/// A high-performance bridge for synchronizing Nexus state with Game Engines (Unity, Unreal, Godot).
/// Supports bi-directional sync with dirty tracking.
/// </summary>
public unsafe class NexusBridge<T> where T : unmanaged
{
    public delegate void PushDelegate(EntityId entity, T* nexusData);
    public delegate void PullDelegate(EntityId entity, T* nexusData);

    /// <summary>
    /// Pushes changes from Nexus to the Engine (Simulation -> Rendering).
    /// Typically called in Update or LateUpdate.
    /// </summary>
    public static void Push(Nexus.Registry.Registry registry, PushDelegate pushCallback)
    {
        var set = registry.GetSet<T>();
        for (int i = 0; i < set.Count; i++)
        {
            if (set.IsDirty((uint)i))
            {
                pushCallback(set.GetEntity(i), set.GetComponent(i));
                // We clear here if we assume this is the final sync of the frame
                set.ClearDirty((uint)i);
            }
        }
    }

    /// <summary>
    /// Pulls changes from the Engine to Nexus (Engine Input/Physics -> Simulation).
    /// Typically called at the start of a frame before simulation.
    /// </summary>
    public static void Pull(Nexus.Registry.Registry registry, PullDelegate pullCallback, Func<EntityId, bool> engineDirtyCheck = null)
    {
        var set = registry.GetSet<T>();
        for (int i = 0; i < set.Count; i++)
        {
            EntityId entity = set.GetEntity(i);
            
            // Only pull if engine side is dirty, or if no check is provided (pull all)
            if (engineDirtyCheck == null || engineDirtyCheck(entity))
            {
                T* component = set.GetComponent(i);
                pullCallback(entity, component);
                
                // Mark as dirty so Nexus systems know this changed externally
                set.SetDirty((uint)i);
            }
        }
    }
}

/// <summary>
/// Orchestrator for managing multiple component bridges and handling frequency.
/// </summary>
public class BridgeOrchestrator
{
    private readonly Nexus.Registry.Registry _registry;
    private float _syncInterval = 1.0f / 60.0f; // Default 60FPS sync
    private float _timer = 0;

    public BridgeOrchestrator(Nexus.Registry.Registry registry, int targetFps = 60)
    {
        _registry = registry;
        _syncInterval = 1.0f / targetFps;
    }

    public void Update(float deltaTime, Action syncBatch)
    {
        _timer += deltaTime;
        if (_timer >= _syncInterval)
        {
            syncBatch?.Invoke();
            _timer = 0;
        }
    }
}
