namespace PersonalizedLearningPath.Models;

public class Course
{
    public int CourseId { get; set; }

    public int SkillId { get; set; }

    public string CourseTitle { get; set; } = null!;

    // Beginner / Intermediate / Advanced
    public string CourseLevel { get; set; } = null!;

    public string YoutubeVideoUrl { get; set; } = null!;

    public int TotalVideos { get; set; }

    public int SequenceOrder { get; set; }

    public Skill Skill { get; set; } = null!;

    public ICollection<CourseVideo> Videos { get; set; } = new List<CourseVideo>();

    public ICollection<LearningPathCourse> LearningPathCourses { get; set; } = new List<LearningPathCourse>();
    public ICollection<UserVideoProgress> UserVideoProgress { get; set; } = new List<UserVideoProgress>();
}
