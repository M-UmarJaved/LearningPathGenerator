namespace PersonalizedLearningPath.Models;

public class CourseVideo
{
    public int CourseVideoId { get; set; }

    public int CourseId { get; set; }

    // 1..N (unique per course)
    public int VideoIndex { get; set; }

    public string VideoTitle { get; set; } = null!;

    // Store normal YouTube URL (watch or youtu.be). Frontend converts to embed.
    public string YoutubeVideoUrl { get; set; } = null!;

    public Course Course { get; set; } = null!;
}
