namespace PersonalizedLearningPath.DTOs.ConceptDependency
{
    public class SkillGraphDto
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;

        public List<ConceptDto> Concepts { get; set; } = new();
        public List<EdgeDto> Edges { get; set; } = new();

        // Deterministic topological order (foundation -> advanced)
        public List<int> TopologicalOrder { get; set; } = new();
    }
}
