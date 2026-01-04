namespace PersonalizedLearningPath.DTOs.SkillAssessment
{
        public class AnswerDto
        {
            public int UserId { get; set; }
            public int SkillId { get; set; }
            public int CurrentIndex { get; set; }
            public string? SelectedOption { get; set; }

            public int CorrectCount { get; set; }
            public int TotalCount { get; set; }
        }
    
}
