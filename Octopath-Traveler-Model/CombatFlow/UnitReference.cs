namespace Octopath_Traveler_Model.CombatFlow;

public sealed class UnitReference
{
    public Unit Unit { get; }
    public CombatantKind Kind { get; }
    public int BoardPosition { get; }

    public UnitReference(Unit unit, CombatantKind kind, int boardPosition)
    {
        Unit = unit ?? throw new ArgumentNullException(nameof(unit));
        Kind = kind;

        if (boardPosition < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(boardPosition), "Board position must be zero or greater.");
        }

        BoardPosition = boardPosition;
    }
}

