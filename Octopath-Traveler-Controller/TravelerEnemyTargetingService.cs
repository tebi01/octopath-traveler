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

    public UnitReference? TrySelectTarget(TravelerTurnContext travelerTurnContext)
    {
        var aliveBeasts = travelerTurnContext.CombatState.GetAliveBeasts();
        var enemySnapshots = aliveBeasts
            .Select(reference => BuildSnapshot(travelerTurnContext.CombatState, reference))
            .ToList();

        var selectedTarget = _view.AskTravelerTarget(travelerTurnContext.Traveler.Name, enemySnapshots);
        if (selectedTarget == enemySnapshots.Count + 1)
        {
            return null;
        }

        return aliveBeasts[selectedTarget - 1];
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

