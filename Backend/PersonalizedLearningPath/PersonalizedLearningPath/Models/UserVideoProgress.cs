namespace PersonalizedLearningPath.Models;

public class UserVideoProgress
{
    public int ProgressId { get; set; }

    public int UserId { get; set; }
    public int CourseId { get; set; }

    // 1..TotalVideos
    public int VideoIndex { get; set; }

    public bool IsWatched { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
