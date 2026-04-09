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
            .Select(state => BuildTurnEntry(state, isCurrentRound: true))
            .OrderBy(entry => entry.PriorityTier)
            .ThenBy(entry => GetPrimaryOrderKey(entry))
            .ThenBy(entry => GetSecondaryOrderKey(entry))
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
            .Select(state => BuildTurnEntry(state, isCurrentRound: false))
            .OrderBy(entry => entry.PriorityTier)
            .ThenBy(entry => GetPrimaryOrderKey(entry))
            .ThenBy(entry => GetSecondaryOrderKey(entry))
            .ThenBy(entry => entry.UnitReference.BoardPosition)
            .ToList();

        return new TurnQueue(entries);
    }

    private static TurnEntry BuildTurnEntry(CombatUnitState state, bool isCurrentRound)
    {
        var speed = state.UnitReference.Unit.Speed;
        var priorityTier = ResolvePriorityTier(state, isCurrentRound);
        return new TurnEntry(state.UnitReference, speed, priorityTier: priorityTier);
    }

    private static int ResolvePriorityTier(CombatUnitState state, bool isCurrentRound)
    {
        if (isCurrentRound && state.HasBreakingRecoveryPriorityThisRound)
        {
            return 1;
        }

        if (!isCurrentRound && state.HasBreakingRecoveryPriorityNextRound)
        {
            return 1;
        }

        if (state.UnitReference.Kind == CombatantKind.Traveler && state.HasDefenderPriorityNextRound)
        {
            return 2;
        }

        if (isCurrentRound && state.HasIncreasedPriorityThisRound)
        {
            return 3;
        }

        if (!isCurrentRound && state.PriorityModifierNextRound > 0)
        {
            return 3;
        }

        if (isCurrentRound && state.HasDecreasedPriorityThisRound)
        {
            return 5;
        }

        if (!isCurrentRound && state.PriorityModifierNextRound < 0)
        {
            return 5;
        }

        return 4;
    }

    private static int GetPrimaryOrderKey(TurnEntry entry)
    {
        if (entry.PriorityTier is 3 or 5)
        {
            return entry.UnitReference.Kind == CombatantKind.Traveler ? 0 : 1;
        }

        return -entry.Speed;
    }

    private static int GetSecondaryOrderKey(TurnEntry entry)
    {
        if (entry.PriorityTier is 3 or 5)
        {
            return -entry.Speed;
        }

        return entry.UnitReference.Kind == CombatantKind.Traveler ? 0 : 1;
    }
}

