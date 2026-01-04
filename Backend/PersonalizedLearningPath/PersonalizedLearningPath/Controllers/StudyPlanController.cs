using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DTOs.StudyPlan;
using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.Controllers;

[ApiController]
[Route("api/study-plan")]
public class StudyPlanController : ControllerBase
{
    private readonly AppDbContext _db;

    public StudyPlanController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/study-plan/events?userId=1&from=2025-12-01&to=2025-12-31
    [HttpGet("events")]
    public async Task<ActionResult<List<StudyPlanEventDto>>> GetEvents(
        [FromQuery] int userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        if (userId <= 0)
            return BadRequest(new { message = "Invalid userId." });

        var query = _db.Set<StudyPlanEvent>()
            .AsNoTracking()
            .Where(e => e.UserId == userId);

        if (from.HasValue)
            query = query.Where(e => e.EndAtUtc >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.StartAtUtc <= to.Value);

        var items = await query
            .OrderBy(e => e.StartAtUtc)
            .Select(e => new StudyPlanEventDto
            {
                StudyPlanEventId = e.StudyPlanEventId,
                UserId = e.UserId,
                Title = e.Title,
                Category = e.Category,
                Notes = e.Notes,
                StartAtUtc = e.StartAtUtc,
                EndAtUtc = e.EndAtUtc,
                SkillId = e.SkillId,
                CourseId = e.CourseId,
                IsCompleted = e.IsCompleted
            })
            .ToListAsync();

        return Ok(items);
    }

    // POST /api/study-plan/events
    [HttpPost("events")]
    public async Task<ActionResult<StudyPlanEventDto>> Create([FromBody] CreateStudyPlanEventDto dto)
    {
        if (dto.UserId <= 0)
            return BadRequest(new { message = "Invalid userId." });

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Title is required." });

        if (dto.EndAtUtc <= dto.StartAtUtc)
            return BadRequest(new { message = "EndAtUtc must be after StartAtUtc." });

        var entity = new StudyPlanEvent
        {
            UserId = dto.UserId,
            Title = dto.Title.Trim(),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "Study" : dto.Category.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            StartAtUtc = DateTime.SpecifyKind(dto.StartAtUtc, DateTimeKind.Utc),
            EndAtUtc = DateTime.SpecifyKind(dto.EndAtUtc, DateTimeKind.Utc),
            SkillId = dto.SkillId,
            CourseId = dto.CourseId,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Add(entity);
        await _db.SaveChangesAsync();

        var res = new StudyPlanEventDto
        {
            StudyPlanEventId = entity.StudyPlanEventId,
            UserId = entity.UserId,
            Title = entity.Title,
            Category = entity.Category,
            Notes = entity.Notes,
            StartAtUtc = entity.StartAtUtc,
            EndAtUtc = entity.EndAtUtc,
            SkillId = entity.SkillId,
            CourseId = entity.CourseId,
            IsCompleted = entity.IsCompleted
        };

        return Ok(res);
    }

    // PUT /api/study-plan/events/{id}?userId=1
    [HttpPut("events/{id:int}")]
    public async Task<ActionResult<StudyPlanEventDto>> Update(
        [FromRoute] int id,
        [FromQuery] int userId,
        [FromBody] UpdateStudyPlanEventDto dto)
    {
        if (userId <= 0)
            return BadRequest(new { message = "Invalid userId." });

        if (id <= 0)
            return BadRequest(new { message = "Invalid id." });

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Title is required." });

        if (dto.EndAtUtc <= dto.StartAtUtc)
            return BadRequest(new { message = "EndAtUtc must be after StartAtUtc." });

        var entity = await _db.Set<StudyPlanEvent>().FirstOrDefaultAsync(e => e.StudyPlanEventId == id && e.UserId == userId);
        if (entity == null)
            return NotFound(new { message = "Event not found." });

        entity.Title = dto.Title.Trim();
        entity.Category = string.IsNullOrWhiteSpace(dto.Category) ? "Study" : dto.Category.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        entity.StartAtUtc = DateTime.SpecifyKind(dto.StartAtUtc, DateTimeKind.Utc);
        entity.EndAtUtc = DateTime.SpecifyKind(dto.EndAtUtc, DateTimeKind.Utc);
        entity.SkillId = dto.SkillId;
        entity.CourseId = dto.CourseId;
        entity.IsCompleted = dto.IsCompleted;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new StudyPlanEventDto
        {
            StudyPlanEventId = entity.StudyPlanEventId,
            UserId = entity.UserId,
            Title = entity.Title,
            Category = entity.Category,
            Notes = entity.Notes,
            StartAtUtc = entity.StartAtUtc,
            EndAtUtc = entity.EndAtUtc,
            SkillId = entity.SkillId,
            CourseId = entity.CourseId,
            IsCompleted = entity.IsCompleted
        });
    }

    // DELETE /api/study-plan/events/{id}?userId=1
    [HttpDelete("events/{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id, [FromQuery] int userId)
    {
        if (userId <= 0)
            return BadRequest(new { message = "Invalid userId." });

        var entity = await _db.Set<StudyPlanEvent>().FirstOrDefaultAsync(e => e.StudyPlanEventId == id && e.UserId == userId);
        if (entity == null)
            return NotFound(new { message = "Event not found." });

        _db.Remove(entity);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Deleted." });
    }
}
