namespace PersonalizedLearningPath.Models;

public class UserNotification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public string Type { get; set; } = "General";

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
