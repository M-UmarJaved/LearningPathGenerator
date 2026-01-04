namespace PersonalizedLearningPath.DataStructures;

// Manual hash-set built on top of DsaMap.
// Intentionally avoids System.Collections.Generic.HashSet<T>.
public sealed class DsaHashSet<T>
{
    private readonly DsaMap<T, byte> _map;

    public int Count => _map.Count;

    public DsaHashSet(int initialCapacity = 101)
    {
        _map = new DsaMap<T, byte>(initialCapacity);
    }

    public bool Add(T item)
    {
        if (_map.ContainsKey(item)) return false;
        _map.Set(item, 1);
        return true;
    }

    public bool Contains(T item) => _map.ContainsKey(item);

    public bool Remove(T item) => _map.Remove(item);
}
