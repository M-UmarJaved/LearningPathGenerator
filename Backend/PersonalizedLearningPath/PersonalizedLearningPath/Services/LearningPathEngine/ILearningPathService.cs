using PersonalizedLearningPath.DTOs.LearningPath;

namespace PersonalizedLearningPath.Services.LearningPathEngine;

public interface ILearningPathService
{
    Task<LearningPathDto> GenerateOrGetActiveAsync(int userId, int skillId, CancellationToken ct = default);
    Task<LearningPathDto> GetByIdAsync(int pathId, CancellationToken ct = default);
}
