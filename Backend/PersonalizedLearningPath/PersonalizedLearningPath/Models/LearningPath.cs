namespace PersonalizedLearningPath.Models;

public class LearningPath
{
    public int PathId { get; set; }

    public int UserId { get; set; }
    public int SkillId { get; set; }

    public DateTime CreatedAt { get; set; }

    // Active / Completed
    public string Status { get; set; } = "Active";

    public User User { get; set; } = null!;
    public Skill Skill { get; set; } = null!;

    public ICollection<LearningPathCourse> LearningPathCourses { get; set; } = new List<LearningPathCourse>();
}
