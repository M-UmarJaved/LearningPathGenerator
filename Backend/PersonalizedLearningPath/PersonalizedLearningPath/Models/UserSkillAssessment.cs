namespace PersonalizedLearningPath.Models;

using System;

public class UserSkillAssessment
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int SkillId { get; set; }

    public int CorrectAnswers { get; set; }
    public int TotalAnswered { get; set; }

    public string? SkillLevel { get; set; } // Beginner / Intermediate / Advanced

    public DateTime CompletedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
