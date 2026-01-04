namespace PersonalizedLearningPath.DTOs;

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = "";

    // Dev-friendly: since we aren't sending emails, return the token so the UI can complete the flow.
    public string? ResetToken { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }
}
