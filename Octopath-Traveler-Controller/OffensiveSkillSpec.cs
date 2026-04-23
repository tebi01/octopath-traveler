namespace Octopath_Traveler;

internal sealed record OffensiveSkillSpec(
    int SpCost,
    double Modifier,
    string DamageType,
    bool IsElemental,
    bool IsMercyStrike);
