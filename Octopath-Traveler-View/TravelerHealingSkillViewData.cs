namespace Octopath_Traveler_View;

public sealed record TravelerHealingSkillViewData(
    string TravelerName,
    string SkillName,
    IReadOnlyList<TravelerHealingTargetViewData> Targets);
