namespace Octopath_Traveler;

internal interface TeamCatalogsAssembler
{
    Catalogs Build(
        List<CharacterDto> characters,
        List<EnemyDto> enemies,
        List<SkillDto> activeSkills,
        List<SkillDto> passiveSkills,
        List<SkillDto> beastSkills);
}

internal sealed class DefaultTeamCatalogsAssembler : TeamCatalogsAssembler
{
    public Catalogs Build(
        List<CharacterDto> characters,
        List<EnemyDto> enemies,
        List<SkillDto> activeSkills,
        List<SkillDto> passiveSkills,
        List<SkillDto> beastSkills)
    {
        ArgumentNullException.ThrowIfNull(characters);
        ArgumentNullException.ThrowIfNull(enemies);
        ArgumentNullException.ThrowIfNull(activeSkills);
        ArgumentNullException.ThrowIfNull(passiveSkills);
        ArgumentNullException.ThrowIfNull(beastSkills);

        return new Catalogs(
            BuildCharactersByName(characters),
            BuildEnemiesByName(enemies),
            BuildSkillNames(activeSkills),
            BuildSkillNames(passiveSkills),
            BuildSkillNames(beastSkills));
    }

    private static Dictionary<string, CharacterDto> BuildCharactersByName(IEnumerable<CharacterDto> characters)
        => characters.ToDictionary(character => character.Name, StringComparer.OrdinalIgnoreCase);

    private static Dictionary<string, EnemyDto> BuildEnemiesByName(IEnumerable<EnemyDto> enemies)
        => enemies.ToDictionary(enemy => enemy.Name, StringComparer.OrdinalIgnoreCase);

    private static HashSet<string> BuildSkillNames(IEnumerable<SkillDto> skills)
        => skills.Select(skill => skill.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
}

