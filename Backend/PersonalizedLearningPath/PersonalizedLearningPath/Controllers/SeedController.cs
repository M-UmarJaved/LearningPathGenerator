using Microsoft.AspNetCore.Mvc;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.Services.Seeding;

namespace PersonalizedLearningPath.Controllers;

[ApiController]
[Route("api/seed")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly DashboardSeeder _seeder;

    public SeedController(AppDbContext db, DashboardSeeder seeder)
    {
        _db = db;
        _seeder = seeder;
    }

    // POST: api/seed/dashboard
    // Creates/updates Instructors + CourseProfiles for existing Courses.
    [HttpPost("dashboard")]
    public async Task<IActionResult> SeedDashboard(CancellationToken ct)
    {
        var result = await _seeder.SeedAsync(_db, ct);
        return Ok(new
        {
            instructors = result.Instructors,
            courses = result.Courses,
            courseProfilesCreated = result.CourseProfilesCreated,
            courseProfilesUpdated = result.CourseProfilesUpdated
        });
    }
}
