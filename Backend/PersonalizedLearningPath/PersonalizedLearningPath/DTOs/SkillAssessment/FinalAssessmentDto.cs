namespace PersonalizedLearningPath.DTOs.SkillAssessment
{
    public class FinalAssessmentDto
    {
        public bool Completed { get; set; }
        public QuestionDto? NextQuestion { get; set; }
        public string? SkillLevel { get; set; }

        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
    }
}
