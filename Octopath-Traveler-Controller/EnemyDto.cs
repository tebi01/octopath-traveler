namespace Octopath_Traveler;

internal sealed class EnemyDto
{
    public required string Name { get; init; }
    public required EnemyStatsDto Stats { get; init; }
    public required string Skill { get; init; }
    public int Shields { get; init; }
    public required List<string> Weaknesses { get; init; }
}
