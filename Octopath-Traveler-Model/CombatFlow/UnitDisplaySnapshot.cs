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
