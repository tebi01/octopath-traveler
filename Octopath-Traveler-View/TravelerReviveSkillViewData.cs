namespace Octopath_Traveler_View;

public sealed record TravelerReviveSkillViewData(
    string TravelerName,
    string SkillName,
    IReadOnlyList<TravelerReviveTargetViewData> Targets);
