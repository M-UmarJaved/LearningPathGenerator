namespace PersonalizedLearningPath.Models;

public class StudyPlanEvent
{
    public int StudyPlanEventId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Category { get; set; } = "Study";

    public string? Notes { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    // Optional integration with existing domain
    public int? SkillId { get; set; }

    public int? CourseId { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    public Skill? Skill { get; set; }

    public Course? Course { get; set; }
}
