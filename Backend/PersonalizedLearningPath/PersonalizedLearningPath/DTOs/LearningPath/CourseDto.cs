namespace PersonalizedLearningPath.DTOs.LearningPath;

public class CourseDto
{
    public int CourseId { get; set; }
    public int SkillId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;
    public string CourseLevel { get; set; } = string.Empty;
    public string YoutubeVideoUrl { get; set; } = string.Empty;

    public int TotalVideos { get; set; }
    public int SequenceOrder { get; set; }

    public bool IsCompleted { get; set; }
    public int CompletionPercentage { get; set; }
}
