using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler;

public sealed class TravelerTurnContext
{
    public CombatFlowState CombatState { get; }
    public TurnEntry TravelerTurn { get; }
    public Traveler Traveler { get; }

    public TravelerTurnContext(CombatFlowState combatState, TurnEntry travelerTurn)
    {
        CombatState = combatState ?? throw new ArgumentNullException(nameof(combatState));
        TravelerTurn = travelerTurn ?? throw new ArgumentNullException(nameof(travelerTurn));
        Traveler = travelerTurn.UnitReference.Unit as Traveler
            ?? throw new InvalidOperationException("Traveler turn context requires a traveler unit.");
    }
}

public sealed class TravelerBasicAttackContext
{
    public TravelerTurnContext TravelerTurnContext { get; }
    public string WeaponType { get; }
    public UnitReference TargetBeast { get; }

    public TravelerBasicAttackContext(TravelerTurnContext travelerTurnContext, string weaponType, UnitReference targetBeast)
    {
        TravelerTurnContext = travelerTurnContext ?? throw new ArgumentNullException(nameof(travelerTurnContext));
        if (string.IsNullOrWhiteSpace(weaponType))
        {
            throw new ArgumentException("Weapon type cannot be null or empty.", nameof(weaponType));
        }

        WeaponType = weaponType;
        TargetBeast = targetBeast ?? throw new ArgumentNullException(nameof(targetBeast));
    }
}

public sealed class BeastTurnContext
{
    public CombatFlowState CombatState { get; }
    public TurnEntry BeastTurn { get; }
    public Beast Beast { get; }
    public UnitReference TargetTraveler { get; }

    public BeastTurnContext(CombatFlowState combatState, TurnEntry beastTurn, UnitReference targetTraveler)
    {
        CombatState = combatState ?? throw new ArgumentNullException(nameof(combatState));
        BeastTurn = beastTurn ?? throw new ArgumentNullException(nameof(beastTurn));
        Beast = beastTurn.UnitReference.Unit as Beast
            ?? throw new InvalidOperationException("Beast turn context requires a beast unit.");
        TargetTraveler = targetTraveler ?? throw new ArgumentNullException(nameof(targetTraveler));
    }
}

