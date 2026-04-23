namespace Octopath_Traveler;

internal sealed record Catalogs(
    Dictionary<string, CharacterDto> Characters,
    Dictionary<string, EnemyDto> Enemies,
    HashSet<string> ActiveSkills,
    HashSet<string> PassiveSkills,
    HashSet<string> BeastSkills);
