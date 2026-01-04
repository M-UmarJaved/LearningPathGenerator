namespace PersonalizedLearningPath.Models;

public class UserCourseResume
{
    public int ResumeId { get; set; }

    public int UserId { get; set; }
    public int CourseId { get; set; }

    // 1..TotalVideos
    public int LastVideoIndex { get; set; }

    // seconds into the video
    public int LastPositionSeconds { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
