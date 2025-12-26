namespace PersonalizedLearningPath.DTOs
{
    public class SkillDto
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; } = null!;

        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
