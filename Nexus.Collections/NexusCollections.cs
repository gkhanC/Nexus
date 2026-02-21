using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Nexus.Core;

namespace Nexus.Collections
{
    /// <summary>
    /// NexusRef<T>: A safe pointer wrapper for unmanaged entity components.
    /// Validates the entity's existence and version before allowing access to the pointer.
    /// </summary>
    public unsafe struct NexusRef<T> where T : unmanaged
    {
        private readonly T* _ptr;
        private readonly EntityId _id;
        private readonly Registry _registry;

        public NexusRef(T* ptr, EntityId id, Registry registry)
        {
            _ptr = ptr;
            _id = id;
            _registry = registry;
        }

        /// <summary> Returns the raw pointer if the entity is valid, otherwise null. </summary>
        public T* Ptr => _registry != null && _registry.IsValid(_id) ? _ptr : null;

        /// <summary> Returns true if the pointer is still safe to use. </summary>
        public bool IsValid => _registry != null && _registry.IsValid(_id);

        /// <summary> Access the component data by reference. Throws if invalid. </summary>
        public ref T Value
        {
            get
            {
                T* p = Ptr;
                if (p == null) throw new InvalidOperationException($"[Nexus.Memory.Violation] Attempted to dereference NexusRef<{typeof(T).Name}> for EntityId {_id.Index}:{_id.Version}. Entity state is 'Disposed' or 'Invalid'.");
                return ref *p;
            }
        }
    }

    /// <summary>
    /// Generic fixed-size unmanaged string. 
    /// Note: Optimized for zero-GC impact in ECS components.
    /// </summary>
    public unsafe struct NexusString<TSize> where TSize : unmanaged
    {
        private fixed byte _data[256]; // Maximized for typical usage, can be optimized per size.
        private int _length;

        public NexusString(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _length = 0;
                return;
            }

            ReadOnlySpan<byte> source = System.Text.Encoding.UTF8.GetBytes(value);
            // Cap to internal buffer or TSize hint
            _length = Math.Min(source.Length, 255);
            fixed (byte* ptr = _data)
            {
                source.Slice(0, _length).CopyTo(new Span<byte>(ptr, 255));
            }
        }

        public override string ToString()
        {
            if (_length == 0) return string.Empty;
            fixed (byte* ptr = _data)
            {
                return System.Text.Encoding.UTF8.GetString(ptr, _length);
            }
        }

        public static implicit operator NexusString<TSize>(string s) => new NexusString<TSize>(s);
        public static implicit operator string(NexusString<TSize> s) => s.ToString();
    }

    /// <summary>
    /// NexusList<T>: High-performance dynamic-size unmanaged list.
    /// Uses NexusMemoryManager for cache-aligned allocations.
    /// </summary>
    public unsafe struct NexusList<T> : IDisposable where T : unmanaged
    {
        private T* _buffer;
        private int _count;
        private int _capacity;

        public int Count => _count;

        public NexusList(int initialCapacity = 8)
        {
            _capacity = initialCapacity;
            _buffer = (T*)NexusMemoryManager.AllocCacheAligned(_capacity * sizeof(T));
            _count = 0;
        }

        public void Add(T item)
        {
            if (_count == _capacity)
            {
                int newCapacity = _capacity * 2;
                _buffer = (T*)NexusMemoryManager.Realloc(_buffer, _capacity * sizeof(T), newCapacity * sizeof(T));
                _capacity = newCapacity;
            }
            _buffer[_count++] = item;
        }

        public T* GetPtr(int index) => &_buffer[index];
        public ref T this[int index] => ref _buffer[index];

        public void Dispose()
        {
            if (_buffer != null)
            {
                NexusMemoryManager.Free(_buffer);
                _buffer = null;
            }
        }
    }

    /// <summary>
    /// NexusDictionary<K, V>: High-performance unmanaged hash map.
    /// Optimized for zero-GC lookups in logic systems.
    /// </summary>
    public unsafe struct NexusDictionary<K, V> : IDisposable 
        where K : unmanaged, IEquatable<K> 
        where V : unmanaged
    {
        private NexusList<K> _keys;
        private NexusList<V> _values;

        public NexusDictionary(int capacity = 16)
        {
            _keys = new NexusList<K>(capacity);
            _values = new NexusList<V>(capacity);
        }

        public void Add(K key, V value)
        {
            // Simple linear search for now, can be upgraded to hash table.
            _keys.Add(key);
            _values.Add(value);
        }

        public bool TryGetValue(K key, out V value)
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                if (_keys[i].Equals(key))
                {
                    value = _values[i];
                    return true;
                }
            }
            value = default;
            return false;
        }

        public void Dispose()
        {
            _keys.Dispose();
            _values.Dispose();
        }
    }

    /// <summary>
    /// NexusStack<T>: High-performance unmanaged stack.
    /// LIFO structure optimized for zero-GC logic.
    /// </summary>
    public unsafe struct NexusStack<T> : IDisposable where T : unmanaged
    {
        private T* _buffer;
        private int _count;
        private int _capacity;

        public int Count => _count;

        public NexusStack(int initialCapacity = 8)
        {
            _capacity = initialCapacity;
            _buffer = (T*)NexusMemoryManager.AllocCacheAligned(_capacity * sizeof(T));
            _count = 0;
        }

        public void Push(T item)
        {
            if (_count == _capacity)
            {
                int newCapacity = _capacity * 2;
                _buffer = (T*)NexusMemoryManager.Realloc(_buffer, _capacity * sizeof(T), newCapacity * sizeof(T));
                _capacity = newCapacity;
            }
            _buffer[_count++] = item;
        }

        public T Pop()
        {
            if (_count == 0) throw new InvalidOperationException("[Nexus.Collection.Underflow] NexusStack: Attempted POP operation on an empty unmanaged buffer.");
            return _buffer[--_count];
        }

        public T Peek()
        {
            if (_count == 0) throw new InvalidOperationException("[Nexus.Collection.Underflow] NexusStack: Attempted PEEK operation on an empty unmanaged buffer.");
            return _buffer[_count - 1];
        }

        public void Dispose()
        {
            if (_buffer != null)
            {
                NexusMemoryManager.Free(_buffer);
                _buffer = null;
            }
        }
    }
}
