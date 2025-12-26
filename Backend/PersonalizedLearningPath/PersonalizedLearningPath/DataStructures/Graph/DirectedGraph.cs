using PersonalizedLearningPath.DataStructures;

namespace PersonalizedLearningPath.DataStructures.Graph
{
    // Core DSA graph. IMPORTANT: Do not use List/Dictionary/HashSet/LINQ here.
    public sealed class DirectedGraph
    {
        private readonly LinkedList<GraphNode> _nodes = new();
        private readonly HashTable<int, GraphNode> _byId = new();
        private int _visitToken = 1;
        private int _count;

        public int Count => _count;

        public LinkedListNode<GraphNode>? NodesHead => _nodes.Head;

        public bool ContainsNode(int id)
        {
            return _byId.Get(id) != null;
        }

        public GraphNode? GetNode(int id)
        {
            return _byId.Get(id);
        }

        public void AddNode(int id, string name)
        {
            if (ContainsNode(id)) return;

            var node = new GraphNode(id, name);
            _nodes.Add(node);
            _byId.Add(id, node);
            _count++;
        }

        public bool HasEdge(int fromId, int toId)
        {
            var from = _byId.Get(fromId);
            if (from == null) return false;

            var e = from.Outgoing.Head;
            while (e != null)
            {
                if (e.Data.ToId == toId) return true;
                e = e.Next;
            }

            return false;
        }

        // Adds an edge (fromId -> toId) only if it doesn't create a cycle.
        public bool TryAddEdgeAcyclic(int fromId, int toId)
        {
            if (fromId == toId) return false;

            var from = _byId.Get(fromId);
            var to = _byId.Get(toId);
            if (from == null || to == null) return false;

            if (HasEdge(fromId, toId)) return true; // idempotent

            // Cycle check: if there is already a path toId -> fromId, adding fromId -> toId would create a cycle.
            if (IsReachable(toId, fromId)) return false;

            from.Outgoing.Add(new GraphEdge(toId));
            return true;
        }

        // Reachability using iterative DFS and node visit marks.
        private bool IsReachable(int startId, int targetId)
        {
            var start = _byId.Get(startId);
            if (start == null) return false;

            var token = NextVisitToken();
            start.VisitMark = token;

            // Stack implemented via custom LinkedList
            var stack = new LinkedList<GraphNode>();
            stack.Add(start);

            while (stack.Head != null)
            {
                // Pop last: O(n) due to singly linked list, but acceptable for lab-scale graphs.
                var current = PopLast(stack);
                if (current.Id == targetId) return true;

                var edge = current.Outgoing.Head;
                while (edge != null)
                {
                    var next = _byId.Get(edge.Data.ToId);
                    if (next != null && next.VisitMark != token)
                    {
                        next.VisitMark = token;
                        stack.Add(next);
                    }
                    edge = edge.Next;
                }
            }

            return false;
        }

        // Deterministic topological order (Kahn). Returns false if a cycle is detected.
        public bool TryTopologicalOrder(out LinkedList<int> order)
        {
            order = new LinkedList<int>();
            if (_count == 0) return true;

            ResetInDegrees();

            var queue = new SimpleQueue<GraphNode>();
            var n = _nodes.Head;
            while (n != null)
            {
                if (n.Data.InDegree == 0) queue.Enqueue(n.Data);
                n = n.Next;
            }

            int produced = 0;
            while (!queue.IsEmpty)
            {
                var node = queue.Dequeue();
                order.Add(node.Id);
                produced++;

                var edge = node.Outgoing.Head;
                while (edge != null)
                {
                    var to = _byId.Get(edge.Data.ToId);
                    if (to != null)
                    {
                        to.InDegree--;
                        if (to.InDegree == 0)
                        {
                            queue.Enqueue(to);
                        }
                    }
                    edge = edge.Next;
                }
            }

            return produced == _count;
        }

        private void ResetInDegrees()
        {
            var n = _nodes.Head;
            while (n != null)
            {
                n.Data.InDegree = 0;
                n = n.Next;
            }

            n = _nodes.Head;
            while (n != null)
            {
                var edge = n.Data.Outgoing.Head;
                while (edge != null)
                {
                    var to = _byId.Get(edge.Data.ToId);
                    if (to != null) to.InDegree++;
                    edge = edge.Next;
                }
                n = n.Next;
            }
        }

        private int NextVisitToken()
        {
            // Avoid overflow causing token collisions
            _visitToken++;
            if (_visitToken == int.MaxValue) _visitToken = 1;
            return _visitToken;
        }

        private static GraphNode PopLast(LinkedList<GraphNode> list)
        {
            var head = list.Head;
            if (head == null) throw new InvalidOperationException("Stack underflow");

            if (head.Next == null)
            {
                var only = head.Data;
                list.Head = null;
                return only;
            }

            var prev = head;
            var cur = head.Next;
            while (cur.Next != null)
            {
                prev = cur;
                cur = cur.Next;
            }

            prev.Next = null;
            return cur.Data;
        }

        // Minimal queue using custom LinkedList nodes.
        private sealed class SimpleQueue<T>
        {
            private LinkedListNode<T>? _head;
            private LinkedListNode<T>? _tail;

            public bool IsEmpty => _head == null;

            public void Enqueue(T item)
            {
                var node = new LinkedListNode<T>(item);
                if (_tail == null)
                {
                    _head = node;
                    _tail = node;
                    return;
                }

                _tail.Next = node;
                _tail = node;
            }

            public T Dequeue()
            {
                if (_head == null) throw new InvalidOperationException("Queue underflow");
                var data = _head.Data;
                _head = _head.Next;
                if (_head == null) _tail = null;
                return data;
            }
        }
    }
}
