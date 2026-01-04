namespace PersonalizedLearningPath.DTOs.ProgressGraph;

public sealed class ProgressGraphNodeDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;

    // "user" | "metric" | "skill" | "course"
    public string Type { get; set; } = string.Empty;

    // BFS distance from the user root (used by frontend layout)
    public int Level { get; set; }

    // Optional node details (used by UI tooltips/cards)
    public int? Value { get; set; }
    public bool? Completed { get; set; }

    public int? CompletionPercentage { get; set; }
    public int? WatchedVideos { get; set; }
    public int? TotalVideos { get; set; }
}
