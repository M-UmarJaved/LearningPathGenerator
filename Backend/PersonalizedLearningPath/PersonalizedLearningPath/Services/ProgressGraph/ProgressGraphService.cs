using Microsoft.EntityFrameworkCore;
using PersonalizedLearningPath.Data;
using PersonalizedLearningPath.DataStructures.Graph;
using PersonalizedLearningPath.DTOs.ProgressGraph;

namespace PersonalizedLearningPath.Services.ProgressGraph;

public sealed class ProgressGraphService : IProgressGraphService
{
    private readonly AppDbContext _db;

    // Node ID ranges (keeps IDs unique across types)
    private const int UserOffset = 1_000_000_000;
    private const int MetricOffset = 1_500_000_000;
    private const int SkillOffset = 1_800_000_000;
    private const int CourseOffset = 100_000_000;

    private sealed class NodeMeta
    {
        public string Type { get; set; } = string.Empty;
        public int? Value { get; set; }
        public bool? Completed { get; set; }
        public int? CompletionPercentage { get; set; }
        public int? WatchedVideos { get; set; }
        public int? TotalVideos { get; set; }
    }

    public ProgressGraphService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProgressGraphResponseDto> BuildAsync(int userId, CancellationToken ct = default)
    {
        if (userId <= 0) throw new ArgumentException("userId is required");

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) throw new InvalidOperationException("User not found");

        var graph = new DirectedGraph();
        var metaById = new Dictionary<int, NodeMeta>();

        var userNodeId = UserOffset + userId;
        AddNode(graph, metaById, userNodeId, $"User: {user.FullName ?? user.Email ?? "Student"}", "user");

        // Metrics (we compute them via DFS over graph, but these nodes are kept for visualization)
        var metricCoursesCompletedId = MetricOffset + 1;
        var metricVideosWatchedId = MetricOffset + 2;
        var metricSkillsLearnedId = MetricOffset + 3;

        AddNode(graph, metaById, metricCoursesCompletedId, "Total Courses Done", "metric");
        AddNode(graph, metaById, metricVideosWatchedId, "Total Videos Watched", "metric");
        AddNode(graph, metaById, metricSkillsLearnedId, "Total Skills Learned", "metric");

        graph.TryAddEdgeAcyclic(userNodeId, metricCoursesCompletedId);
        graph.TryAddEdgeAcyclic(userNodeId, metricVideosWatchedId);
        graph.TryAddEdgeAcyclic(userNodeId, metricSkillsLearnedId);

