using PersonalizedLearningPath.DataStructures;

namespace PersonalizedLearningPath.DataStructures.Graph
{
    public sealed class GraphEdge
    {
        public int ToId { get; }

        public GraphEdge(int toId)
        {
            ToId = toId;
        }
    }

    public sealed class GraphNode
    {
        public int Id { get; }
        public string Name { get; }

        // Adjacency list: outgoing edges (Id -> ToId)
        public LinkedList<GraphEdge> Outgoing { get; } = new();

        // Used by core traversal algorithms (no HashSet/Dictionary)
        internal int VisitMark { get; set; }
        internal int InDegree { get; set; }

        public GraphNode(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
