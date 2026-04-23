using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class TravelerTurnResolver
{
    private readonly MainConsoleView _view;
    private readonly TravelerSkillDispatchResolver _travelerSkillDispatchResolver;
    private readonly TravelerBasicAttackResolver _basicAttackResolver;
    private readonly TravelerTurnOutcomeResolver _travelerTurnOutcomeResolver;

    public TravelerTurnResolver(
        MainConsoleView view,
        TravelerSkillDispatchResolver travelerSkillDispatchResolver,
        TravelerBasicAttackResolver basicAttackResolver,
        TravelerTurnOutcomeResolver travelerTurnOutcomeResolver)
    {
        _view = view;
        _travelerSkillDispatchResolver = travelerSkillDispatchResolver;
        _basicAttackResolver = basicAttackResolver;
        _travelerTurnOutcomeResolver = travelerTurnOutcomeResolver;
    }

    public void ResolveTravelerTurn(CombatFlowState combatState, TurnEntry travelerTurn)
    {
        var travelerTurnContext = new TravelerTurnContext(combatState, travelerTurn);

        while (IsBattleOngoing(travelerTurnContext.CombatState))
        {
            var selectedAction = AskTravelerAction(travelerTurnContext);
            if (TryHandleTravelerAction(travelerTurnContext, selectedAction))
            {
                return;
            }
        }
    }

    private int AskTravelerAction(TravelerTurnContext travelerTurnContext)
        => _view.AskTravelerMainAction(travelerTurnContext.Traveler.Name);

    private bool TryHandleTravelerAction(TravelerTurnContext travelerTurnContext, int selectedAction)
    {
        return selectedAction switch
        {
            1 => TryHandleTravelerBasicAttack(travelerTurnContext),
            2 => _travelerSkillDispatchResolver.ShowTravelerSkills(travelerTurnContext),
            3 => _travelerTurnOutcomeResolver.DefendTraveler(travelerTurnContext),
            4 => _travelerTurnOutcomeResolver.TravelerFlees(travelerTurnContext),
            _ => false
        };
    }

    private bool TryHandleTravelerBasicAttack(TravelerTurnContext travelerTurnContext)
    {
        if (!_basicAttackResolver.TryResolveBasicAttack(travelerTurnContext))
        {
            return false;
        }

        travelerTurnContext.CombatState.CompleteTurn();
        return true;
    }

    private static bool IsBattleOngoing(CombatFlowState combatState)
        => combatState.Result == BattleResult.Ongoing;
}

