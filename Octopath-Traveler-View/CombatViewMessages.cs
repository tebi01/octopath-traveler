namespace Octopath_Traveler_View;

public sealed record TravelerAttackViewData(
    string AttackerName,
    string TargetName,
    string WeaponType,
    int Damage,
    bool HasWeakness,
    bool TargetEnteredBreakingPoint,
    int TargetCurrentHp);

public sealed record TravelerSkillAttackViewData(
    string TravelerName,
    string SkillName,
    string TargetName,
    string DamageType,
    int Damage,
    bool HasWeakness,
    bool TargetEnteredBreakingPoint,
    int TargetCurrentHp);

public sealed record LegholdTrapViewData(
    string TravelerName,
    string TargetName);

public sealed record TravelerSkillAreaTargetViewData(
    string TargetName,
    int Damage,
    bool HasWeakness,
    bool TargetEnteredBreakingPoint,
    int TargetCurrentHp);

public sealed record TravelerSkillAreaAttackViewData(
    string TravelerName,
    string SkillName,
    string DamageType,
    IReadOnlyList<TravelerSkillAreaTargetViewData> Targets);

public sealed record TravelerSkillAreaHitViewData(
    string TargetName,
    int Damage,
    string DamageType,
    bool HasWeakness,
    bool TargetEnteredBreakingPoint);

public sealed record TravelerSkillFinalHpViewData(
    string TargetName,
    int TargetCurrentHp);

public sealed record TravelerSkillMultiHitAreaAttackViewData(
    string TravelerName,
    string SkillName,
    IReadOnlyList<TravelerSkillAreaHitViewData> Hits,
    IReadOnlyList<TravelerSkillFinalHpViewData> FinalHpByTarget);

public sealed record TravelerHealingTargetViewData(
    string TargetName,
    int RecoveredHp,
    int TargetCurrentHp);

public sealed record TravelerHealingSkillViewData(
    string TravelerName,
    string SkillName,
    IReadOnlyList<TravelerHealingTargetViewData> Targets);

public sealed record TravelerReviveTargetViewData(
    string TargetName,
    bool IsRevived,
    int RecoveredHp,
    int TargetCurrentHp);

public sealed record TravelerReviveSkillViewData(
    string TravelerName,
    string SkillName,
    IReadOnlyList<TravelerReviveTargetViewData> Targets);

public sealed record BeastAttackViewData(
    string BeastName,
    string SkillName,
    string TargetName,
    string DamageType,
    int Damage,
    bool TargetIsDefending,
    int TargetCurrentHp);

public sealed record BeastAreaTargetViewData(
    string TargetName,
    int Damage,
    bool TargetIsDefending,
    int TargetCurrentHp);

public sealed record BeastAreaAttackViewData(
    string BeastName,
    string SkillName,
    string DamageType,
    IReadOnlyList<BeastAreaTargetViewData> Targets);
