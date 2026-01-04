using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DataStructures.Graph;
using PersonalizedLearningPath.DTOs.ConceptDependency;
using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.Services.ConceptDependencyGraph
{
    public class ConceptDependencyService : IConceptDependencyService
    {
        private readonly AppDbContext _db;

        public ConceptDependencyService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ConceptDto> AddConceptAsync(CreateConceptDto dto, CancellationToken ct = default)
        {
            if (dto.SkillId <= 0) throw new ArgumentException("SkillId is required");
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required");

            var skillExists = await _db.Skills.AnyAsync(s => s.SkillId == dto.SkillId, ct);
            if (!skillExists) throw new InvalidOperationException("Skill not found");

            // Prevent duplicates per skill
            var exists = await _db.Concepts.AnyAsync(c => c.SkillId == dto.SkillId && c.Name == dto.Name, ct);
            if (exists) throw new InvalidOperationException("Concept already exists for this skill");

            var concept = new Concept
            {
                SkillId = dto.SkillId,
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
            };

            _db.Concepts.Add(concept);
            await _db.SaveChangesAsync(ct);

            return new ConceptDto
            {
                ConceptId = concept.ConceptId,
                SkillId = concept.SkillId,
                Name = concept.Name,
                Description = concept.Description
            };
        }

        public async Task<List<ConceptDto>> GetConceptsBySkillAsync(int skillId, CancellationToken ct = default)
        {
            var concepts = await _db.Concepts
                .Where(c => c.SkillId == skillId)
                .OrderBy(c => c.ConceptId)
                .ToListAsync(ct);

            var result = new List<ConceptDto>(concepts.Count);
            foreach (var c in concepts)
            {
                result.Add(new ConceptDto
                {
                    ConceptId = c.ConceptId,
                    SkillId = c.SkillId,
                    Name = c.Name,
                    Description = c.Description
                });
            }
            return result;
        }

        public async Task<bool> AddPrerequisiteAsync(CreatePrerequisiteDto dto, CancellationToken ct = default)
        {
            if (dto.SkillId <= 0) throw new ArgumentException("SkillId is required");
            if (dto.ConceptId <= 0 || dto.PrerequisiteId <= 0) throw new ArgumentException("ConceptId and PrerequisiteId are required");
            if (dto.ConceptId == dto.PrerequisiteId) return false;

            // Both concepts must exist and belong to the same skill
            var concept = await _db.Concepts.FirstOrDefaultAsync(c => c.ConceptId == dto.ConceptId, ct);
            var prereq = await _db.Concepts.FirstOrDefaultAsync(c => c.ConceptId == dto.PrerequisiteId, ct);
            if (concept == null || prereq == null) throw new InvalidOperationException("Concept or prerequisite not found");
            if (concept.SkillId != dto.SkillId || prereq.SkillId != dto.SkillId) throw new InvalidOperationException("Concepts must belong to the selected skill");

            // Idempotent: if relation exists, just return true
            var exists = await _db.ConceptPrerequisites.AnyAsync(cp =>
                cp.SkillId == dto.SkillId && cp.ConceptId == dto.ConceptId && cp.PrerequisiteId == dto.PrerequisiteId, ct);
            if (exists) return true;

            // Build current graph and validate new edge doesn't create a cycle.
            var graph = await BuildGraphForSkillAsync(dto.SkillId, ct);

            // Edge direction: prerequisite -> concept
            var ok = graph.TryAddEdgeAcyclic(dto.PrerequisiteId, dto.ConceptId);
            if (!ok) return false; // would create cycle

            _db.ConceptPrerequisites.Add(new ConceptPrerequisite
            {
                SkillId = dto.SkillId,
                ConceptId = dto.ConceptId,
                PrerequisiteId = dto.PrerequisiteId
            });

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<SkillGraphDto> GetSkillGraphAsync(int skillId, CancellationToken ct = default)
        {
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.SkillId == skillId, ct);
            if (skill == null) throw new InvalidOperationException("Skill not found");

            var concepts = await GetConceptsBySkillAsync(skillId, ct);
            var edges = await _db.ConceptPrerequisites
                .Where(cp => cp.SkillId == skillId)
                .OrderBy(cp => cp.Id)
                .ToListAsync(ct);

            var graph = new DirectedGraph();
            foreach (var c in concepts)
            {
                graph.AddNode(c.ConceptId, c.Name);
            }

            var edgeDtos = new List<EdgeDto>(edges.Count);
            foreach (var e in edges)
            {
                // prerequisite -> concept
                var added = graph.TryAddEdgeAcyclic(e.PrerequisiteId, e.ConceptId);
                if (!added)
                {
                    // Data corruption: cycle exists in DB
                    throw new InvalidOperationException("Circular dependency detected in stored data");
                }

                edgeDtos.Add(new EdgeDto
                {
                    FromPrerequisiteId = e.PrerequisiteId,
                    ToConceptId = e.ConceptId
                });
            }

            if (!graph.TryTopologicalOrder(out var topo))
            {
                throw new InvalidOperationException("Circular dependency detected");
            }

            var topoList = new List<int>();
            var node = topo.Head;
            while (node != null)
            {
                topoList.Add(node.Data);
                node = node.Next;
            }

            return new SkillGraphDto
            {
                SkillId = skill.SkillId,
                SkillName = skill.SkillName,
                Concepts = concepts,
                Edges = edgeDtos,
                TopologicalOrder = topoList
            };
        }

        private async Task<DirectedGraph> BuildGraphForSkillAsync(int skillId, CancellationToken ct)
        {
            var concepts = await _db.Concepts
                .Where(c => c.SkillId == skillId)
                .OrderBy(c => c.ConceptId)
                .ToListAsync(ct);

            var edges = await _db.ConceptPrerequisites
                .Where(cp => cp.SkillId == skillId)
                .OrderBy(cp => cp.Id)
                .ToListAsync(ct);

            var graph = new DirectedGraph();
            foreach (var c in concepts)
            {
                graph.AddNode(c.ConceptId, c.Name);
            }

            foreach (var e in edges)
            {
                if (!graph.TryAddEdgeAcyclic(e.PrerequisiteId, e.ConceptId))
                {
                    throw new InvalidOperationException("Circular dependency detected in stored data");
                }
            }

            return graph;
        }
    }
}
