using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DTOs;

namespace PersonalizedLearningPath.Controllers
{
    [ApiController]
    [Route("api/skills")]
    public class SkillsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SkillsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetAll()
        {
            var skills = await _context.Skills
                .Where(s => s.IsActive)
                .OrderBy(s => s.SkillName)
                .Select(s => new SkillDto
                {
                    SkillId = s.SkillId,
                    SkillName = s.SkillName,
                    Description = s.Description,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(skills);
        }
    }
}
