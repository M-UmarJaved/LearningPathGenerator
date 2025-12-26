namespace PersonalizedLearningPath.DTOs.LearningPath;

public class CourseProgressDto
{
    public int UserId { get; set; }
    public int CourseId { get; set; }

    public int TotalVideos { get; set; }
    public int CompletionPercentage { get; set; }

    public List<int> WatchedVideoIndexes { get; set; } = new();
}
