using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DataStructures.Trees;
using PersonalizedLearningPath.DTOs.SkillAssessment;
using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.Services.SkillAssessment
{
    public class SkillAssessmentService : ISkillAssessmentService
    {
        private readonly AppDbContext _context;

        // Complete binary tree depth/height = 5 => nodes = 2^5 - 1 = 31.
        private const int QuestionsPerSkill = 31;
        private const int QuestionsToAskPerAssessment = 5;

        public SkillAssessmentService(AppDbContext context)
        {
            _context = context;
        }

        public BinaryQuestionTree BuildTree(int skillId)
        {
            var questions = _context.Questions
                .Where(q => q.SkillId == skillId)
                .OrderBy(q => q.TreeIndex)
                .Take(QuestionsPerSkill)
                .ToArray();

            // Ensure the skill has exactly 31 questions arranged as a complete binary tree (indices 0..30)
            if (questions.Length != QuestionsPerSkill)
            {
                throw new InvalidOperationException($"Skill {skillId} must have exactly {QuestionsPerSkill} questions (found {questions.Length}).");
            }

            for (int i = 0; i < QuestionsPerSkill; i++)
            {
                if (questions[i].TreeIndex != i)
                {
                    throw new InvalidOperationException($"Questions for skill {skillId} must have TreeIndex values 0..{QuestionsPerSkill - 1} in order. Expected index {i} at position {i}, found {questions[i].TreeIndex}.");
                }
            }

            return new BinaryQuestionTree(questions);
        }

        public QuestionDto StartAssessment(int skillId)
        {
            var tree = BuildTree(skillId);
            return Map(tree.GetRoot());
        }

        public FinalAssessmentDto SubmitAnswer(AnswerDto dto)
        {
            var tree = BuildTree(dto.SkillId);

            var current = tree.GetByTreeIndex(dto.CurrentIndex);
            if (current == null)
                throw new InvalidOperationException($"Current question with TreeIndex {dto.CurrentIndex} for skill {dto.SkillId} not found.");

            bool correct = string.Equals(current.CorrectAnswer, dto.SelectedOption, StringComparison.OrdinalIgnoreCase);

            int correctCount = dto.CorrectCount + (correct ? 1 : 0);
            int totalCount = dto.TotalCount + 1;

            // Hard stop: each assessment is exactly 5 answered questions per skill.
            // Even though the tree has 31 nodes, we only traverse 5 questions (root-to-leaf path).
            if (totalCount >= QuestionsToAskPerAssessment)
            {
                string level =
                    correctCount <= 2 ? "Beginner" :
                    correctCount <= 4 ? "Intermediate" :
                    "Advanced";

                var existing = _context.UserSkillAssessments
                    .OrderByDescending(x => x.CompletedAt)
                    .FirstOrDefault(x => x.UserId == dto.UserId && x.SkillId == dto.SkillId);

                if (existing == null)
                {
                    _context.UserSkillAssessments.Add(new UserSkillAssessment
                    {
                        UserId = dto.UserId,
                        SkillId = dto.SkillId,
                        CorrectAnswers = correctCount,
                        TotalAnswered = totalCount,
                        SkillLevel = level,
                        CompletedAt = DateTime.Now
                    });
                }
                else
                {
                    existing.CorrectAnswers = correctCount;
                    existing.TotalAnswered = totalCount;
                    existing.SkillLevel = level;
                    existing.CompletedAt = DateTime.Now;
                }

                var skillName = _context.Skills.Where(s => s.SkillId == dto.SkillId)
                    .Select(s => s.SkillName)
                    .FirstOrDefault() ?? $"Skill {dto.SkillId}";

                _context.UserActivities.Add(new UserActivity
                {
                    UserId = dto.UserId,
                    Action = "Completed assessment",
                    Label = skillName,
                    SkillId = dto.SkillId,
                    CreatedAt = DateTime.UtcNow
                });

                _context.UserNotifications.Add(new UserNotification
                {
                    UserId = dto.UserId,
                    Type = "Assessment",
                    Message = $"Assessment completed: {skillName} ({level})",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                _context.SaveChanges();

                return new FinalAssessmentDto
                {
                    Completed = true,
                    SkillLevel = level,
                    CorrectCount = correctCount,
                    TotalCount = totalCount,
                    NextQuestion = null
                };
            }

            var next = tree.GetNextByTreeIndex(dto.CurrentIndex, correct);

            if (next == null)
            {
                string level =
                    correctCount <= 2 ? "Beginner" :
                    correctCount <= 4 ? "Intermediate" :
                    "Advanced";

                var existing = _context.UserSkillAssessments
                    .OrderByDescending(x => x.CompletedAt)
                    .FirstOrDefault(x => x.UserId == dto.UserId && x.SkillId == dto.SkillId);

                if (existing == null)
                {
                    _context.UserSkillAssessments.Add(new UserSkillAssessment
                    {
                        UserId = dto.UserId,
                        SkillId = dto.SkillId,
                        CorrectAnswers = correctCount,
                        TotalAnswered = totalCount,
                        SkillLevel = level,
                        CompletedAt = DateTime.Now
                    });
                }
                else
                {
                    existing.CorrectAnswers = correctCount;
                    existing.TotalAnswered = totalCount;
                    existing.SkillLevel = level;
                    existing.CompletedAt = DateTime.Now;
                }

                var skillName = _context.Skills.Where(s => s.SkillId == dto.SkillId)
                    .Select(s => s.SkillName)
                    .FirstOrDefault() ?? $"Skill {dto.SkillId}";

                _context.UserActivities.Add(new UserActivity
                {
                    UserId = dto.UserId,
                    Action = "Completed assessment",
                    Label = skillName,
                    SkillId = dto.SkillId,
                    CreatedAt = DateTime.UtcNow
                });

                _context.UserNotifications.Add(new UserNotification
                {
                    UserId = dto.UserId,
                    Type = "Assessment",
                    Message = $"Assessment completed: {skillName} ({level})",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                _context.SaveChanges();

                return new FinalAssessmentDto
                {
                    Completed = true,
                    SkillLevel = level
                    ,
                    CorrectCount = correctCount,
                    TotalCount = totalCount,
                    NextQuestion = null
                };
            }

            return new FinalAssessmentDto
            {
                Completed = false,
                NextQuestion = Map(next),
                CorrectCount = correctCount,
                TotalCount = totalCount,
                SkillLevel = null
            };
        }

        private QuestionDto Map(Question q) => new QuestionDto
        {
            QuestionId = q.QuestionId,
            TreeIndex = q.TreeIndex,
            QuestionText = q.QuestionText,
            ChoiceA = q.ChoiceA,
            ChoiceB = q.ChoiceB,
            ChoiceC = q.ChoiceC
        };
    }

}
