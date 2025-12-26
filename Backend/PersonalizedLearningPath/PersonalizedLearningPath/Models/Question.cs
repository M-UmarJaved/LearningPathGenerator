namespace PersonalizedLearningPath.Models;

public class Question
{
    public int QuestionId { get; set; }

    // FK to Skill
    public int SkillId { get; set; }

    // Required fields
    public string QuestionText { get; set; } = null!;
    public string ChoiceA { get; set; } = null!;
    public string ChoiceB { get; set; } = null!;
    public string ChoiceC { get; set; } = null!;
    public string CorrectAnswer { get; set; } = null!;
    public string DifficultyLevel { get; set; } = null!;
    public int TreeIndex { get; set; }

    // Navigation
    public Skill Skill { get; set; } = null!;
}
