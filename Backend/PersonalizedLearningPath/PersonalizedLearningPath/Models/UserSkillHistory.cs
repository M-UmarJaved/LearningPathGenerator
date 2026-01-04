namespace PersonalizedLearningPath.Models;

public class UserSkillHistory
{
    public int HistoryId { get; set; }

    public int UserId { get; set; }
    public int SkillId { get; set; }

    public DateTime CompletedAt { get; set; }

    public User User { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
