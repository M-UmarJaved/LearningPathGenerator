using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.CoreIntelligence;

public static class LearningPathBuilder
{
    private static int LevelRank(string level)
    {
        return level.Trim().ToLowerInvariant() switch
        {
            "beginner" => 0,
            "intermediate" => 1,
            "advanced" => 2,
            _ => 99
        };
    }

    public static List<Course> OrderCourses(IEnumerable<Course> courses)
    {
        // Deterministic ordering: Beginner -> Intermediate -> Advanced, then SequenceOrder.
        return courses
            .OrderBy(c => LevelRank(c.CourseLevel))
            .ThenBy(c => c.SequenceOrder)
            .ThenBy(c => c.CourseId)
            .ToList();
    }

    public static List<int> BuildRoadmapCourseIds(List<Course> orderedCourses)
    {
        var graph = new Graph();

        for (var i = 0; i < orderedCourses.Count; i++)
        {
            graph.AddNode(orderedCourses[i].CourseId);
            if (i > 0)
            {
                graph.AddEdge(orderedCourses[i - 1].CourseId, orderedCourses[i].CourseId);
            }
        }

        var start = graph.GetStartNode();
        if (start == null) return new List<int>();

        return graph.BfsOrderFrom(start.Value);
    }
}
