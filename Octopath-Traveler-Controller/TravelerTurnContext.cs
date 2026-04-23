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
