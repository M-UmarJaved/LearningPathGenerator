namespace PersonalizedLearningPath.DTOs.ConceptDependency
{
    public class CreateConceptDto
    {
        public int SkillId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
