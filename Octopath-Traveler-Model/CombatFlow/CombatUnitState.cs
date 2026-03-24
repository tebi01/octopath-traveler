namespace Octopath_Traveler_Model.CombatFlow;

public sealed class CombatUnitState
{
    public UnitReference UnitReference { get; }
    public int MaxHp { get; }
    public int CurrentHp { get; set; }

    public bool IsAlive { get; set; } = true;
    public bool CanActThisRound { get; set; } = true;
    public bool CanActNextRound { get; set; } = true;

    // Travelers
    public int MaxSp { get; }
    public int CurrentSp { get; set; }
    public int CurrentBp { get; set; }
    public bool UsedBoostingThisRound { get; set; }

    // Beasts (Breaking Point hooks for a later implementation)
    public int MaxShields { get; }
    public int CurrentShields { get; set; }
    public bool IsInBreakingPoint { get; set; }
    public int BreakingRoundsRemaining { get; set; }

    // Turn-order hooks for a later implementation
    public bool HasDefenderPriorityNextRound { get; set; }
    public int PriorityModifierNextRound { get; set; }

    public CombatUnitState(UnitReference unitReference)
    {
        UnitReference = unitReference ?? throw new ArgumentNullException(nameof(unitReference));

        MaxHp = unitReference.Unit.Stats.MaxHp;
        CurrentHp = unitReference.Unit.Stats.CurrentHp;
        IsAlive = CurrentHp > 0;

        if (unitReference.Unit is Traveler traveler)
        {
            MaxSp = traveler.SkillPoints.MaxSp;
            CurrentSp = traveler.SkillPoints.CurrentSp;
            CurrentBp = 1;
            return;
        }

        if (unitReference.Unit is Beast beast)
        {
            MaxShields = beast.MaxShields;
            CurrentShields = beast.CurrentShields;
        }
    }
}

