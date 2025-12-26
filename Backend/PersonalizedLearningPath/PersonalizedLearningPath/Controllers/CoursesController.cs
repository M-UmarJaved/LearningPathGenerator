using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DTOs.LearningPath;
using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CoursesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("skill/{skillId:int}")]
    public async Task<IActionResult> GetBySkill([FromRoute] int skillId, CancellationToken ct)
    {
        var courses = await _db.Courses
            .Where(c => c.SkillId == skillId)
            .OrderBy(c => c.CourseLevel)
            .ThenBy(c => c.SequenceOrder)
            .ThenBy(c => c.CourseId)
            .Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                SkillId = c.SkillId,
                CourseTitle = c.CourseTitle,
                CourseLevel = c.CourseLevel,
                YoutubeVideoUrl = c.YoutubeVideoUrl,
                TotalVideos = c.TotalVideos,
                SequenceOrder = c.SequenceOrder,
                IsCompleted = false,
                CompletionPercentage = 0
            })
            .ToListAsync(ct);

        return Ok(courses);
    }

    [HttpGet("{courseId:int}")]
    public async Task<IActionResult> GetById([FromRoute] int courseId, CancellationToken ct)
    {
        var c = await _db.Courses
            .Include(x => x.Videos)
            .FirstOrDefaultAsync(x => x.CourseId == courseId, ct);
        if (c == null) return NotFound(new { message = "Course not found" });

        var videos = c.Videos
            .OrderBy(v => v.VideoIndex)
            .Select(v => new CourseVideoDto
            {
                VideoIndex = v.VideoIndex,
                VideoTitle = v.VideoTitle,
                YoutubeVideoUrl = v.YoutubeVideoUrl
            })
            .ToList();

        return Ok(new CourseDetailsDto
        {
            CourseId = c.CourseId,
            SkillId = c.SkillId,
            CourseTitle = c.CourseTitle,
            CourseLevel = c.CourseLevel,
            SequenceOrder = c.SequenceOrder,
            TotalVideos = videos.Count,
            Videos = videos
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto, CancellationToken ct)
    {
        if (dto.SkillId <= 0) return BadRequest(new { message = "SkillId is required" });
        if (string.IsNullOrWhiteSpace(dto.CourseTitle)) return BadRequest(new { message = "CourseTitle is required" });
        if (string.IsNullOrWhiteSpace(dto.CourseLevel)) return BadRequest(new { message = "CourseLevel is required" });
        if (string.IsNullOrWhiteSpace(dto.YoutubeVideoUrl)) return BadRequest(new { message = "YoutubeVideoUrl is required" });
        if (dto.TotalVideos <= 0) return BadRequest(new { message = "TotalVideos must be > 0" });

        var skillExists = await _db.Skills.AnyAsync(s => s.SkillId == dto.SkillId, ct);
        if (!skillExists) return NotFound(new { message = "Skill not found" });

        var course = new Course
        {
            SkillId = dto.SkillId,
            CourseTitle = dto.CourseTitle.Trim(),
            CourseLevel = dto.CourseLevel.Trim(),
            // Keep legacy fields populated for compatibility.
            YoutubeVideoUrl = dto.YoutubeVideoUrl.Trim(),
            TotalVideos = dto.TotalVideos,
            SequenceOrder = dto.SequenceOrder
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync(ct);

        return Ok(new { courseId = course.CourseId });
    }

    [HttpPost("{courseId:int}/videos")]
    public async Task<IActionResult> AddVideo([FromRoute] int courseId, [FromBody] CreateCourseVideoDto dto, CancellationToken ct)
    {
        if (dto.VideoIndex <= 0) return BadRequest(new { message = "VideoIndex must be > 0" });
        if (string.IsNullOrWhiteSpace(dto.VideoTitle)) return BadRequest(new { message = "VideoTitle is required" });
        if (string.IsNullOrWhiteSpace(dto.YoutubeVideoUrl)) return BadRequest(new { message = "YoutubeVideoUrl is required" });

        var courseExists = await _db.Courses.AnyAsync(c => c.CourseId == courseId, ct);
        if (!courseExists) return NotFound(new { message = "Course not found" });

        var indexTaken = await _db.CourseVideos.AnyAsync(v => v.CourseId == courseId && v.VideoIndex == dto.VideoIndex, ct);
        if (indexTaken) return Conflict(new { message = "VideoIndex already exists for this course" });

        _db.CourseVideos.Add(new CourseVideo
        {
            CourseId = courseId,
            VideoIndex = dto.VideoIndex,
            VideoTitle = dto.VideoTitle.Trim(),
            YoutubeVideoUrl = dto.YoutubeVideoUrl.Trim()
        });

        await _db.SaveChangesAsync(ct);

        // Keep Courses.TotalVideos roughly in sync as a denormalized convenience.
        var total = await _db.CourseVideos.CountAsync(v => v.CourseId == courseId, ct);
        await _db.Courses.Where(c => c.CourseId == courseId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.TotalVideos, total), ct);

        return Ok(new { message = "Video added" });
    }
}
