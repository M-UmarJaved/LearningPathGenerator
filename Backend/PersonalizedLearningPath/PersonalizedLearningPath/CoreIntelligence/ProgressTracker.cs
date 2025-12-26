using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Data;

namespace PersonalizedLearningPath.CoreIntelligence;

public static class ProgressTracker
{
    public static async Task<int> CalculateCourseCompletionAsync(AppDbContext db, int userId, int courseId, CancellationToken ct)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId, ct);
        if (course == null || course.TotalVideos <= 0) return 0;

        var watchedCount = await db.UserVideoProgress
            .Where(p => p.UserId == userId && p.CourseId == courseId && p.IsWatched)
            .Select(p => p.VideoIndex)
            .Distinct()
            .CountAsync(ct);

        var percent = (int)Math.Floor((watchedCount * 100.0) / course.TotalVideos);
        if (percent < 0) percent = 0;
        if (percent > 100) percent = 100;
        return percent;
    }

    public static int CalculateSkillCompletionFromCourses(IEnumerable<int> coursePercentages)
    {
        var list = coursePercentages.ToList();
        if (list.Count == 0) return 0;
        var avg = list.Sum() / list.Count;
        if (avg < 0) avg = 0;
        if (avg > 100) avg = 100;
        return avg;
    }
}
