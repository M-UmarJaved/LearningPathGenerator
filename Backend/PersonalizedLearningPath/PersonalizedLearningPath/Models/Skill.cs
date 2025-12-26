namespace PersonalizedLearningPath.Models;

using System.Collections.Generic;

public class Skill
{
    public int SkillId { get; set; }

    // Required
    public string SkillName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<UserSkillAssessment> UserSkillAssessments { get; set; } = new List<UserSkillAssessment>();

    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<LearningPath> LearningPaths { get; set; } = new List<LearningPath>();
}
