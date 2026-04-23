namespace Octopath_Traveler_View;

public sealed record TravelerSkillAreaAttackViewData(
    string TravelerName,
    string SkillName,
    string DamageType,
    IReadOnlyList<TravelerSkillAreaTargetViewData> Targets);
