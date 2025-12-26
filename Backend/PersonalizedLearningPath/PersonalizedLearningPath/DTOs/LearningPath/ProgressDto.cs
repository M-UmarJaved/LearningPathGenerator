namespace PersonalizedLearningPath.DTOs.LearningPath;

public class ProgressDto
{
    public int PathId { get; set; }
    public int SkillId { get; set; }

    public int CourseId { get; set; }
    public int CourseCompletionPercentage { get; set; }
    public bool CourseCompleted { get; set; }

    public int SkillCompletionPercentage { get; set; }
    public string PathStatus { get; set; } = string.Empty;

    public int? NextCourseId { get; set; }
}
