using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.Services.Seeding;

public class DashboardSeeder
{
    public sealed record SeedResult(int Instructors, int Courses, int CourseProfilesCreated, int CourseProfilesUpdated);

    public async Task<SeedResult> SeedAsync(CancellationToken ct = default)
    {
        // This service is intended to be resolved via DI.
        throw new InvalidOperationException("Use SeedAsync(AppDbContext, ...) overload.");
    }

    public async Task<SeedResult> SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        // 1) Ensure a few instructors exist
        var desiredInstructors = new[]
        {
            "Dr. Sarah Johnson",
            "Mike Chen",
            "Prof. Emily Watson",
            "Alex Turner",
            "Rachel Kim",
            "David Park"
        };

        var existingByName = await db.Instructors
            .ToDictionaryAsync(i => i.FullName, i => i, ct);

        foreach (var name in desiredInstructors)
        {
            if (!existingByName.ContainsKey(name))
            {
                db.Instructors.Add(new Instructor { FullName = name });
            }
        }

        await db.SaveChangesAsync(ct);

        var instructors = await db.Instructors
            .OrderBy(i => i.InstructorId)
            .ToListAsync(ct);

        // 2) Upsert course profiles
        var courses = await db.Courses
            .Include(c => c.Skill)
            .OrderBy(c => c.CourseId)
            .ToListAsync(ct);

        var existingProfiles = await db.CourseProfiles
            .ToDictionaryAsync(cp => cp.CourseId, cp => cp, ct);

        int created = 0;
        int updated = 0;

        foreach (var c in courses)
        {
            var instructorId = instructors.Count == 0
                ? (int?)null
                : instructors[(c.CourseId % instructors.Count)].InstructorId;

            var thumbnail = PickThumbnail(c.CourseId);
            var category = c.Skill?.SkillName ?? "Course";
            var estMinutes = c.TotalVideos > 0 ? c.TotalVideos * 10 : (int?)null;

            // Deterministic pseudo-rating and enrolled count
            var rating = (decimal)(4.3 + ((c.CourseId % 7) * 0.1));
            if (rating > 5.0m) rating = 5.0m;

            var enrolled = 5000 + ((c.CourseId * 137) % 25000);

            if (!existingProfiles.TryGetValue(c.CourseId, out var cp))
            {
                db.CourseProfiles.Add(new CourseProfile
                {
                    CourseId = c.CourseId,
                    InstructorId = instructorId,
                    ThumbnailUrl = thumbnail,
                    Category = category,
                    EstimatedMinutes = estMinutes,
                    Rating = rating,
                    EnrolledCount = enrolled
                });
                created++;
            }
            else
            {
                // Only fill missing/empty fields to avoid overwriting real admin edits.
                bool changed = false;

                if (cp.InstructorId == null && instructorId != null) { cp.InstructorId = instructorId; changed = true; }
                if (string.IsNullOrWhiteSpace(cp.ThumbnailUrl)) { cp.ThumbnailUrl = thumbnail; changed = true; }
                if (string.IsNullOrWhiteSpace(cp.Category)) { cp.Category = category; changed = true; }
                if (cp.EstimatedMinutes == null && estMinutes != null) { cp.EstimatedMinutes = estMinutes; changed = true; }
                if (cp.Rating == 0m) { cp.Rating = rating; changed = true; }
                if (cp.EnrolledCount == 0) { cp.EnrolledCount = enrolled; changed = true; }

                if (changed) updated++;
            }
        }

        await db.SaveChangesAsync(ct);

        return new SeedResult(
            Instructors: instructors.Count,
            Courses: courses.Count,
            CourseProfilesCreated: created,
            CourseProfilesUpdated: updated
        );
    }

    private static string PickThumbnail(int seed)
    {
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
}
