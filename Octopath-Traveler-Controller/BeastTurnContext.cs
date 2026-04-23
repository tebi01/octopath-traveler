using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler;

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
