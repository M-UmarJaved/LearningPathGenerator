namespace PersonalizedLearningPath.Models;

public class LearningPathCourse
{
    public int PathCourseId { get; set; }

    public int PathId { get; set; }
    public int CourseId { get; set; }

    public bool IsCompleted { get; set; }

    // 0..100
    public int CompletionPercentage { get; set; }

    public LearningPath LearningPath { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
