using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class TravelerTurnOutcomeResolver
{
    private readonly MainConsoleView _view;
    private readonly Func<TravelerTurnContext, bool> _completeTravelerTurn;

    public TravelerTurnOutcomeResolver(MainConsoleView view, Func<TravelerTurnContext, bool> completeTravelerTurn)
    {
        _view = view;
        _completeTravelerTurn = completeTravelerTurn;
    }

    public bool DefendTraveler(TravelerTurnContext travelerTurnContext)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        travelerState.IsDefending = true;
        travelerState.HasDefenderPriorityNextRound = true;
        return _completeTravelerTurn(travelerTurnContext);
    }

    public bool TravelerFlees(TravelerTurnContext travelerTurnContext)
    {
        _view.ShowFleeMessage();
        travelerTurnContext.CombatState.FinishBattle(BattleResult.PlayerDefeat);
        _view.ShowEnemyWinMessage();
        return true;
    }
}

