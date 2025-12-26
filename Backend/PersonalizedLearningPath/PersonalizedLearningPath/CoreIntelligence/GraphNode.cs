namespace PersonalizedLearningPath.CoreIntelligence;

public class GraphNode
{
    public int CourseId { get; }

    // Adjacency list: next course IDs
    public List<int> Next { get; } = new();

    public GraphNode(int courseId)
    {
        CourseId = courseId;
    }
}
