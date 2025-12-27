using Microsoft.AspNetCore.Mvc;
using PersonalizedLearningPath.Services.ProgressGraph;

namespace PersonalizedLearningPath.Controllers;

[ApiController]
[Route("api/progress-graph")]
public class ProgressGraphController : ControllerBase
{
    private readonly IProgressGraphService _service;

    public ProgressGraphController(IProgressGraphService service)
    {
        _service = service;
    }

    // GET: api/progress-graph?userId=123
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int userId, CancellationToken ct)
    {
        try
        {
            var result = await _service.BuildAsync(userId, ct);
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
}
