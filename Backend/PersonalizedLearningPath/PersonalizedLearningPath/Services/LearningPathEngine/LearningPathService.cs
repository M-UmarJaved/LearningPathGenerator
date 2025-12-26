using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.CoreIntelligence;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DTOs.LearningPath;
using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.Services.LearningPathEngine;

public class LearningPathService : ILearningPathService
{
    private readonly AppDbContext _db;

    public LearningPathService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LearningPathDto> GenerateOrGetActiveAsync(int userId, int skillId, CancellationToken ct = default)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists) throw new InvalidOperationException("User not found");

        var skill = await _db.Skills.FirstOrDefaultAsync(s => s.SkillId == skillId, ct);
        if (skill == null) throw new InvalidOperationException("Skill not found");

        var existing = await _db.LearningPaths
            .Include(lp => lp.LearningPathCourses)
            .ThenInclude(lpc => lpc.Course)
            .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.SkillId == skillId && lp.Status == "Active", ct);

        if (existing != null)
        {
            return await MapLearningPathAsync(existing, skill.SkillName, userId, ct);
        }

        var courses = await _db.Courses
            .Where(c => c.SkillId == skillId)
            .ToListAsync(ct);

        if (courses.Count == 0)
        {
            throw new InvalidOperationException("No courses found for this skill");
        }

        var ordered = LearningPathBuilder.OrderCourses(courses);
        var roadmap = LearningPathBuilder.BuildRoadmapCourseIds(ordered);

        var path = new LearningPath
        {
            UserId = userId,
            SkillId = skillId,
            Status = "Active"
        };

        _db.LearningPaths.Add(path);
        await _db.SaveChangesAsync(ct);

        // Create path-course rows in roadmap order
        var courseById = ordered.ToDictionary(c => c.CourseId, c => c);
        foreach (var courseId in roadmap)
        {
            if (!courseById.ContainsKey(courseId)) continue;

            _db.LearningPathCourses.Add(new LearningPathCourse
            {
                PathId = path.PathId,
                CourseId = courseId,
                IsCompleted = false,
                CompletionPercentage = 0
            });
        }

        await _db.SaveChangesAsync(ct);

        // Reload with includes for DTO mapping
        var created = await _db.LearningPaths
            .Include(lp => lp.LearningPathCourses)
            .ThenInclude(lpc => lpc.Course)
            .FirstAsync(lp => lp.PathId == path.PathId, ct);

        _db.UserActivities.Add(new UserActivity
        {
            UserId = userId,
            Action = "Started learning path",
            Label = skill.SkillName,
            PathId = created.PathId,
            SkillId = skillId,
            CreatedAt = DateTime.UtcNow
        });

        _db.UserNotifications.Add(new UserNotification
        {
            UserId = userId,
            Type = "LearningPath",
            Message = $"New learning path started: {skill.SkillName}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return await MapLearningPathAsync(created, skill.SkillName, userId, ct);
    }

    public async Task<LearningPathDto> GetByIdAsync(int pathId, CancellationToken ct = default)
    {
        var path = await _db.LearningPaths
            .Include(lp => lp.Skill)
            .Include(lp => lp.LearningPathCourses)
            .ThenInclude(lpc => lpc.Course)
            .FirstOrDefaultAsync(lp => lp.PathId == pathId, ct);

        if (path == null) throw new InvalidOperationException("Learning path not found");

        return await MapLearningPathAsync(path, path.Skill.SkillName, path.UserId, ct);
    }

    private async Task<LearningPathDto> MapLearningPathAsync(LearningPath path, string skillName, int userId, CancellationToken ct)
    {
        var courses = path.LearningPathCourses
            .OrderBy(lpc => lpc.Course.CourseLevel)
            .ThenBy(lpc => lpc.Course.SequenceOrder)
            .ThenBy(lpc => lpc.CourseId)
            .ToList();

        // Refresh completion percentage from watched videos for accuracy
        var courseDtos = new List<CourseDto>(courses.Count);
        var percentages = new List<int>(courses.Count);

        foreach (var lpc in courses)
        {
            var pct = await ProgressTracker.CalculateCourseCompletionAsync(_db, userId, lpc.CourseId, ct);
            var completed = pct >= 100;
            percentages.Add(pct);

            // Persist quick snapshot in LearningPathCourses
            if (lpc.CompletionPercentage != pct || lpc.IsCompleted != completed)
            {
                lpc.CompletionPercentage = pct;
                lpc.IsCompleted = completed;
            }

            courseDtos.Add(new CourseDto
            {
                CourseId = lpc.CourseId,
                SkillId = lpc.Course.SkillId,
                CourseTitle = lpc.Course.CourseTitle,
                CourseLevel = lpc.Course.CourseLevel,
                YoutubeVideoUrl = lpc.Course.YoutubeVideoUrl,
                TotalVideos = lpc.Course.TotalVideos,
                SequenceOrder = lpc.Course.SequenceOrder,
                IsCompleted = completed,
                CompletionPercentage = pct
            });
        }

        await _db.SaveChangesAsync(ct);

        var activeCourseId = courseDtos.FirstOrDefault(c => !c.IsCompleted)?.CourseId;
        var skillPct = ProgressTracker.CalculateSkillCompletionFromCourses(percentages);

        return new LearningPathDto
        {
            PathId = path.PathId,
            UserId = path.UserId,
            SkillId = path.SkillId,
            SkillName = skillName,
            CreatedAt = path.CreatedAt,
            Status = path.Status,
            SkillCompletionPercentage = skillPct,
            ActiveCourseId = activeCourseId,
            Courses = courseDtos
        };
    }
}
