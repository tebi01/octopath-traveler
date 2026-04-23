namespace Octopath_Traveler_Model.CombatFlow;

public sealed class CombatViewSnapshot
{
    public int RoundNumber { get; }
    public IReadOnlyList<UnitDisplaySnapshot> PlayerTeam { get; }
    public IReadOnlyList<UnitDisplaySnapshot> EnemyTeam { get; }
    public IReadOnlyList<string> CurrentRoundTurns { get; }
    public IReadOnlyList<string> NextRoundTurns { get; }

    public CombatViewSnapshot(
        int roundNumber,
        IReadOnlyList<UnitDisplaySnapshot> playerTeam,
        IReadOnlyList<UnitDisplaySnapshot> enemyTeam,
        IReadOnlyList<string> currentRoundTurns,
        IReadOnlyList<string> nextRoundTurns)
    {
        RoundNumber = roundNumber;
        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;
        CurrentRoundTurns = currentRoundTurns;
        NextRoundTurns = nextRoundTurns;
    }
}

