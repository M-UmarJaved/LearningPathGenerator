namespace PersonalizedLearningPath.DTOs.ConceptDependency
{
    public class EdgeDto
    {
        // prerequisite -> concept
        public int FromPrerequisiteId { get; set; }
        public int ToConceptId { get; set; }
    }
}
