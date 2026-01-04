namespace PersonalizedLearningPath.DTOs.ConceptDependency
{
    public class CreatePrerequisiteDto
    {
        public int SkillId { get; set; }
        public int ConceptId { get; set; }
        public int PrerequisiteId { get; set; }
    }
}
