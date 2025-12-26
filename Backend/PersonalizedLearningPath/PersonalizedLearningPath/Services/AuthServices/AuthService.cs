using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DTOs;
using PersonalizedLearningPath.Models;
using System.Security.Cryptography;
using System.Text;

namespace PersonalizedLearningPath.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        private static readonly TimeSpan PasswordResetTtl = TimeSpan.FromMinutes(15);

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public void Register(RegisterDTO dto)
        {
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public LoginResponseDto? Login(LoginDTO dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null)
                return null;

            if (user.PasswordHash != HashPassword(dto.Password))
                return null;

            var hasCompletedAssessment = _context.UserSkillAssessments
                .Any(usa => usa.UserId == user.Id);

            return new LoginResponseDto
            {
                Token = "FAKE-JWT-TOKEN-FOR-NOW",
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                HasCompletedAssessment = hasCompletedAssessment
            };
        }

        public string RequestPasswordReset(string email)
        {
            // Always generate a token, even if user doesn't exist (prevents account enumeration).
            var token = GenerateResetToken();

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return token;
            }

            user.PasswordResetTokenHash = HashResetToken(token);
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.Add(PasswordResetTtl);
            _context.SaveChanges();

            return token;
        }

        public bool ResetPassword(string email, string resetToken, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(user.PasswordResetTokenHash) || user.PasswordResetTokenExpiresAt == null)
            {
                return false;
            }

            if (user.PasswordResetTokenExpiresAt.Value < DateTime.UtcNow)
            {
                return false;
            }

            var providedHash = HashResetToken(resetToken);
            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(user.PasswordResetTokenHash),
                    Encoding.UTF8.GetBytes(providedHash)))
            {
                return false;
            }

            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresAt = null;
            _context.SaveChanges();

            return true;
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateResetToken()
        {
            // 32 bytes -> base64url string (safe for URLs / copy-paste)
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private string HashResetToken(string token)
        {
            // Hash token before persisting (same SHA256 approach used elsewhere in this project).
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}
