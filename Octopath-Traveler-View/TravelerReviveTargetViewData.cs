namespace Octopath_Traveler_View;

public sealed record TravelerReviveTargetViewData(
    string TargetName,
    bool IsRevived,
    int RecoveredHp,
    int TargetCurrentHp);
