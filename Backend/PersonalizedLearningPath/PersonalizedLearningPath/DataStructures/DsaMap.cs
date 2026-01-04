namespace PersonalizedLearningPath.DataStructures;

// Manual hash-map (separate chaining) built on top of the project's custom LinkedList.
// Intentionally avoids System.Collections.Generic.Dictionary<TKey,TValue>.
public sealed class DsaMap<TKey, TValue>
{
    private sealed class Entry
    {
        public TKey Key;
        public TValue Value;

        public Entry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    private LinkedList<Entry>[] _buckets;
    private int _count;

    public int Count => _count;

    public DsaMap(int initialCapacity = 101)
    {
        if (initialCapacity <= 0) initialCapacity = 101;
        _buckets = new LinkedList<Entry>[initialCapacity];
        for (int i = 0; i < _buckets.Length; i++) _buckets[i] = new LinkedList<Entry>();
    }

    private int IndexFor(TKey key)
    {
        var hc = key?.GetHashCode() ?? 0;
        // Keep it non-negative
        hc &= 0x7fffffff;
        return hc % _buckets.Length;
    }

    public bool ContainsKey(TKey key)
    {
        var idx = IndexFor(key);
        var n = _buckets[idx].Head;
        while (n != null)
        {
            if (Equals(n.Data.Key, key)) return true;
            n = n.Next;
        }
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        var idx = IndexFor(key);
        var n = _buckets[idx].Head;
        while (n != null)
        {
            if (Equals(n.Data.Key, key))
            {
                value = n.Data.Value;
                return true;
            }
            n = n.Next;
        }

        value = default!;
        return false;
    }

    public TValue GetOrDefault(TKey key, TValue defaultValue = default!)
    {
        return TryGetValue(key, out var v) ? v : defaultValue;
    }

    public void Set(TKey key, TValue value)
    {
        var idx = IndexFor(key);
        var n = _buckets[idx].Head;
        while (n != null)
        {
            if (Equals(n.Data.Key, key))
            {
                n.Data.Value = value;
                return;
            }
            n = n.Next;
        }

        _buckets[idx].Add(new Entry(key, value));
        _count++;

        // Simple resize policy
        if (_count > _buckets.Length * 2)
        {
            Rehash(_buckets.Length * 2 + 1);
        }
    }

    public bool Remove(TKey key)
    {
        var idx = IndexFor(key);
        var removed = _buckets[idx].RemoveFirst(e => Equals(e.Key, key));
        if (removed) _count--;
        return removed;
    }

    public void ForEach(Action<TKey, TValue> action)
    {
        for (int i = 0; i < _buckets.Length; i++)
        {
            var n = _buckets[i].Head;
            while (n != null)
            {
                action(n.Data.Key, n.Data.Value);
                n = n.Next;
            }
        }
    }

    public TKey[] Keys()
    {
        var keys = new TKey[_count];
        int k = 0;
        ForEach((key, _) => keys[k++] = key);
        return keys;
    }

    private void Rehash(int newCapacity)
    {
        var newBuckets = new LinkedList<Entry>[newCapacity];
        for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = new LinkedList<Entry>();

        ForEach((k, v) =>
        {
            var hc = k?.GetHashCode() ?? 0;
            hc &= 0x7fffffff;
            var idx = hc % newBuckets.Length;
            newBuckets[idx].Add(new Entry(k, v));
        });

        _buckets = newBuckets;
        // _count unchanged
    }
}
