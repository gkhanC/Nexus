using System;
using UnityEngine.Events;
using UnityEngine;

namespace Nexus.Unity
{
    /// <summary>
    /// NexusStackable: A container for values that can be stacked, capped, and spent.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    [Serializable]
    public class NexusStackable<T> where T : struct
    {
        [SerializeField] private int _count;
        [SerializeField] private int _cap = -1;
        [SerializeField] private bool _useCap = false;

        public UnityEvent<int> OnValueChanged = new();

        public int Count => _count;
        public int Cap => _cap;
        public bool IsCapped => _useCap;

        public NexusStackable<T> SetCap(int cap)
        {
            _cap = cap;
            _useCap = cap >= 0;
            return this;
        }

        public bool Add(int amount)
        {
            if (_useCap && (_count + amount) > _cap) return false;
            _count += amount;
            OnValueChanged?.Invoke(_count);
            return true;
        }

        public bool TrySpend(int amount)
        {
            if (_count < amount) return false;
            _count -= amount;
            OnValueChanged?.Invoke(_count);
            return true;
        }

        public static implicit operator int(NexusStackable<T> s) => s.Count;
    }
}
