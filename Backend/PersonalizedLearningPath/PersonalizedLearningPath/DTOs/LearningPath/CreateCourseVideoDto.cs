namespace PersonalizedLearningPath.DTOs.LearningPath;

public class CreateCourseVideoDto
{
    public int VideoIndex { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public string YoutubeVideoUrl { get; set; } = string.Empty;
}
