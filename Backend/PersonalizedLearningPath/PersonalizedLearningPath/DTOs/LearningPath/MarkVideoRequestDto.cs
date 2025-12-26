namespace PersonalizedLearningPath.DTOs.LearningPath;

public class MarkVideoRequestDto
{
    public int UserId { get; set; }
    public int CourseId { get; set; }

    // 1..TotalVideos
    public int VideoIndex { get; set; }

    public bool IsWatched { get; set; }
}
