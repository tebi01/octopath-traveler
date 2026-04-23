namespace Octopath_Traveler_View;

public sealed record TravelerSkillAttackViewData(
    string TravelerName,
    string SkillName,
    string TargetName,
    string DamageType,
    int Damage,
    bool HasWeakness,
    bool TargetEnteredBreakingPoint,
    int TargetCurrentHp);
