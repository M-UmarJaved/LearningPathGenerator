using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DTOs;

namespace PersonalizedLearningPath.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/profile/me?userId=123
    [HttpGet("me")]
    public async Task<IActionResult> GetMe([FromQuery] int userId, CancellationToken ct)
    {
        if (userId <= 0)
            return BadRequest(new { message = "userId is required" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var learningPathsCount = await _db.LearningPaths
            .Where(lp => lp.UserId == userId)
            .CountAsync(ct);

        var skillsAssessedCount = await _db.UserSkillAssessments
            .Where(a => a.UserId == userId)
            .Select(a => a.SkillId)
            .Distinct()
            .CountAsync(ct);

        var coursesInPathsCount = await (
            from lpc in _db.LearningPathCourses
            join lp in _db.LearningPaths on lpc.PathId equals lp.PathId
            where lp.UserId == userId
            select lpc.CourseId
        ).Distinct().CountAsync(ct);

        var completedCoursesCount = await (
            from lpc in _db.LearningPathCourses
            join lp in _db.LearningPaths on lpc.PathId equals lp.PathId
            where lp.UserId == userId && lpc.IsCompleted
            select lpc.CourseId
        ).Distinct().CountAsync(ct);

        var videosWatched = await _db.UserVideoProgress
            .Where(p => p.UserId == userId && p.IsWatched)
            .CountAsync(ct);

        // Heuristic: ~10 minutes per video watched
        var hoursLearned = (int)Math.Round((videosWatched * 10.0) / 60.0);

        var dto = new UserProfileDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            LearningPaths = learningPathsCount,
            SkillsAssessed = skillsAssessedCount,
            CoursesInPaths = coursesInPathsCount,
            CompletedCourses = completedCoursesCount,
            VideosWatched = videosWatched,
            HoursLearned = hoursLearned
        };

        return Ok(dto);
    }
}
