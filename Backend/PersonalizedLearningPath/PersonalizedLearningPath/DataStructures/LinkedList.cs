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
    }
}
