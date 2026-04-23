namespace Octopath_Traveler_View;

public sealed record BeastAreaTargetViewData(
    string TargetName,
    int Damage,
    bool TargetIsDefending,
    int TargetCurrentHp);
