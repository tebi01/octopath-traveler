namespace Octopath_Traveler_View;

public sealed record BeastAreaAttackViewData(
    string BeastName,
    string SkillName,
    string DamageType,
    IReadOnlyList<BeastAreaTargetViewData> Targets);
