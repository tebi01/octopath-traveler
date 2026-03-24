namespace Octopath_Traveler_Model.CombatFlow;

public sealed class TurnEntry
{
    public UnitReference UnitReference { get; }
    public int Speed { get; }

    // Initiative and priority fields are placeholders for the later turn-order implementation.
    public int Initiative { get; }
    public int PriorityTier { get; }

    public TurnEntry(UnitReference unitReference, int speed, int initiative = 0, int priorityTier = 0)
    {
        UnitReference = unitReference ?? throw new ArgumentNullException(nameof(unitReference));
        if (speed < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed cannot be negative.");
        }

        Speed = speed;
        Initiative = initiative;
        PriorityTier = priorityTier;
    }
}

