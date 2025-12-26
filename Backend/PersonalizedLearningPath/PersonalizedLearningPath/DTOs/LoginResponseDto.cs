namespace PersonalizedLearningPath.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool HasCompletedAssessment { get; set; }
    }
}
