namespace Octopath_Traveler;

internal sealed class CharacterDto
{
    public required string Name { get; init; }
    public required CharacterStatsDto Stats { get; init; }
    public required List<string> Weapons { get; init; }
}
