using Microsoft.AspNetCore.Mvc;
using PersonalizedLearningPath.DTOs.LearningPath;
using PersonalizedLearningPath.Services.LearningPathEngine;

namespace PersonalizedLearningPath.Controllers;

[ApiController]
[Route("api/learning-path")]
public class LearningPathController : ControllerBase
{
    private readonly ILearningPathService _service;

    public LearningPathController(ILearningPathService service)
    {
        _service = service;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateLearningPathRequestDto dto, CancellationToken ct)
    {
        try
        {
            var path = await _service.GenerateOrGetActiveAsync(dto.UserId, dto.SkillId, ct);
            return Ok(path);
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

    [HttpGet("{pathId:int}")]
    public async Task<IActionResult> GetById([FromRoute] int pathId, CancellationToken ct)
    {
        try
        {
            var path = await _service.GetByIdAsync(pathId, ct);
            return Ok(path);
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
