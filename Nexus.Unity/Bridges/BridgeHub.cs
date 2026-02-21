using Nexus.Registry;

namespace Nexus.Bridge;

/// <summary>
/// A central hub for managing all active component bridges.
/// Engines register their sync logic here once and call PushAll/PullAll per frame.
/// </summary>
public class BridgeHub
{
    private readonly Nexus.Registry.Registry _registry;
    private readonly List<Action> _pushActions = new();
    private readonly List<Action> _pullActions = new();

    public BridgeHub(Nexus.Registry.Registry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Registers a component type for synchronization.
    /// </summary>
    public void Register<T>(NexusBridge<T>.PushDelegate push, NexusBridge<T>.PullDelegate pull = null, Func<EntityId, bool> engineDirtyCheck = null) where T : unmanaged
    {
        if (push != null)
        {
            _pushActions.Add(() => NexusBridge<T>.Push(_registry, push));
        }
        
        if (pull != null)
        {
            _pullActions.Add(() => NexusBridge<T>.Pull(_registry, pull, engineDirtyCheck));
        }
    }

    /// <summary>
    /// Called at start of frame: Engine -> Nexus.
    /// </summary>
    public void PullAll()
    {
        foreach (var action in _pullActions) action();
    }

    /// <summary>
    /// Called at end of frame or update: Nexus -> Engine.
    /// </summary>
    public void PushAll()
    {
        foreach (var action in _pushActions) action();
    }
}
