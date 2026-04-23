using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class TravelerEnemyTargetingService
{
    private readonly MainConsoleView _view;

    public TravelerEnemyTargetingService(MainConsoleView view)
    {
        _view = view;
    }

    public bool TrySelectTarget(TravelerTurnContext travelerTurnContext, out UnitReference selectedTarget)
    {
        var aliveBeasts = travelerTurnContext.CombatState.GetAliveBeasts();
        var enemySnapshots = aliveBeasts
            .Select(reference => BuildSnapshot(travelerTurnContext.CombatState, reference))
            .ToList();

        var selectedTargetIndex = _view.AskTravelerTarget(travelerTurnContext.Traveler.Name, enemySnapshots);
        if (selectedTargetIndex == enemySnapshots.Count + 1)
        {
            selectedTarget = travelerTurnContext.TravelerTurn.UnitReference;
            return false;
        }

        selectedTarget = aliveBeasts[selectedTargetIndex - 1];
        return true;
    }

    private static UnitDisplaySnapshot BuildSnapshot(CombatFlowState combatState, UnitReference reference)
    {
        var state = combatState.GetUnitState(reference);
        var slot = ((char)('A' + reference.BoardPosition)).ToString();

        return new UnitDisplaySnapshot(
            slot,
            reference.Unit.Name,
            reference.Kind,
            state.CurrentHp,
            state.MaxHp,
            state.CurrentSp,
            state.MaxSp,
            state.CurrentBp,
            state.CurrentShields);
    }
}

