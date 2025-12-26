namespace PersonalizedLearningPath.DTOs.LearningPath;

public class CourseDetailsDto
{
    public int CourseId { get; set; }
    public int SkillId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;
    public string CourseLevel { get; set; } = string.Empty;

    public int SequenceOrder { get; set; }

    public int TotalVideos { get; set; }

    public List<CourseVideoDto> Videos { get; set; } = new();
}
