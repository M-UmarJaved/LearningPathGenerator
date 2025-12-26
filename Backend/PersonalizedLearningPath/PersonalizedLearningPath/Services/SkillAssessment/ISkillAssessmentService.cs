using PersonalizedLearningPath.DataStructures.Trees;
using PersonalizedLearningPath.DTOs.SkillAssessment;
using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.Services.SkillAssessment
{
    public interface ISkillAssessmentService
    {
        QuestionDto StartAssessment(int skillId);
        FinalAssessmentDto SubmitAnswer(AnswerDto dto);
        BinaryQuestionTree BuildTree(int skillId);
    }

}
