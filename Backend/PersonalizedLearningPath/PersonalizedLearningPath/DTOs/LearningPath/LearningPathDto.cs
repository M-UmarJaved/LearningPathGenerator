namespace PersonalizedLearningPath.DTOs.LearningPath;

public class LearningPathDto
{
    public int PathId { get; set; }
    public int UserId { get; set; }
    public int SkillId { get; set; }

    public string SkillName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;

    public int SkillCompletionPercentage { get; set; }
    public int? ActiveCourseId { get; set; }

    public List<CourseDto> Courses { get; set; } = new();
}
