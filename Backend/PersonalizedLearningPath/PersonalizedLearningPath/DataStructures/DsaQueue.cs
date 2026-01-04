namespace PersonalizedLearningPath.DataStructures;

// Manual FIFO queue implemented using linked nodes.
// Intentionally avoids System.Collections.Generic.Queue<T>.
public sealed class DsaQueue<T>
{
    private LinkedListNode<T>? _head;
    private LinkedListNode<T>? _tail;
    private int _count;

    public int Count => _count;

    public bool IsEmpty => _head == null;

    public void Enqueue(T item)
    {
        var node = new LinkedListNode<T>(item);
        if (_tail == null)
        {
            _head = node;
            _tail = node;
            _count = 1;
            return;
        }

        _tail.Next = node;
        _tail = node;
        _count++;
    }

    public T Dequeue()
    {
        if (_head == null) throw new InvalidOperationException("Queue underflow");

        var data = _head.Data;
        _head = _head.Next;
        if (_head == null) _tail = null;

        _count--;
        return data;
    }

    public T Peek()
    {
        if (_head == null) throw new InvalidOperationException("Queue underflow");
        return _head.Data;
    }
}
