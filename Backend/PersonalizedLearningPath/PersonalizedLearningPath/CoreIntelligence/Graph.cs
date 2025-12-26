namespace PersonalizedLearningPath.CoreIntelligence;

public class Graph
{
    private readonly Dictionary<int, GraphNode> _nodes = new();
    private readonly Dictionary<int, int> _inDegree = new();

    public void AddNode(int courseId)
    {
        if (_nodes.ContainsKey(courseId)) return;
        _nodes[courseId] = new GraphNode(courseId);
        _inDegree[courseId] = 0;
    }

    public void AddEdge(int fromCourseId, int toCourseId)
    {
        AddNode(fromCourseId);
        AddNode(toCourseId);

        var node = _nodes[fromCourseId];
        if (node.Next.Contains(toCourseId)) return;

        node.Next.Add(toCourseId);
        _inDegree[toCourseId] = _inDegree[toCourseId] + 1;
    }

    public int? GetStartNode()
    {
        foreach (var kv in _inDegree)
        {
            if (kv.Value == 0) return kv.Key;
        }
        return null;
    }

    public List<int> BfsOrderFrom(int startCourseId)
    {
        var result = new List<int>();
        var visited = new HashSet<int>();
        var q = new Queue<int>();

        q.Enqueue(startCourseId);
        visited.Add(startCourseId);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            result.Add(cur);

            if (!_nodes.TryGetValue(cur, out var node)) continue;

            foreach (var nxt in node.Next)
            {
                if (visited.Add(nxt))
                {
                    q.Enqueue(nxt);
                }
            }
        }

        return result;
    }
}
