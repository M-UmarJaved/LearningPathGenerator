namespace PersonalizedLearningPath.DTOs.StudyPlan;

public class UpdateStudyPlanEventDto
{
    public string Title { get; set; } = null!;

    public string Category { get; set; } = "Study";

    public string? Notes { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public int? SkillId { get; set; }

    public int? CourseId { get; set; }

    public bool IsCompleted { get; set; }
}
