namespace PersonalizedLearningPath.Models;

public class Instructor
{
    public int InstructorId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Title { get; set; }

    public ICollection<CourseProfile> CourseProfiles { get; set; } = new List<CourseProfile>();
}
