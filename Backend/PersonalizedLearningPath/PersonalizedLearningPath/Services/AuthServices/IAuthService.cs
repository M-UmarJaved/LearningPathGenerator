using PersonalizedLearningPath.DTOs;

namespace PersonalizedLearningPath.Services.AuthService
{
    public interface IAuthService
    {
        void Register(RegisterDTO registerDto);
        LoginResponseDto? Login(LoginDTO loginDTO);

        string RequestPasswordReset(string email);
        bool ResetPassword(string email, string resetToken, string newPassword);
    }
}
