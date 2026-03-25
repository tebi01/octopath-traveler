namespace Octopath_Traveler_View;

public sealed record TravelerAttackViewData(
    string AttackerName,
    string TargetName,
    string WeaponType,
    int Damage,
    int TargetCurrentHp);

public sealed record BeastAttackViewData(
    string BeastName,
    string TargetName,
    int Damage,
    int TargetCurrentHp);

