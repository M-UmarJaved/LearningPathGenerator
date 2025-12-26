namespace PersonalizedLearningPath.Models;

using System.Collections.Generic;

public class User
{
    public int Id { get; set; }

    // Required fields - initialized to satisfy non-nullable reference types
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    // Password reset (token is stored as a hash for safety)
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    // Navigation
    public ICollection<UserSkillAssessment> UserSkillAssessments { get; set; } = new List<UserSkillAssessment>();

    public ICollection<LearningPath> LearningPaths { get; set; } = new List<LearningPath>();
    public ICollection<UserVideoProgress> UserVideoProgress { get; set; } = new List<UserVideoProgress>();
    public ICollection<UserSkillHistory> UserSkillHistory { get; set; } = new List<UserSkillHistory>();
}
