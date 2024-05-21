using System.Collections.Generic;

namespace GameStateAndData
{
    public interface IObservable<out T>
    {
        event ObservableChangedHandler<T> Changed;

        T Value { get; }
    }

    public delegate void ObservableChangedHandler<in T>(T previousValue, T value);

    public interface IObservableCollection<in T>
    {
        event ObservableCollectionChangedHandler Changed;

        bool Contains(T value);
    }

    public delegate void ObservableCollectionChangedHandler();

    public interface IObservableDictionary<in TKey, TValue>
    {
        event ObservableCollectionChangedHandler Changed;

        bool ContainsKey(TKey key);
        
        TValue this[TKey key] { get; }
        
        bool TryGetValue(TKey key, out TValue value);
    }

    public abstract class GameDataField<T> : IObservable<T>, ISaveDataVisitable
    {
        private readonly IDirtiable _dirtyWhenChanged;
        private T _value;

        protected GameDataField(IDirtiable dirtyWhenChanged)
        {
            _dirtyWhenChanged = dirtyWhenChanged;
        }

        public event ObservableChangedHandler<T> Changed;

        public T Value
        {
            get => _value;
            set
            {
                var previousValue = _value;
                _value = value;
                Changed?.Invoke(previousValue, value);
                _dirtyWhenChanged.MarkDirty();
            }
        }

        public abstract void Accept(ISaveDataVisitor visitor);

        public override string ToString()
        {
            return _value.ToString();
        }
    }

    public abstract class GameDataQueueField<T> : IObservableCollection<T>, ISaveDataVisitable
    {
        public event ObservableCollectionChangedHandler Changed;

        private readonly Queue<T> _queue = new();

        protected GameDataQueueField(IDirtiable dirtyWhenChanged)
        {
            Changed += dirtyWhenChanged.MarkDirty;
        }

        public abstract void Accept(ISaveDataVisitor visitor);

        public T Dequeue()
        {
            var value = _queue.Dequeue();
            Changed?.Invoke();
            return value;
        }

        public void Enqueue(T value)
        {
            _queue.Enqueue(value);
            Changed?.Invoke();
        }

        public void Clear()
        {
            _queue.Clear();
            Changed?.Invoke();
        }

        public bool Contains(T value)
        {
            return _queue.Contains(value);
        }

        public IReadOnlyCollection<T> Items => _queue;
    }
    
    public abstract class GameDataHashSetField<T> : IObservableCollection<T>, ISaveDataVisitable
    {
        public event ObservableCollectionChangedHandler Changed;

        private readonly HashSet<T> _hashSet = new();

        protected GameDataHashSetField(IDirtiable dirtyWhenChanged)
        {
            Changed += dirtyWhenChanged.MarkDirty;
        }

        public abstract void Accept(ISaveDataVisitor visitor);

        public void Remove(T value)
        {
            _hashSet.Remove(value);
            Changed?.Invoke();
        }

        public void Add(T value)
        {
            _hashSet.Add(value);
            Changed?.Invoke();
        }

        public void Clear()
        {
            _hashSet.Clear();
            Changed?.Invoke();
        }

        public bool Contains(T value)
        {
            return _hashSet.Contains(value);
        }

        public IReadOnlyCollection<T> Items => _hashSet;
    }

    public abstract class GameDataDictionaryField<TKey, TValue> : IObservableDictionary<TKey, TValue>, ISaveDataVisitable
    {
        public event ObservableCollectionChangedHandler Changed;
        
        private readonly Dictionary<TKey, TValue> _dictionary = new();

        protected GameDataDictionaryField(IDirtiable dirtyWhenChanged)
        {
            Changed += dirtyWhenChanged.MarkDirty;
        }

        public abstract void Accept(ISaveDataVisitor visitor);

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Clear()
        {
            _dictionary.Clear();
            Changed?.Invoke();
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            Changed?.Invoke();
        }
        
        public bool Remove(TKey key)
        {
            var result = _dictionary.Remove(key);
            Changed?.Invoke();
            return result;
        }

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                _dictionary[key] = value; 
                Changed?.Invoke();
            }
        }

        public IReadOnlyCollection<TKey> Keys => _dictionary.Keys;

        public IReadOnlyCollection<TValue> Values => _dictionary.Values;

        public IReadOnlyCollection<KeyValuePair<TKey,TValue>> Items => _dictionary;

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}