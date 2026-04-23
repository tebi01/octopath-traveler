using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal enum AllyTargetFilter
{
    Alive,
    Dead
}

internal sealed class TravelerAllyTargetingService
{
    private readonly MainConsoleView _view;

    public TravelerAllyTargetingService(MainConsoleView view)
    {
        _view = view;
    }

    public UnitReference? TrySelectTarget(TravelerTurnContext travelerTurnContext, AllyTargetFilter targetFilter)
    {
        var candidates = GetCandidates(travelerTurnContext.CombatState, targetFilter);
        if (candidates.Count == 0)
        {
            _ = _view.AskAllyTarget(travelerTurnContext.Traveler.Name, Array.Empty<UnitDisplaySnapshot>());
            return null;
        }

        var snapshots = candidates
            .Select(reference => BuildSnapshot(travelerTurnContext.CombatState, reference))
            .ToList();

        var selectedTarget = _view.AskAllyTarget(travelerTurnContext.Traveler.Name, snapshots);
        if (selectedTarget == snapshots.Count + 1)
        {
            return null;
        }

        return candidates[selectedTarget - 1];
    }

    public IReadOnlyList<UnitReference> GetOrderedTargets(
        CombatFlowState combatState,
        UnitReference casterReference,
        AllyTargetFilter targetFilter)
    {
        var filteredTargets = GetCandidates(combatState, targetFilter);
        var nonCasterTargets = filteredTargets
            .Where(reference => !ReferenceEquals(reference.Unit, casterReference.Unit))
            .ToList();

        var casterTarget = filteredTargets.FirstOrDefault(reference => ReferenceEquals(reference.Unit, casterReference.Unit));
        if (casterTarget is not null)
        {
            nonCasterTargets.Add(casterTarget);
        }

        return nonCasterTargets;
    }

    private static List<UnitReference> GetCandidates(CombatFlowState combatState, AllyTargetFilter targetFilter)
    {
        return combatState.UnitStates
            .Where(state => state.UnitReference.Kind == CombatantKind.Traveler)
            .OrderBy(state => state.UnitReference.BoardPosition)
            .Where(state => targetFilter == AllyTargetFilter.Alive ? state.IsAlive : !state.IsAlive)
            .Select(state => state.UnitReference)
            .ToList();
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

