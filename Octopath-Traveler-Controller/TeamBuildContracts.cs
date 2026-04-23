namespace Octopath_Traveler;

internal sealed record ParsedTeam(IReadOnlyList<ParsedTravelerLine> Travelers, IReadOnlyList<string> Beasts);

internal sealed record ParsedTravelerLine(
    string Name,
    IReadOnlyList<string> ActiveSkills,
    IReadOnlyList<string> PassiveSkills);

internal sealed record Catalogs(
    Dictionary<string, CharacterDto> Characters,
    Dictionary<string, EnemyDto> Enemies,
    HashSet<string> ActiveSkills,
    HashSet<string> PassiveSkills,
    HashSet<string> BeastSkills);


