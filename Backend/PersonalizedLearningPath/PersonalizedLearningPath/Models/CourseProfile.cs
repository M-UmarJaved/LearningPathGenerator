namespace PersonalizedLearningPath.Models;

public class CourseProfile
{
    // 1:1 with Course (CourseId is both PK + FK)
    public int CourseId { get; set; }

    public int? InstructorId { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? Category { get; set; }

    // Optional override for estimated duration.
    public int? EstimatedMinutes { get; set; }

    public decimal Rating { get; set; }

    public int EnrolledCount { get; set; }

    public Course Course { get; set; } = null!;

    public Instructor? Instructor { get; set; }
}
