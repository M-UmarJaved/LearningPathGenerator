using PersonalizedLearningPath.DTOs.LearningPath;

namespace PersonalizedLearningPath.Services.LearningPathEngine;

public interface IProgressService
{
    Task<ProgressDto> MarkVideoAsync(int userId, int courseId, int videoIndex, bool isWatched, CancellationToken ct = default);

    Task<CourseProgressDto> GetCourseProgressAsync(int userId, int courseId, CancellationToken ct = default);
}
