using Octopath_Traveler_Model;

namespace Octopath_Traveler;

internal static class TeamUnitFactory
{
    private const string InnerStrengthPassive = "Inner Strength";
    private const string ElementalAugmentationPassive = "Elemental Augmentation";
    private const string SummonStrengthPassive = "Summon Strength";
    private const string HaleAndHeartyPassive = "Hale and Hearty";
    private const string FleefootPassive = "Fleefoot";

    public static PlayerTeam BuildPlayerTeam(ParsedTeam parsedTeam, Catalogs catalogs)
    {
        return new PlayerTeam(BuildTravelers(parsedTeam.Travelers, catalogs));
    }

    public static EnemyTeam BuildEnemyTeam(ParsedTeam parsedTeam, Catalogs catalogs)
    {
        return new EnemyTeam(BuildBeasts(parsedTeam.Beasts, catalogs));
    }

    private static IReadOnlyList<Traveler> BuildTravelers(IEnumerable<ParsedTravelerLine> travelerLines, Catalogs catalogs)
    {
        return travelerLines
            .Select(travelerLine => BuildTraveler(travelerLine, catalogs))
            .ToList();
    }

    private static IReadOnlyList<Beast> BuildBeasts(IEnumerable<string> beastNames, Catalogs catalogs)
    {
        return beastNames
            .Select(beastName => BuildBeast(beastName, catalogs))
            .ToList();
    }

    private static Traveler BuildTraveler(ParsedTravelerLine travelerLine, Catalogs catalogs)
    {
        var character = catalogs.Characters[travelerLine.Name];
        var stats = BuildCharacterCombatStats(character);
        var sp = BuildCharacterSkillPoints(character);
        var (enhancedStats, enhancedSp) = ApplyBaseStatPassives(stats, sp, travelerLine.PassiveSkills);

        return new Traveler(
            character.Name,
            enhancedStats,
            enhancedSp,
            character.Weapons,
            travelerLine.ActiveSkills,
            travelerLine.PassiveSkills);
    }

    private static (CombatStats Stats, SkillPoints SkillPoints) ApplyBaseStatPassives(
        CombatStats baseStats,
        SkillPoints baseSkillPoints,
        IReadOnlyList<string> passiveSkills)
    {
        var bonuses = CalculateBasePassiveBonuses(passiveSkills);

        var enhancedStats = new CombatStats(
            baseStats.MaxHp + bonuses.hpBonus,
            baseStats.CurrentHp + bonuses.hpBonus,
            baseStats.PhysicalAttack + bonuses.physicalAttackBonus,
            baseStats.PhysicalDefense,
            baseStats.ElementalAttack + bonuses.elementalAttackBonus,
            baseStats.ElementalDefense,
            baseStats.Speed + bonuses.speedBonus);

        var enhancedSkillPoints = new SkillPoints(
            baseSkillPoints.MaxSp + bonuses.spBonus,
            baseSkillPoints.CurrentSp + bonuses.spBonus);

        return (enhancedStats, enhancedSkillPoints);
    }

    private static (int hpBonus, int spBonus, int physicalAttackBonus, int elementalAttackBonus, int speedBonus)
        CalculateBasePassiveBonuses(IReadOnlyList<string> passiveSkills)
    {
        return (
            HasPassive(passiveSkills, HaleAndHeartyPassive) ? 500 : 0,
            HasPassive(passiveSkills, InnerStrengthPassive) ? 50 : 0,
            HasPassive(passiveSkills, SummonStrengthPassive) ? 50 : 0,
            HasPassive(passiveSkills, ElementalAugmentationPassive) ? 50 : 0,
            HasPassive(passiveSkills, FleefootPassive) ? 50 : 0);
    }

    private static bool HasPassive(IEnumerable<string> passiveSkills, string passiveName)
    {
        return passiveSkills.Any(skill => string.Equals(skill, passiveName, StringComparison.OrdinalIgnoreCase));
    }

    private static Beast BuildBeast(string beastName, Catalogs catalogs)
    {
        var enemy = catalogs.Enemies[beastName];
        var stats = BuildEnemyCombatStats(enemy);

        return new Beast(enemy.Name, stats, enemy.Skill, enemy.Shields, enemy.Weaknesses);
    }

    private static CombatStats BuildCharacterCombatStats(CharacterDto character)
    {
        return new CombatStats(
            character.Stats.HP,
            character.Stats.HP,
            character.Stats.PhysAtk,
            character.Stats.PhysDef,
            character.Stats.ElemAtk,
            character.Stats.ElemDef,
            character.Stats.Speed);
    }

    private static SkillPoints BuildCharacterSkillPoints(CharacterDto character)
    {
        return new SkillPoints(character.Stats.SP, character.Stats.SP);
    }

    private static CombatStats BuildEnemyCombatStats(EnemyDto enemy)
    {
        return new CombatStats(
            enemy.Stats.HP,
            enemy.Stats.HP,
            enemy.Stats.PhysAtk,
            enemy.Stats.PhysDef,
            enemy.Stats.ElemAtk,
            enemy.Stats.ElemDef,
            enemy.Stats.Speed);
    }

}

