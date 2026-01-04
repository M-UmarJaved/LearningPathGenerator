using Microsoft.AspNetCore.Mvc;
using PersonalizedLearningPath.DTOs.ConceptDependency;
using PersonalizedLearningPath.Services.ConceptDependencyGraph;

namespace PersonalizedLearningPath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConceptDependencyController : ControllerBase
    {
        private readonly IConceptDependencyService _service;

        public ConceptDependencyController(IConceptDependencyService service)
        {
            _service = service;
        }

        // POST: api/ConceptDependency/concept
        [HttpPost("concept")]
        public async Task<IActionResult> CreateConcept([FromBody] CreateConceptDto dto, CancellationToken ct)
        {
            try
            {
                var created = await _service.AddConceptAsync(dto, ct);
                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "Skill not found")
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "Concept already exists for this skill")
            {
                return Conflict(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/ConceptDependency/prerequisite
        // Creates prerequisite -> concept relationship (DAG enforced)
        [HttpPost("prerequisite")]
        public async Task<IActionResult> CreatePrerequisite([FromBody] CreatePrerequisiteDto dto, CancellationToken ct)
        {
            if (dto.ConceptId == dto.PrerequisiteId)
            {
                return BadRequest(new { message = "A concept cannot be a prerequisite of itself" });
            }

            try
            {
                var ok = await _service.AddPrerequisiteAsync(dto, ct);
                if (!ok)
                {
                    // If service returns false, it means cycle would be introduced (or invalid self-edge already handled above).
                    return Conflict(new { message = "Cannot add prerequisite because it would create a cycle" });
                }

                return Ok(new { success = true });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "Concept or prerequisite not found" || ex.Message == "Skill not found")
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/ConceptDependency/skill/{skillId}/graph
        [HttpGet("skill/{skillId:int}/graph")]
        public async Task<IActionResult> GetSkillGraph([FromRoute] int skillId, CancellationToken ct)
        {
            try
            {
                var graph = await _service.GetSkillGraphAsync(skillId, ct);
                return Ok(graph);
            }
            catch (InvalidOperationException ex) when (ex.Message == "Skill not found")
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/ConceptDependency/skill/{skillId}/concepts
        [HttpGet("skill/{skillId:int}/concepts")]
        public async Task<IActionResult> GetConceptsBySkill([FromRoute] int skillId, CancellationToken ct)
        {
            var concepts = await _service.GetConceptsBySkillAsync(skillId, ct);
            return Ok(concepts);
        }
    }
}
