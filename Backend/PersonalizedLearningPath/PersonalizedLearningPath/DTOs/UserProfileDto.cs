namespace PersonalizedLearningPath.DTOs;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Summary metrics
    public int LearningPaths { get; set; }
    public int SkillsAssessed { get; set; }
    public int CoursesInPaths { get; set; }
    public int CompletedCourses { get; set; }
    public int VideosWatched { get; set; }
    public int HoursLearned { get; set; }
}
