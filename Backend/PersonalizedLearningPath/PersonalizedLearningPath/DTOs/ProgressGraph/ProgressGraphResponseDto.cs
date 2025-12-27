namespace PersonalizedLearningPath.DTOs.ProgressGraph;

public sealed class ProgressGraphResponseDto
{
    public ProgressGraphSummaryDto Summary { get; set; } = new();
    public List<ProgressGraphNodeDto> Nodes { get; set; } = new();
    public List<ProgressGraphEdgeDto> Edges { get; set; } = new();
}
