using UnityEngine;
using System.Collections.Generic;

namespace Nexus.Unity.Core
{
    /// <summary>
    /// INexusPoolable: Interface for objects that can be managed by the NexusObjectPool.
    /// </summary>
    public interface INexusPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }

    /// <summary>
    /// NexusObjectPool: Memory-efficient object pooling system.
    /// </summary>
    public class NexusObjectPool : MonoBehaviour
    {
        private static readonly Dictionary<string, Queue<GameObject>> _pools = new();

        public static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            string key = prefab.name;
            if (_pools.TryGetValue(key, out var queue) && queue.Count > 0)
            {
                var obj = queue.Dequeue();
                obj.transform.SetPositionAndRotation(pos, rot);
                obj.SetActive(true);
                obj.GetComponent<INexusPoolable>()?.OnSpawn();
                return obj;
            }
            return Instantiate(prefab, pos, rot);
        }

        public static void Despawn(GameObject obj)
        {
            obj.GetComponent<INexusPoolable>()?.OnDespawn();
            obj.SetActive(false);
            string key = obj.name.Replace("(Clone)", "");
            if (!_pools.ContainsKey(key)) _pools[key] = new Queue<GameObject>();
            _pools[key].Enqueue(obj);
        }
    }

    /// <summary>
    /// NexusMonoBehaviourSingleton: Thread-safe and performance-optimized Singleton pattern.
    /// </summary>
    public abstract class NexusMonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new();

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<T>();
                        if (_instance == null)
                        {
                            var go = new GameObject(typeof(T).Name);
                            _instance = go.AddComponent<T>();
                        }
                    }
                    return _instance;
                }
            }
        }
    }
}
