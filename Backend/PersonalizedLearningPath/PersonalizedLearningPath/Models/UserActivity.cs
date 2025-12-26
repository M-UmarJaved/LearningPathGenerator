namespace PersonalizedLearningPath.Models;

public class UserActivity
{
    public int ActivityId { get; set; }

    public int UserId { get; set; }

    public string Action { get; set; } = null!;

    public string Label { get; set; } = null!;

    public int? CourseId { get; set; }
    public int? SkillId { get; set; }
    public int? PathId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public Course? Course { get; set; }
    public Skill? Skill { get; set; }
    public LearningPath? LearningPath { get; set; }
}
