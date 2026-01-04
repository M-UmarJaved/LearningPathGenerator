namespace PersonalizedLearningPath.DTOs.StudyPlan;

public class StudyPlanEventDto
{
    public int StudyPlanEventId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Category { get; set; } = "Study";

    public string? Notes { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public int? SkillId { get; set; }

    public int? CourseId { get; set; }

    public bool IsCompleted { get; set; }
}
