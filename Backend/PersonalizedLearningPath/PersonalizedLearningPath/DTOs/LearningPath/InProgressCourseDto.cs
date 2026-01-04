namespace PersonalizedLearningPath.DTOs.LearningPath;

public class InProgressCourseDto
{
    public int CourseId { get; set; }
    public int SkillId { get; set; }

    public string SkillName { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;
    public string CourseLevel { get; set; } = string.Empty;

    public int CompletionPercentage { get; set; }

    public int LastVideoIndex { get; set; }
    public int LastPositionSeconds { get; set; }

    public DateTime? LastResumeAt { get; set; }
}
