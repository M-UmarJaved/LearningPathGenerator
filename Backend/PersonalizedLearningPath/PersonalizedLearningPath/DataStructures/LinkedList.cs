using System;

namespace PersonalizedLearningPath.DataStructures
{
    public class LinkedListNode<T>
    {
        public T Data;
        public LinkedListNode<T>? Next;

        public LinkedListNode(T data)
        {
            Data = data;
            Next = null;
        }
    }

    public class LinkedList<T>
    {
        public LinkedListNode<T>? Head;

        public void Add(T data)
        {
            var node = new LinkedListNode<T>(data);
            if (Head == null)
            {
                Head = node;
                return;
            }

            var temp = Head;
            while (temp.Next != null)
                temp = temp.Next;

            temp.Next = node;
        }

        public bool RemoveFirst(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            LinkedListNode<T>? prev = null;
            var cur = Head;
            while (cur != null)
            {
                if (predicate(cur.Data))
                {
                    if (prev == null)
                    {
                        Head = cur.Next;
                    }
                    else
                    {
                        prev.Next = cur.Next;
                    }

                    return true;
                }

                prev = cur;
                cur = cur.Next;
            }

            return false;
        }
    }
}
