using PersonalizedLearningPath.DTOs.LearningPath;

namespace PersonalizedLearningPath.Services.LearningPathEngine;

public interface IProgressService
{
    Task<ProgressDto> MarkVideoAsync(int userId, int courseId, int videoIndex, bool isWatched, CancellationToken ct = default);

    Task<CourseProgressDto> GetCourseProgressAsync(int userId, int courseId, CancellationToken ct = default);

    Task<CourseResumeDto> UpsertCourseResumeAsync(int userId, int courseId, int lastVideoIndex, int lastPositionSeconds, CancellationToken ct = default);

    Task<CourseResumeDto> GetCourseResumeAsync(int userId, int courseId, CancellationToken ct = default);

    Task<List<InProgressCourseDto>> GetInProgressCoursesAsync(int userId, CancellationToken ct = default);
}
