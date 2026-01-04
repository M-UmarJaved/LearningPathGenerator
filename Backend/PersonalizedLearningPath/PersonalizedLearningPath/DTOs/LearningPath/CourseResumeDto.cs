namespace PersonalizedLearningPath.DTOs.LearningPath;

public class CourseResumeDto
{
    public int UserId { get; set; }
    public int CourseId { get; set; }

    public int LastVideoIndex { get; set; }
    public int LastPositionSeconds { get; set; }

    public DateTime UpdatedAt { get; set; }
}
