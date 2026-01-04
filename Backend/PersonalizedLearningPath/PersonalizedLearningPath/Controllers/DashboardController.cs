using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DTOs.Dashboard;

namespace PersonalizedLearningPath.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/dashboard/summary?userId=123
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int userId, CancellationToken ct)
    {
        if (userId <= 0) return BadRequest(new { message = "userId is required" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return NotFound(new { message = "User not found" });

        // Stats
        var coursesCompleted = await _db.LearningPathCourses
            .Where(lpc => lpc.LearningPath.UserId == userId && lpc.IsCompleted)
            .CountAsync(ct);

        var watchedVideos = await _db.UserVideoProgress
            .Where(p => p.UserId == userId && p.IsWatched)
            .CountAsync(ct);

        // Heuristic: count each watched video as ~10 minutes.
        var hoursLearned = (int)Math.Round((watchedVideos * 10.0) / 60.0);

        var skillsAcquired = await _db.UserSkillAssessments
            .Where(a => a.UserId == userId)
            .Select(a => a.SkillId)
            .Distinct()
            .CountAsync(ct);

        var unreadNotifications = await _db.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync(ct);

        // Current streak: not currently trackable (no timestamps for video progress).
        const int currentStreak = 0;

        // Learning paths (all paths for user)
        var paths = await _db.LearningPaths
            .Include(lp => lp.Skill)
            .Include(lp => lp.LearningPathCourses)
            .Where(lp => lp.UserId == userId)
            .OrderByDescending(lp => lp.Status == "Active")
            .ThenByDescending(lp => lp.CreatedAt)
            .ToListAsync(ct);

        var pathDtos = paths.Select((lp, idx) =>
        {
            var courseCount = lp.LearningPathCourses.Count;
            var progress = courseCount == 0
                ? 0
                : (int)Math.Round(lp.LearningPathCourses.Average(x => x.CompletionPercentage));

            return new DashboardLearningPathDto
            {
                Id = lp.PathId,
                Name = lp.Skill?.SkillName ?? "Learning Path",
                Description = lp.Skill?.Description ?? "Personalized learning path",
                Courses = courseCount,
                Progress = Math.Clamp(progress, 0, 100),
                EstimatedTime = courseCount == 1 ? "1 course" : $"{courseCount} courses",
                Icon = PickIcon(idx)
            };
        }).ToList();

        // Active courses: courses in user's active paths that are not completed
        var activeCourses = await (
            from lpc in _db.LearningPathCourses
            join lp in _db.LearningPaths on lpc.PathId equals lp.PathId
            join c in _db.Courses on lpc.CourseId equals c.CourseId
            join s in _db.Skills on c.SkillId equals s.SkillId
            join cp0 in _db.CourseProfiles on c.CourseId equals cp0.CourseId into cpj
            from cp in cpj.DefaultIfEmpty()
            join i0 in _db.Instructors on cp.InstructorId equals i0.InstructorId into ij
            from i in ij.DefaultIfEmpty()
            where lp.UserId == userId && lp.Status == "Active" && !lpc.IsCompleted
            orderby lpc.CompletionPercentage descending, c.SequenceOrder, c.CourseId
            select new DashboardCourseDto
            {
                Id = c.CourseId,
                SkillId = c.SkillId,
                Title = c.CourseTitle,
                Instructor = i != null ? i.FullName : "Instructor",
                Progress = Math.Clamp(lpc.CompletionPercentage, 0, 100),
                Duration = EstimateDuration(cp != null && cp.EstimatedMinutes.HasValue ? cp.EstimatedMinutes.Value : (int?)null, c.TotalVideos),
                Thumbnail = cp != null && cp.ThumbnailUrl != null && cp.ThumbnailUrl != "" ? cp.ThumbnailUrl : PickThumbnail(c.CourseId),
                Difficulty = string.IsNullOrWhiteSpace(c.CourseLevel) ? "Beginner" : c.CourseLevel,
                Category = cp != null && cp.Category != null && cp.Category != "" ? cp.Category : s.SkillName,
                Rating = cp != null ? (double)cp.Rating : 0,
                Enrolled = cp != null ? cp.EnrolledCount : 0
            }
        ).Take(6).ToListAsync(ct);

        // Recommended courses: pick beginner courses for assessed skills not already active, fallback to any.
        var activeCourseIds = activeCourses.Select(x => x.Id).Distinct().ToHashSet();

        var assessedSkillIds = await _db.UserSkillAssessments
            .Where(a => a.UserId == userId)
            .Select(a => a.SkillId)
            .Distinct()
            .ToListAsync(ct);

        var recommendedDtos = await (
            from c in _db.Courses
            join s in _db.Skills on c.SkillId equals s.SkillId
            join cp0 in _db.CourseProfiles on c.CourseId equals cp0.CourseId into cpj
            from cp in cpj.DefaultIfEmpty()
            join i0 in _db.Instructors on cp.InstructorId equals i0.InstructorId into ij
            from i in ij.DefaultIfEmpty()
            where (assessedSkillIds.Count == 0 || assessedSkillIds.Contains(c.SkillId))
            orderby c.CourseLevel, c.SequenceOrder, c.CourseId
            select new DashboardCourseDto
            {
                Id = c.CourseId,
                SkillId = c.SkillId,
                Title = c.CourseTitle,
                Instructor = i != null ? i.FullName : "Instructor",
                Progress = 0,
                Duration = EstimateDuration(cp != null && cp.EstimatedMinutes.HasValue ? cp.EstimatedMinutes.Value : (int?)null, c.TotalVideos),
                Thumbnail = cp != null && cp.ThumbnailUrl != null && cp.ThumbnailUrl != "" ? cp.ThumbnailUrl : PickThumbnail(c.CourseId),
                Difficulty = string.IsNullOrWhiteSpace(c.CourseLevel) ? "Beginner" : c.CourseLevel,
                Category = cp != null && cp.Category != null && cp.Category != "" ? cp.Category : s.SkillName,
                Rating = cp != null ? (double)cp.Rating : 0,
                Enrolled = cp != null ? cp.EnrolledCount : 0
            }
        )
        .Where(dto => !activeCourseIds.Contains(dto.Id))
        .Take(6)
        .ToListAsync(ct);

        var recent = await _db.UserActivities
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new DashboardActivityDto
            {
                Action = a.Action,
                Course = a.Label,
                Time = ToRelativeTime(a.CreatedAt)
            })
            .ToListAsync(ct);

        var dto = new DashboardSummaryDto
        {
            UserName = user.FullName ?? user.Email ?? "Student",
            NotificationCount = unreadNotifications,
            Stats = new DashboardStatsDto
            {
                CoursesCompleted = coursesCompleted,
                HoursLearned = hoursLearned,
                CurrentStreak = currentStreak,
                SkillsAcquired = skillsAcquired
            },
            LearningPaths = pathDtos,
            ActiveCourses = activeCourses,
            RecommendedCourses = recommendedDtos,
            RecentActivity = recent
        };

        return Ok(dto);
    }

    private static string EstimateDuration(int? estimatedMinutes, int totalVideos)
    {
        var minutes = estimatedMinutes ?? (totalVideos > 0 ? totalVideos * 10 : 0);
        if (minutes <= 0) return "";
        var hours = Math.Max(1, (int)Math.Round(minutes / 60.0));
        return hours == 1 ? "1 hour" : $"{hours} hours";
    }

    private static string PickIcon(int idx)
    {
        var icons = new[] { "🧩", "💻", "🤖" };
        return icons[Math.Abs(idx) % icons.Length];
    }

    private static string PickThumbnail(int seed)
    {
        // Reuse a small stable set of public placeholder images.
        var urls = new[]
        {
            "https://images.unsplash.com/photo-1516116216624-53e697fedbea?w=400",
            "https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=400",
            "https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?w=400",
            "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=400",
            "https://images.unsplash.com/photo-1627398242454-45a1465c2479?w=400",
            "https://images.unsplash.com/photo-1581291518633-83b4ebd1d83e?w=400"
        };

        return urls[Math.Abs(seed) % urls.Length];
    }

    private static string ToRelativeTime(DateTime when)
    {
        var now = DateTime.Now;
        var delta = now - when;

        if (delta.TotalMinutes < 1) return "just now";
        if (delta.TotalMinutes < 60) return $"{(int)delta.TotalMinutes} minutes ago";
        if (delta.TotalHours < 24) return $"{(int)delta.TotalHours} hours ago";
        if (delta.TotalDays < 7) return $"{(int)delta.TotalDays} days ago";

        var weeks = (int)Math.Floor(delta.TotalDays / 7);
        return weeks == 1 ? "1 week ago" : $"{weeks} weeks ago";
    }
}
