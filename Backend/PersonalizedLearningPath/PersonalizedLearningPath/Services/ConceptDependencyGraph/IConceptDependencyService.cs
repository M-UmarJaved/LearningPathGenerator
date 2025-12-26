using PersonalizedLearningPath.DTOs.ConceptDependency;

namespace PersonalizedLearningPath.Services.ConceptDependencyGraph
{
    public interface IConceptDependencyService
    {
        Task<ConceptDto> AddConceptAsync(CreateConceptDto dto, CancellationToken ct = default);

        // Adds a prerequisite relationship: prerequisite -> concept.
        // Rejects cycles and duplicates.
        Task<bool> AddPrerequisiteAsync(CreatePrerequisiteDto dto, CancellationToken ct = default);

        Task<SkillGraphDto> GetSkillGraphAsync(int skillId, CancellationToken ct = default);
        Task<List<ConceptDto>> GetConceptsBySkillAsync(int skillId, CancellationToken ct = default);
    }
}
