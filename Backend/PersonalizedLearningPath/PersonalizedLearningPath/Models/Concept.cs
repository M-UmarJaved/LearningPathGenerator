using System.ComponentModel.DataAnnotations;

namespace PersonalizedLearningPath.Models
{
    public class Concept
    {
        [Key]
        public int ConceptId { get; set; }

        [Required]
        public int SkillId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public Skill? Skill { get; set; }

        // Navigation (DB only; not used in core graph logic)
        public ICollection<ConceptPrerequisite>? Prerequisites { get; set; }
        public ICollection<ConceptPrerequisite>? Dependents { get; set; }
    }
}
