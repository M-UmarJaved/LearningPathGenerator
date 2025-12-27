using PersonalizedLearningPath.DTOs.ProgressGraph;

namespace PersonalizedLearningPath.Services.ProgressGraph;

public interface IProgressGraphService
{
    Task<ProgressGraphResponseDto> BuildAsync(int userId, CancellationToken ct = default);
}
