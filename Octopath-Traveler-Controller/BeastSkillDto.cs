namespace Octopath_Traveler;

internal sealed class BeastSkillDto
{
    public string Name { get; init; } = string.Empty;
    public double Modifier { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public int Hits { get; init; }
}
