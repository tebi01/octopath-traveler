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

