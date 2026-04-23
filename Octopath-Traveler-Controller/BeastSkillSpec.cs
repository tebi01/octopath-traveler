namespace Octopath_Traveler;

public sealed record BeastSkillSpec(
    string Name,
    double Modifier,
    string Target,
    int Hits,
    BeastAttackKind AttackKind,
    BeastTargetRule TargetRule,
    bool IgnoresDefend)
{
    public bool IsArea => string.Equals(Target, "Enemies", StringComparison.OrdinalIgnoreCase);
}
