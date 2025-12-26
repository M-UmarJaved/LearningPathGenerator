using Microsoft.AspNetCore.Mvc;
using PersonalizedLearningPath.DTOs.LearningPath;
using PersonalizedLearningPath.Services.LearningPathEngine;

namespace PersonalizedLearningPath.Controllers;

[ApiController]
[Route("api/progress")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _service;

    public ProgressController(IProgressService service)
    {
        _service = service;
    }

    [HttpPost("video")]
    public async Task<IActionResult> MarkVideo([FromBody] MarkVideoRequestDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.MarkVideoAsync(dto.UserId, dto.CourseId, dto.VideoIndex, dto.IsWatched, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/progress/course/{courseId}?userId=123
    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetCourseProgress([FromRoute] int courseId, [FromQuery] int userId, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetCourseProgressAsync(userId, courseId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
