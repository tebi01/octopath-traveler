namespace Octopath_Traveler_Model.CombatFlow;

public enum CombatPhase
{
    NotStarted,
    RoundSetup,
    TurnInProgress,
    RoundEnd,
    Finished
}

public enum BattleResult
{
    Ongoing,
    PlayerVictory,
    PlayerDefeat
}

public enum CombatantKind
{
    Traveler,
    Beast
}


