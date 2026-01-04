
namespace PersonalizedLearningPath.DataStructures
{
    public class HashTable<TKey, TValue>
    {
        private const int SIZE = 100;
        private LinkedList<KeyValue<TKey, TValue>>[] table;

        public HashTable()
        {
            table = new LinkedList<KeyValue<TKey, TValue>>[SIZE];
        }

        private int Hash(TKey key)
        {
            return key.GetHashCode() % SIZE;
        }

        public void Add(TKey key, TValue value)
        {
            int index = Hash(key);
            if (table[index] == null)
                table[index] = new LinkedList<KeyValue<TKey, TValue>>();

            table[index].Add(new KeyValue<TKey, TValue>(key, value));
        }

        public TValue? Get(TKey key)
        {
            int index = Hash(key);
            var list = table[index];
            if (list == null) return default;

            var node = list.Head;
            while (node != null)
            {
                if (node.Data.Key.Equals(key))
                    return node.Data.Value;
                node = node.Next;
            }
            return default;
        }
    }

    public class KeyValue<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public KeyValue(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
