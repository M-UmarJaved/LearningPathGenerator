namespace PersonalizedLearningPath.DTOs.ConceptDependency
{
    public class ConceptDto
    {
        public int ConceptId { get; set; }
        public int SkillId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
