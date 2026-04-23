namespace Octopath_Traveler;

internal sealed class CharacterDto
{
    public required string Name { get; init; }
    public required CharacterStatsDto Stats { get; init; }
    public required List<string> Weapons { get; init; }
}

internal sealed class CharacterStatsDto
{
    public int HP { get; init; }
    public int SP { get; init; }
    public int PhysAtk { get; init; }
    public int PhysDef { get; init; }
    public int ElemAtk { get; init; }
    public int ElemDef { get; init; }
    public int Speed { get; init; }
}

internal sealed class EnemyDto
{
    public required string Name { get; init; }
    public required EnemyStatsDto Stats { get; init; }
    public required string Skill { get; init; }
    public int Shields { get; init; }
    public required List<string> Weaknesses { get; init; }
}

internal sealed class EnemyStatsDto
{
    public int HP { get; init; }
    public int PhysAtk { get; init; }
    public int PhysDef { get; init; }
    public int ElemAtk { get; init; }
    public int ElemDef { get; init; }
    public int Speed { get; init; }
}

internal sealed class SkillDto
{
    public required string Name { get; init; }
}

