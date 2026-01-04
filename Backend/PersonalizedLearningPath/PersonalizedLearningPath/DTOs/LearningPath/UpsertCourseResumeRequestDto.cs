namespace PersonalizedLearningPath.DTOs.LearningPath;

public class UpsertCourseResumeRequestDto
{
    public int UserId { get; set; }
    public int CourseId { get; set; }

    // 1..TotalVideos
    public int LastVideoIndex { get; set; }

    // seconds
    public int LastPositionSeconds { get; set; }
}
