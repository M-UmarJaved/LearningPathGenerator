using System.ComponentModel.DataAnnotations;

namespace PersonalizedLearningPath.Models
{
    public class ConceptPrerequisite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SkillId { get; set; }

        // The concept that requires a prerequisite
        [Required]
        public int ConceptId { get; set; }

        // The prerequisite concept
        [Required]
        public int PrerequisiteId { get; set; }

        public Concept? Concept { get; set; }
        public Concept? Prerequisite { get; set; }
    }
}
