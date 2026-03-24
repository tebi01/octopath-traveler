namespace Octopath_Traveler_Model.CombatFlow;

public static class TurnQueueFactory
{
    public static TurnQueue BuildCurrentRoundQueue(CombatFlowState flowState)
    {
        if (flowState is null)
        {
            throw new ArgumentNullException(nameof(flowState));
        }

        var entries = flowState.UnitStates
            .Where(state => state.IsAlive && state.CanActThisRound)
            .Select(state => new TurnEntry(state.UnitReference, state.UnitReference.Unit.Speed))
            .OrderByDescending(entry => entry.Speed)
            .ThenBy(entry => entry.UnitReference.Kind)
            .ThenBy(entry => entry.UnitReference.BoardPosition)
            .ToList();

        return new TurnQueue(entries);
    }

    public static TurnQueue BuildNextRoundQueue(CombatFlowState flowState)
    {
        if (flowState is null)
        {
            throw new ArgumentNullException(nameof(flowState));
        }

        var entries = flowState.UnitStates
            .Where(state => state.IsAlive && state.CanActNextRound)
            .Select(state => new TurnEntry(state.UnitReference, state.UnitReference.Unit.Speed))
            .OrderByDescending(entry => entry.Speed)
            .ThenBy(entry => entry.UnitReference.Kind)
            .ThenBy(entry => entry.UnitReference.BoardPosition)
            .ToList();

        return new TurnQueue(entries);
    }
}

