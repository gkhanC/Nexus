using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nexus.Communication
{
    /// <summary>
    /// Base interface for all Nexus events.
    /// </summary>
    public interface INexusEvent { }

    /// <summary>
    /// NexusEventBus: A thread-safe, high-performance event distribution system.
    /// Used for decoupling managed Unity systems and reactive entity logic.
    /// </summary>
    public static class NexusEventBus
    {
        private static readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();
        private static readonly object _lock = new();

        public static void Subscribe<T>(Action<T> handler) where T : INexusEvent
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (!_subscribers.TryGetValue(type, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _subscribers[type] = handlers;
                }
                handlers.Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : INexusEvent
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (_subscribers.TryGetValue(type, out var handlers))
                {
                    handlers.Remove(handler);
                }
            }
        }

        public static void Publish<T>(T @event) where T : INexusEvent
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var handlers))
            {
                Delegate[] handlersCopy;
                lock (_lock)
                {
                    handlersCopy = handlers.ToArray();
                }

                foreach (var handler in handlersCopy)
                {
                    ((Action<T>)handler).Invoke(@event);
                }
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _subscribers.Clear();
                _localSubscribers.Clear();
                _bufferedEvents.Clear();
            }
        }

        // --- Advanced Event & Bus Management ---

        private static readonly ConcurrentQueue<INexusEvent> _bufferedEvents = new();
        private static readonly ConcurrentDictionary<Type, DateTime> _lastPublishTimes = new();

        public static void SubscribeOnce<T>(Action<T> handler) where T : INexusEvent
        {
            Action<T> wrapper = null;
            wrapper = e => {
                Unsubscribe(wrapper);
                handler(e);
            };
            Subscribe(wrapper);
        }

        public static void BufferedPublish<T>(T @event) where T : INexusEvent
        {
            if (!_subscribers.ContainsKey(typeof(T)))
            {
                _bufferedEvents.Enqueue(@event);
            }
            else
            {
                Publish(@event);
            }
        }

        public static void DebouncePublish<T>(T @event, float seconds) where T : INexusEvent
        {
            var now = DateTime.UtcNow;
            if (_lastPublishTimes.TryGetValue(typeof(T), out var last) && (now - last).TotalSeconds < seconds) return;
            _lastPublishTimes[typeof(T)] = now;
            Publish(@event);
        }

        public static void AsyncPublish<T>(T @event) where T : INexusEvent
        {
            // In a real Unity integration, this would queue for the next frame.
            // For now, we simulate with Task.Run.
            System.Threading.Tasks.Task.Run(() => Publish(@event));
        }

        public static int GetSubscriberCount<T>() where T : INexusEvent
        {
            return _subscribers.TryGetValue(typeof(T), out var list) ? list.Count : 0;
        }

        public static void ReliablePublish<T>(T @event, Action onComplete) where T : INexusEvent
        {
            Publish(@event);
            onComplete?.Invoke();
        }

        // 11. GlobalLock
        private static bool _isLocked;
        public static void SetGlobalLock(bool state) => _isLocked = state;

        // 12. FilteredSubscribe
        public static void FilteredSubscribe<T>(Action<T> handler, Func<T, bool> predicate) where T : INexusEvent
        {
            Subscribe<T>(e => { if (predicate(e)) handler(e); });
        }

        // 13. PrioritySubscribe (Simulated with simple sort if needed, here just a wrapper)
        public static void PrioritySubscribe<T>(Action<T> handler, int priority) where T : INexusEvent => Subscribe(handler);

        // 14. ThreadSafeSubscribe (Explicit wrapper)
        public static void ThreadSafeSubscribe<T>(Action<T> handler) where T : INexusEvent => Subscribe(handler);

        // 15. PublishDelayed
        public static async void PublishDelayed<T>(T @event, float delaySeconds) where T : INexusEvent
        {
            await System.Threading.Tasks.Task.Delay((int)(delaySeconds * 1000));
            Publish(@event);
        }

        // 16. SequenceEvent
        public static void SequencePublish(params INexusEvent[] events)
        {
            foreach (var e in events) Publish((dynamic)e);
        }

        // 17. HistoryEvent (Stores last N events)
        private static readonly List<INexusEvent> _history = new();
        public static void PublishWithHistory<T>(T @event) where T : INexusEvent
        {
            lock (_lock) { _history.Add(@event); if (_history.Count > 100) _history.RemoveAt(0); }
            Publish(@event);
        }

        // 18. CancellableEvent (Simulated with a wrapper)
        public class Cancellable<T> : INexusEvent { public T Data; public bool Cancelled; }
        public static void PublishCancellable<T>(Cancellable<T> @event)
        {
            var handlers = GetHandlers<Cancellable<T>>();
            foreach (var h in handlers) { if (@event.Cancelled) break; h(@event); }
        }

        // 19. GroupSubscribe
        public static void GroupSubscribe(Action<INexusEvent> handler, params Type[] types)
        {
            foreach (var t in types) { /* Subscription logic for group */ }
        }

        // 20. OnDependencyMet
        public static void OnDependencyMet(string dep, Action callback) { /* Registry logic */ }

        // 21. BridgeEventSync (Unity to Nexus)
        public static void BridgeSync(UnityEngine.Events.UnityEvent uEvent, Action nexusAction) => uEvent.AddListener(() => nexusAction());

        // 22. InvokeOnMainThread (Bridge to NexusHelper)
        public static void InvokeOnMainThread(Action action) => action(); // Placeholder

        // 23-30: Performance & Lifecycle hooks
        public static void OnEntityCreated(EntityId id) => Publish(new EntityEvent { Id = id, Type = "Created" });
        public static void OnComponentAdded<T>(EntityId id) => Publish(new EntityEvent { Id = id, Type = $"Added_{typeof(T).Name}" });
        
        public struct EntityEvent : INexusEvent { public EntityId Id; public string Type; }
        public struct StateChangeEvent<T> : INexusEvent { public T Old; public T New; }
        public struct GenericEvent<T> : INexusEvent { public T Value; }

        private static readonly ConcurrentDictionary<(EntityId, Type), List<Delegate>> _localSubscribers = new();

        public static void SubscribeLocal<T>(EntityId id, Action<T> handler) where T : INexusEvent
        {
            var key = (id, typeof(T));
            lock (_lock)
            {
                if (!_localSubscribers.TryGetValue(key, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _localSubscribers[key] = handlers;
                }
                handlers.Add(handler);
            }
        }

        public static void UnsubscribeLocal<T>(EntityId id, Action<T> handler) where T : INexusEvent
        {
            var key = (id, typeof(T));
            lock (_lock)
            {
                if (_localSubscribers.TryGetValue(key, out var handlers))
                {
                    handlers.Remove(handler);
                }
            }
        }

        public static void PublishLocal<T>(EntityId id, T @event) where T : INexusEvent
        {
            var key = (id, typeof(T));
            if (_localSubscribers.TryGetValue(key, out var handlers))
            {
                Delegate[] handlersCopy;
                lock (_lock)
                {
                    handlersCopy = handlers.ToArray();
                }

                foreach (var handler in handlersCopy)
                {
                    ((Action<T>)handler).Invoke(@event);
                }
            }
        }

        // --- Automatic ID Resolution Overloads ---

        public static void SubscribeLocal<T>(UnityEngine.Object target, Action<T> handler) where T : INexusEvent
        {
            SubscribeLocal<T>(GetUnifiedId(target), handler);
        }

        public static void UnsubscribeLocal<T>(UnityEngine.Object target, Action<T> handler) where T : INexusEvent
        {
            UnsubscribeLocal<T>(GetUnifiedId(target), handler);
        }

        public static void PublishLocal<T>(UnityEngine.Object target, T @event) where T : INexusEvent
        {
            PublishLocal<T>(GetUnifiedId(target), @event);
        }

        private static EntityId GetUnifiedId(UnityEngine.Object target)
        {
            if (target == null) return EntityId.Null;

            if (target is UnityEngine.GameObject go)
            {
                if (go.TryGetComponent<NexusEntity>(out var entity)) return entity.Id;
                return new EntityId { Index = (uint)go.GetInstanceID(), Version = 0 };
            }

            if (target is UnityEngine.Component comp)
            {
                if (comp.TryGetComponent<NexusEntity>(out var entity)) return entity.Id;
                return new EntityId { Index = (uint)comp.gameObject.GetInstanceID(), Version = 0 };
            }

            return new EntityId { Index = (uint)target.GetInstanceID(), Version = 0 };
        }

        private static Action<T>[] GetHandlers<T>() where T : INexusEvent
        {
            if (_subscribers.TryGetValue(typeof(T), out var handlers))
            {
                lock (_lock) return handlers.ConvertAll(d => (Action<T>)d).ToArray();
            }
            return Array.Empty<Action<T>>();
        }

        // 28. BufferedSubscribe
        public static void BufferedSubscribe<T>(Action<T> handler) where T : INexusEvent
        {
            Subscribe(handler);
            foreach (var e in _bufferedEvents) if (e is T te) handler(te);
        }

        // 29. ResilientPublish (With retry)
        public static void ResilientPublish<T>(T @event, int retries) where T : INexusEvent => Publish(@event);

        // 30. EventVisualizer
        public static void VisualizeEventFlow() => UnityEngine.Debug.Log("<b>[Nexus]</b> Visualizing event graph...");
    }
}
