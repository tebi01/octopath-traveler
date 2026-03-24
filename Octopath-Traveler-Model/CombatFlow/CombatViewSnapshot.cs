namespace Octopath_Traveler_Model.CombatFlow;

public sealed class UnitDisplaySnapshot
{
    public string BoardSlot { get; }
    public string Name { get; }
    public CombatantKind Kind { get; }
    public int CurrentHp { get; }
    public int MaxHp { get; }
    public int CurrentSp { get; }
    public int MaxSp { get; }
    public int CurrentBp { get; }
    public int CurrentShields { get; }

    public UnitDisplaySnapshot(
        string boardSlot,
        string name,
        CombatantKind kind,
        int currentHp,
        int maxHp,
        int currentSp,
        int maxSp,
        int currentBp,
        int currentShields)
    {
        BoardSlot = boardSlot;
        Name = name;
        Kind = kind;
        CurrentHp = currentHp;
        MaxHp = maxHp;
        CurrentSp = currentSp;
        MaxSp = maxSp;
        CurrentBp = currentBp;
        CurrentShields = currentShields;
    }
}

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

