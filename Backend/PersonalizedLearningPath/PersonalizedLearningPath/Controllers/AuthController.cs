using Microsoft.AspNetCore.Mvc;
using PersonalizedLearningPath.DTOs;
using PersonalizedLearningPath.Services.AuthService;

namespace PersonalizedLearningPath.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDTO dto)
        {
            _service.Register(dto);
            return Ok("User Registered");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDTO dto)
        {
            var result = _service.Login(dto);
            if (result == null)
                return Unauthorized("Invalid credentials");

            return Ok(result);
        }

        [HttpPost("password-reset/request")]
        public IActionResult RequestPasswordReset([FromBody] ForgotPasswordRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");

            var token = _service.RequestPasswordReset(dto.Email);

            return Ok(new ForgotPasswordResponseDto
            {
                Message = "If the account exists, a reset token has been generated.",
                ResetToken = token,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
            });
        }

        [HttpPost("password-reset/confirm")]
        public IActionResult ConfirmPasswordReset([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.ResetToken) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest("Email, resetToken, and newPassword are required");

            var ok = _service.ResetPassword(dto.Email, dto.ResetToken, dto.NewPassword);
            if (!ok)
                return BadRequest("Invalid or expired reset token");

            return Ok("Password reset successful");
        }
    }
}
