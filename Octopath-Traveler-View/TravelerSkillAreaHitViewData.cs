namespace Octopath_Traveler_View;

public sealed record TravelerSkillAreaHitViewData(
    string TargetName,
    int Damage,
    string DamageType,
    bool HasWeakness,
    bool TargetEnteredBreakingPoint);
