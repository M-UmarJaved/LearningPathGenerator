namespace PersonalizedLearningPath.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public string UserName { get; set; } = "";
    public int NotificationCount { get; set; }

    public DashboardStatsDto Stats { get; set; } = new();

    public List<DashboardLearningPathDto> LearningPaths { get; set; } = new();
    public List<DashboardCourseDto> ActiveCourses { get; set; } = new();
    public List<DashboardCourseDto> RecommendedCourses { get; set; } = new();

    public List<DashboardActivityDto> RecentActivity { get; set; } = new();
}

public class DashboardStatsDto
{
    public int CoursesCompleted { get; set; }
    public int HoursLearned { get; set; }
    public int CurrentStreak { get; set; }
    public int SkillsAcquired { get; set; }
}

public class DashboardLearningPathDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Courses { get; set; }
    public int Progress { get; set; }
    public string EstimatedTime { get; set; } = "";
    public string Icon { get; set; } = "🧩";
}

public class DashboardCourseDto
{
    public int Id { get; set; }
    public int SkillId { get; set; }

    public string Title { get; set; } = "";
    public string Instructor { get; set; } = "";
    public int Progress { get; set; }
    public string Duration { get; set; } = "";
    public string Thumbnail { get; set; } = "";

    public string Difficulty { get; set; } = "Beginner";
    public string Category { get; set; } = "";

    public double Rating { get; set; }
    public int Enrolled { get; set; }
}

public class DashboardActivityDto
{
    public string Action { get; set; } = "";
    public string Course { get; set; } = "";
    public string Time { get; set; } = "";
}
