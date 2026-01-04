namespace PersonalizedLearningPath.DataStructures;

// Manual LIFO stack implemented using linked nodes.
// Intentionally avoids System.Collections.Generic.Stack<T>.
public sealed class DsaStack<T>
{
    private LinkedListNode<T>? _top;
    private int _count;

    public int Count => _count;

    public bool IsEmpty => _top == null;

    public void Push(T item)
    {
        _top = new LinkedListNode<T>(item) { Next = _top };
        _count++;
    }

    public T Pop()
    {
        if (_top == null) throw new InvalidOperationException("Stack underflow");

        var value = _top.Data;
        _top = _top.Next;
        _count--;
        return value;
    }

    public T Peek()
    {
        if (_top == null) throw new InvalidOperationException("Stack underflow");
        return _top.Data;
    }
}