        // Watched videos per course (for course meta)
        var watchedByCourse = await _db.UserVideoProgress
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.IsWatched)
            .GroupBy(p => p.CourseId)
            .Select(g => new { CourseId = g.Key, Watched = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Watched, ct);

        // Skills in learning paths -> courses in those paths
        var pathSkillCourses = await (
                from lp in _db.LearningPaths.AsNoTracking()
                join s in _db.Skills.AsNoTracking() on lp.SkillId equals s.SkillId
                join lpc in _db.LearningPathCourses.AsNoTracking() on lp.PathId equals lpc.PathId
                join c in _db.Courses.AsNoTracking() on lpc.CourseId equals c.CourseId
                where lp.UserId == userId
                select new
                {
                    SkillId = s.SkillId,
                    SkillName = s.SkillName,
                    CourseId = c.CourseId,
                    CourseTitle = c.CourseTitle,
                    TotalVideos = c.TotalVideos,
                    IsCompleted = lpc.IsCompleted,
                    CompletionPercentage = lpc.CompletionPercentage
                }
            )
            .ToListAsync(ct);

        // Skills from assessments (even if user has no learning path yet)
        var assessedSkills = await (
                from a in _db.UserSkillAssessments.AsNoTracking()
                join s in _db.Skills.AsNoTracking() on a.SkillId equals s.SkillId
                where a.UserId == userId
                select new { s.SkillId, s.SkillName }
            )
            .Distinct()
            .ToListAsync(ct);

        // Skill completion history
        var completedSkillIds = await _db.UserSkillHistory
            .AsNoTracking()
            .Where(h => h.UserId == userId)
            .Select(h => h.SkillId)
            .Distinct()
            .ToListAsync(ct);
        var completedSkillSet = completedSkillIds.ToHashSet();

        // Add skill nodes (from paths and/or assessments)
        var allSkillIds = pathSkillCourses.Select(x => x.SkillId).Concat(assessedSkills.Select(x => x.SkillId)).Distinct();
        var skillNameById = assessedSkills.ToDictionary(x => x.SkillId, x => x.SkillName);
        foreach (var s in pathSkillCourses)
        {
            if (!skillNameById.ContainsKey(s.SkillId))
            {
                skillNameById[s.SkillId] = s.SkillName;
            }
        }

        foreach (var skillId in allSkillIds)
        {
            var skillNodeId = SkillOffset + skillId;
            var skillName = skillNameById.TryGetValue(skillId, out var nm) ? nm : $"Skill {skillId}";

            var isCompleted = completedSkillSet.Contains(skillId);
            AddNode(graph, metaById, skillNodeId, $"Skill: {skillName}", "skill", completed: isCompleted);
            graph.TryAddEdgeAcyclic(userNodeId, skillNodeId);
        }

        // Add course nodes and connect Skill -> Course
        foreach (var row in pathSkillCourses)
        {
            var courseNodeId = CourseOffset + row.CourseId;
            var watched = watchedByCourse.TryGetValue(row.CourseId, out var w) ? w : 0;

            AddNode(
                graph,
                metaById,
                courseNodeId,
                $"Course: {row.CourseTitle}",
                "course",
                completed: row.IsCompleted,
                completionPercentage: row.CompletionPercentage,
                watchedVideos: watched,
                totalVideos: row.TotalVideos
            );

            var skillNodeId = SkillOffset + row.SkillId;
            graph.TryAddEdgeAcyclic(skillNodeId, courseNodeId);
        }

        // Compute BFS levels for visualization
        var levelById = ComputeLevelsBfs(graph, userNodeId);

        // Compute metrics via DFS traversal of the graph (DSA requirement)
        var summary = ComputeSummaryDfs(graph, metaById, userNodeId);

        // Populate metric node values
        if (metaById.TryGetValue(metricCoursesCompletedId, out var m1)) m1.Value = summary.CoursesCompleted;
        if (metaById.TryGetValue(metricVideosWatchedId, out var m2)) m2.Value = summary.VideosWatched;
        if (metaById.TryGetValue(metricSkillsLearnedId, out var m3)) m3.Value = summary.SkillsLearned;

        var response = new ProgressGraphResponseDto
        {
            Summary = summary,
            Nodes = new List<ProgressGraphNodeDto>(),
            Edges = new List<ProgressGraphEdgeDto>()
        };

        // Nodes
        var n = graph.NodesHead;
        while (n != null)
        {
            var node = n.Data;
            metaById.TryGetValue(node.Id, out var m);
            levelById.TryGetValue(node.Id, out var level);

            response.Nodes.Add(new ProgressGraphNodeDto
            {
                Id = node.Id,
                Label = node.Name,
                Type = m?.Type ?? "unknown",
                Level = level,
                Value = m?.Value,
                Completed = m?.Completed,
                CompletionPercentage = m?.CompletionPercentage,
                WatchedVideos = m?.WatchedVideos,
                TotalVideos = m?.TotalVideos
            });

            n = n.Next;
        }

        // Edges
        n = graph.NodesHead;
        while (n != null)
        {
            var from = n.Data;
            var e = from.Outgoing.Head;
            while (e != null)
            {
                response.Edges.Add(new ProgressGraphEdgeDto { FromId = from.Id, ToId = e.Data.ToId });
                e = e.Next;
            }
            n = n.Next;
        }

        return response;
    }

    private static void AddNode(
        DirectedGraph graph,
        Dictionary<int, NodeMeta> metaById,
        int id,
        string label,
        string type,
        bool? completed = null,
        int? completionPercentage = null,
        int? watchedVideos = null,
        int? totalVideos = null)
    {
        if (!graph.ContainsNode(id))
        {
            graph.AddNode(id, label);
        }

        if (!metaById.ContainsKey(id))
        {
            metaById[id] = new NodeMeta { Type = type };
        }

        var meta = metaById[id];
        meta.Type = type;
        if (completed.HasValue) meta.Completed = completed;
        if (completionPercentage.HasValue) meta.CompletionPercentage = completionPercentage;
        if (watchedVideos.HasValue) meta.WatchedVideos = watchedVideos;
        if (totalVideos.HasValue) meta.TotalVideos = totalVideos;
    }

    private static Dictionary<int, int> ComputeLevelsBfs(DirectedGraph graph, int startId)
    {
        var levels = new Dictionary<int, int>();
        var visited = new HashSet<int>();
        var q = new Queue<int>();

        visited.Add(startId);
        levels[startId] = 0;
        q.Enqueue(startId);

        while (q.Count > 0)
        {
            var curId = q.Dequeue();
            var curLevel = levels[curId];

            var cur = graph.GetNode(curId);
            if (cur == null) continue;

            var e = cur.Outgoing.Head;
            while (e != null)
            {
                var nxtId = e.Data.ToId;
                if (visited.Add(nxtId))
                {
                    levels[nxtId] = curLevel + 1;
                    q.Enqueue(nxtId);
                }
                e = e.Next;
            }
        }

        return levels;
    }

    private static ProgressGraphSummaryDto ComputeSummaryDfs(DirectedGraph graph, Dictionary<int, NodeMeta> metaById, int startId)
    {
        var visited = new HashSet<int>();
        var stack = new Stack<int>();

        int coursesCompleted = 0;
        int videosWatched = 0;
        int skillsLearned = 0;

        visited.Add(startId);
        stack.Push(startId);

        while (stack.Count > 0)
        {
            var curId = stack.Pop();

            if (metaById.TryGetValue(curId, out var meta))
            {
                if (string.Equals(meta.Type, "course", StringComparison.OrdinalIgnoreCase))
                {
                    if (meta.Completed == true) coursesCompleted++;
                    if (meta.WatchedVideos.HasValue) videosWatched += meta.WatchedVideos.Value;
                }
                else if (string.Equals(meta.Type, "skill", StringComparison.OrdinalIgnoreCase))
                {
                    // "Skill learned" here means either explicitly completed (history) OR all its courses are completed.
                    // We compute "all courses completed" by scanning the outgoing adjacency list.
                    if (IsSkillLearned(graph, metaById, curId))
                    {
                        skillsLearned++;
                    }
                }
            }

            var cur = graph.GetNode(curId);
            if (cur == null) continue;

            var e = cur.Outgoing.Head;
            while (e != null)
            {
                var nxt = e.Data.ToId;
                if (visited.Add(nxt))
                {
                    stack.Push(nxt);
                }
                e = e.Next;
            }
        }

        return new ProgressGraphSummaryDto
        {
            CoursesCompleted = coursesCompleted,
            VideosWatched = videosWatched,
            SkillsLearned = skillsLearned
        };
    }

    private static bool IsSkillLearned(DirectedGraph graph, Dictionary<int, NodeMeta> metaById, int skillNodeId)
    {
        if (metaById.TryGetValue(skillNodeId, out var skillMeta) && skillMeta.Completed == true)
        {
            return true;
        }

        var skill = graph.GetNode(skillNodeId);
        if (skill == null) return false;

        bool hasCourseChild = false;
        var e = skill.Outgoing.Head;
        while (e != null)
        {
            hasCourseChild = true;
            var courseId = e.Data.ToId;

            if (!metaById.TryGetValue(courseId, out var courseMeta) || !string.Equals(courseMeta.Type, "course", StringComparison.OrdinalIgnoreCase))
            {
                e = e.Next;
                continue;
            }

            if (courseMeta.Completed != true) return false;
            e = e.Next;
        }

        // If a skill has no course children, don't auto-mark it learned.
        return hasCourseChild;
    }
}
