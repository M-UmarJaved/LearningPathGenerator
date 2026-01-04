namespace PersonalizedLearningPath.DTOs.SkillAssessment
{
    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public string? QuestionText { get; set; }
        public string? ChoiceA { get; set; }
        public string? ChoiceB { get; set; }
        public string? ChoiceC { get; set; }
        public int TreeIndex { get; set; }
    }

}
