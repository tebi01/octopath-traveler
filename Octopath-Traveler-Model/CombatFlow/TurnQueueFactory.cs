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
            .Select(BuildCurrentRoundTurnEntry)
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
            .Select(BuildNextRoundTurnEntry)
            .OrderBy(entry => entry.PriorityTier)
            .ThenBy(entry => GetPrimaryOrderKey(entry))
            .ThenBy(entry => GetSecondaryOrderKey(entry))
            .ThenBy(entry => entry.UnitReference.BoardPosition)
            .ToList();

        return new TurnQueue(entries);
    }

    private static TurnEntry BuildCurrentRoundTurnEntry(CombatUnitState state)
    {
        var speed = state.UnitReference.Unit.Speed;
        var priorityTier = ResolveCurrentRoundPriorityTier(state);
        return new TurnEntry(state.UnitReference, speed, priorityTier: priorityTier);
    }

    private static TurnEntry BuildNextRoundTurnEntry(CombatUnitState state)
    {
        var speed = state.UnitReference.Unit.Speed;
        var priorityTier = ResolveNextRoundPriorityTier(state);
        return new TurnEntry(state.UnitReference, speed, priorityTier: priorityTier);
    }

    private static int ResolveCurrentRoundPriorityTier(CombatUnitState state)
    {
        if (state.HasBreakingRecoveryPriorityThisRound)
        {
            return 1;
        }

        if (state.HasIncreasedPriorityThisRound)
        {
            return 3;
        }

        if (state.HasDecreasedPriorityThisRound)
        {
            return 5;
        }

        return 4;
    }

    private static int ResolveNextRoundPriorityTier(CombatUnitState state)
    {
        if (state.HasBreakingRecoveryPriorityNextRound)
        {
            return 1;
        }

        if (state.UnitReference.Kind == CombatantKind.Traveler && state.HasDefenderPriorityNextRound)
        {
            return 2;
        }

        if (state.PriorityModifierNextRound > 0)
        {
            return 3;
        }

        if (state.PriorityModifierNextRound < 0)
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

