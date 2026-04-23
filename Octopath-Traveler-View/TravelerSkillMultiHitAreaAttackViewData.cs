namespace Octopath_Traveler_View;

public sealed record TravelerSkillMultiHitAreaAttackViewData(
    string TravelerName,
    string SkillName,
    IReadOnlyList<TravelerSkillAreaHitViewData> Hits,
    IReadOnlyList<TravelerSkillFinalHpViewData> FinalHpByTarget);
