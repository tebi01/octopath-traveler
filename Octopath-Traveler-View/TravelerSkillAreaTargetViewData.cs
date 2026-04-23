namespace Octopath_Traveler_View;

public sealed record TravelerSkillAreaTargetViewData(
    string TargetName,
    int Damage,
    bool HasWeakness,
    bool TargetEnteredBreakingPoint,
    int TargetCurrentHp);
