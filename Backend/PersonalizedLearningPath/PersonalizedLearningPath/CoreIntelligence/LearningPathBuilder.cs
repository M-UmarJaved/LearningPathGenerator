using PersonalizedLearningPath.Models;
using PersonalizedLearningPath.DataStructures.Sorting;

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
        var arr = courses.ToArray();
        MergeSort.Sort(arr, (a, b) =>
        {
            var r = LevelRank(a.CourseLevel).CompareTo(LevelRank(b.CourseLevel));
            if (r != 0) return r;

            r = a.SequenceOrder.CompareTo(b.SequenceOrder);
            if (r != 0) return r;

            return a.CourseId.CompareTo(b.CourseId);
        });

        return arr.ToList();
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
