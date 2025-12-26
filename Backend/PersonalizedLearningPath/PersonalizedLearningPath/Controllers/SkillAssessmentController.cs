using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalizedLearningPath.DTOs.SkillAssessment;
using PersonalizedLearningPath.Services.SkillAssessment;

namespace PersonalizedLearningPath.Controllers
{
    [ApiController]
    [Route("api/skill-assessment")]
    public class SkillAssessmentController : ControllerBase
    {
        private readonly ISkillAssessmentService _service;


        public SkillAssessmentController(ISkillAssessmentService service)
        {
            _service = service;
        }

        [HttpGet("start/{skillId}")]
        public IActionResult Start(int skillId)
        {
            return Ok(_service.StartAssessment(skillId));
        }

        [HttpPost("answer")]
        public IActionResult Answer([FromBody] AnswerDto dto)
        {
            return Ok(_service.SubmitAnswer(dto));
        }

        // Final submission endpoint: use this on the 5th question.
        // This endpoint exists so the frontend can explicitly "finalize" an assessment.
        [HttpPost("submit")]
        public IActionResult Submit([FromBody] AnswerDto dto)
        {
            var result = _service.SubmitAnswer(dto);
            if (!result.Completed)
            {
                // If the client calls /submit before the 5th answer, do not hand out more questions here.
                return BadRequest(new
                {
                    message = "Final submit did not complete the assessment. Call /answer until question 5, then call /submit on the 5th answer.",
                    serverTotalCount = result.TotalCount,
                    serverCorrectCount = result.CorrectCount
                });
            }

            return Ok(result);
        }
    }

}
