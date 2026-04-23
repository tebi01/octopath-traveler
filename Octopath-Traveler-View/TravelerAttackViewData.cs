namespace Octopath_Traveler_View;

public sealed record TravelerAttackViewData(
    string AttackerName,
    string TargetName,
    string WeaponType,
    int Damage,
    bool HasWeakness,
    bool TargetEnteredBreakingPoint,
    int TargetCurrentHp);
