namespace PersonalizedLearningPath.Models;

public class UserCourseEnrollment
{
    public int EnrollmentId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;

    public Course Course { get; set; } = null!;
}
