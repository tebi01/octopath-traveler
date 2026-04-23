namespace Octopath_Traveler_View;

public sealed record BeastAttackViewData(
    string BeastName,
    string SkillName,
    string TargetName,
    string DamageType,
    int Damage,
    bool TargetIsDefending,
    int TargetCurrentHp);
