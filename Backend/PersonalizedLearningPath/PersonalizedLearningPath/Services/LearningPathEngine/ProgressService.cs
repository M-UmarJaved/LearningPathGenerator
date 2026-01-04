using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.CoreIntelligence;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DTOs.LearningPath;
using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.Services.LearningPathEngine;

public class ProgressService : IProgressService
{
    private readonly AppDbContext _db;

    public ProgressService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProgressDto> MarkVideoAsync(int userId, int courseId, int videoIndex, bool isWatched, CancellationToken ct = default)
    {
        if (videoIndex <= 0) throw new ArgumentException("VideoIndex must be >= 1");

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId, ct);
        if (course == null) throw new InvalidOperationException("Course not found");
        if (videoIndex > course.TotalVideos) throw new ArgumentException("VideoIndex exceeds TotalVideos for this course");

        // Ensure user exists
        var userExists = await _db.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists) throw new InvalidOperationException("User not found");

        // Upsert video progress
        var existing = await _db.UserVideoProgress.FirstOrDefaultAsync(p =>
            p.UserId == userId && p.CourseId == courseId && p.VideoIndex == videoIndex, ct);

        if (existing == null)
        {
            _db.UserVideoProgress.Add(new UserVideoProgress
            {
                UserId = userId,
                CourseId = courseId,
                VideoIndex = videoIndex,
                IsWatched = isWatched
            });
        }
        else
        {
            existing.IsWatched = isWatched;
        }

        await _db.SaveChangesAsync(ct);

        // Find active path that contains this course (for this user/skill)
        var path = await _db.LearningPaths
            .Include(lp => lp.LearningPathCourses)
            .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.SkillId == course.SkillId && lp.Status == "Active", ct);

        if (path == null)
        {
            throw new InvalidOperationException("Active learning path not found for this user and skill");
        }

        var lpc = path.LearningPathCourses.FirstOrDefault(x => x.CourseId == courseId);
        if (lpc == null)
        {
            throw new InvalidOperationException("Course is not part of the active learning path");
        }

        var wasCourseCompleted = lpc.IsCompleted;

        var coursePct = await ProgressTracker.CalculateCourseCompletionAsync(_db, userId, courseId, ct);
        lpc.CompletionPercentage = coursePct;
        lpc.IsCompleted = coursePct >= 100;

        // Update other course snapshots and compute skill progress
        var percents = new List<int>();
        foreach (var pc in path.LearningPathCourses)
        {
            var pct = pc.CourseId == courseId
                ? coursePct
                : await ProgressTracker.CalculateCourseCompletionAsync(_db, userId, pc.CourseId, ct);

            pc.CompletionPercentage = pct;
            pc.IsCompleted = pct >= 100;
            percents.Add(pct);
        }

        var skillPct = ProgressTracker.CalculateSkillCompletionFromCourses(percents);

        int? nextCourseId = null;
        if (lpc.IsCompleted)
        {
            // Next course is the first incomplete course in deterministic order.
            // (Graph traversal is implicitly linear based on stored ordering.)
            var ordered = await _db.LearningPathCourses
                .Include(x => x.Course)
                .Where(x => x.PathId == path.PathId)
                .OrderBy(x => x.Course.CourseLevel)
                .ThenBy(x => x.Course.SequenceOrder)
                .ThenBy(x => x.CourseId)
                .ToListAsync(ct);

            nextCourseId = ordered.FirstOrDefault(x => !x.IsCompleted)?.CourseId;
        }

        // If all completed -> complete the path and add history
        if (path.LearningPathCourses.All(x => x.IsCompleted))
        {
            path.Status = "Completed";

            _db.UserSkillHistory.Add(new UserSkillHistory
            {
                UserId = userId,
                SkillId = path.SkillId,
                CompletedAt = DateTime.UtcNow
            });

            var skillName = await _db.Skills
                .Where(s => s.SkillId == path.SkillId)
                .Select(s => s.SkillName)
                .FirstOrDefaultAsync(ct) ?? $"Skill {path.SkillId}";

            _db.UserActivities.Add(new UserActivity
            {
                UserId = userId,
                Action = "Completed skill",
                Label = skillName,
                SkillId = path.SkillId,
                PathId = path.PathId,
                CreatedAt = DateTime.UtcNow
            });

            _db.UserNotifications.Add(new UserNotification
            {
                UserId = userId,
                Type = "Progress",
                Message = $"Skill completed: {skillName}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Course completion event
        if (!wasCourseCompleted && lpc.IsCompleted)
        {
            _db.UserActivities.Add(new UserActivity
            {
                UserId = userId,
                Action = "Completed course",
                Label = course.CourseTitle,
                CourseId = courseId,
                SkillId = course.SkillId,
                PathId = path.PathId,
                CreatedAt = DateTime.UtcNow
            });

            _db.UserNotifications.Add(new UserNotification
            {
                UserId = userId,
                Type = "Progress",
                Message = $"Course completed: {course.CourseTitle}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(ct);

        return new ProgressDto
        {
            PathId = path.PathId,
            SkillId = path.SkillId,
            CourseId = courseId,
            CourseCompletionPercentage = coursePct,
            CourseCompleted = lpc.IsCompleted,
            SkillCompletionPercentage = skillPct,
            PathStatus = path.Status,
            NextCourseId = nextCourseId
        };
    }

    public async Task<CourseProgressDto> GetCourseProgressAsync(int userId, int courseId, CancellationToken ct = default)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId, ct);
        if (course == null) throw new InvalidOperationException("Course not found");

        var watched = await _db.UserVideoProgress
            .Where(p => p.UserId == userId && p.CourseId == courseId && p.IsWatched)
            .OrderBy(p => p.VideoIndex)
            .Select(p => p.VideoIndex)
            .ToListAsync(ct);

        var pct = await ProgressTracker.CalculateCourseCompletionAsync(_db, userId, courseId, ct);

        return new CourseProgressDto
        {
            UserId = userId,
            CourseId = courseId,
            TotalVideos = course.TotalVideos,
            CompletionPercentage = pct,
            WatchedVideoIndexes = watched
        };
    }

    public async Task<CourseResumeDto> UpsertCourseResumeAsync(int userId, int courseId, int lastVideoIndex, int lastPositionSeconds, CancellationToken ct = default)
    {
        if (lastVideoIndex <= 0) throw new ArgumentException("LastVideoIndex must be >= 1");
        if (lastPositionSeconds < 0) throw new ArgumentException("LastPositionSeconds must be >= 0");

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId, ct);
        if (course == null) throw new InvalidOperationException("Course not found");
        if (lastVideoIndex > course.TotalVideos) throw new ArgumentException("LastVideoIndex exceeds TotalVideos for this course");

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists) throw new InvalidOperationException("User not found");

        var existing = await _db.UserCourseResumes
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId, ct);

        if (existing == null)
        {
            existing = new UserCourseResume
            {
                UserId = userId,
                CourseId = courseId,
                LastVideoIndex = lastVideoIndex,
                LastPositionSeconds = lastPositionSeconds,
                UpdatedAt = DateTime.UtcNow
            };

            _db.UserCourseResumes.Add(existing);
        }
        else
        {
            existing.LastVideoIndex = lastVideoIndex;
            existing.LastPositionSeconds = lastPositionSeconds;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        return new CourseResumeDto
        {
            UserId = existing.UserId,
            CourseId = existing.CourseId,
            LastVideoIndex = existing.LastVideoIndex,
            LastPositionSeconds = existing.LastPositionSeconds,
            UpdatedAt = existing.UpdatedAt
        };
    }

    public async Task<CourseResumeDto> GetCourseResumeAsync(int userId, int courseId, CancellationToken ct = default)
    {
        var resume = await _db.UserCourseResumes
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId, ct);

        if (resume == null) throw new InvalidOperationException("Resume not found");

        return new CourseResumeDto
        {
            UserId = resume.UserId,
            CourseId = resume.CourseId,
            LastVideoIndex = resume.LastVideoIndex,
            LastPositionSeconds = resume.LastPositionSeconds,
            UpdatedAt = resume.UpdatedAt
        };
    }

    public async Task<List<InProgressCourseDto>> GetInProgressCoursesAsync(int userId, CancellationToken ct = default)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists) throw new InvalidOperationException("User not found");

        // For each ACTIVE learning path (skill), return the next course the user should take.
        // This keeps "My Courses" non-empty until the entire skill path is completed.
        var activePaths = await _db.LearningPaths
            .Include(lp => lp.LearningPathCourses)
                .ThenInclude(lpc => lpc.Course)
                    .ThenInclude(c => c.Skill)
            .Where(lp => lp.UserId == userId && lp.Status == "Active")
            .ToListAsync(ct);

        var resumeMap = await _db.UserCourseResumes
            .Where(r => r.UserId == userId)
            .ToDictionaryAsync(r => r.CourseId, ct);

        var results = new List<InProgressCourseDto>();

        foreach (var path in activePaths)
        {
            var ordered = path.LearningPathCourses
                .OrderBy(x => x.Course.CourseLevel)
                .ThenBy(x => x.Course.SequenceOrder)
                .ThenBy(x => x.CourseId)
                .ToList();

            var next = ordered.FirstOrDefault(x => !x.IsCompleted && x.CompletionPercentage < 100);
            if (next == null) continue;

            resumeMap.TryGetValue(next.CourseId, out var resume);

            results.Add(new InProgressCourseDto
            {
                CourseId = next.CourseId,
                SkillId = next.Course.SkillId,
                SkillName = next.Course.Skill?.SkillName ?? $"Skill {next.Course.SkillId}",
                CourseTitle = next.Course.CourseTitle,
                CourseLevel = next.Course.CourseLevel,
                CompletionPercentage = next.CompletionPercentage,
                LastVideoIndex = resume?.LastVideoIndex ?? 1,
                LastPositionSeconds = resume?.LastPositionSeconds ?? 0,
                LastResumeAt = resume?.UpdatedAt
            });
        }

        return results
            .OrderByDescending(x => x.LastResumeAt)
            .ThenByDescending(x => x.CompletionPercentage)
            .ThenBy(x => x.CourseTitle)
            .ToList();
    }
}
