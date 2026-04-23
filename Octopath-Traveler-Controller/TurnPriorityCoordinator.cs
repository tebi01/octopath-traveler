using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler;

internal sealed class TurnPriorityCoordinator
{
    public void UpdatePriorityModifiers(CombatFlowState combatState)
    {
        foreach (var unitState in combatState.UnitStates.Where(state => state.IsAlive))
        {
            unitState.HasIncreasedPriorityThisRound = false;
            unitState.HasDecreasedPriorityThisRound = false;
            unitState.PriorityModifierNextRound = 0;

            if (unitState.IncreasedPriorityRoundsRemaining > 0)
            {
                unitState.HasIncreasedPriorityThisRound = true;
                unitState.IncreasedPriorityRoundsRemaining--;
            }

            if (unitState.DecreasedPriorityRoundsRemaining > 0)
            {
                unitState.HasDecreasedPriorityThisRound = true;
                unitState.DecreasedPriorityRoundsRemaining--;
            }

            if (unitState.IncreasedPriorityRoundsRemaining > 0)
            {
                unitState.PriorityModifierNextRound = 1;
            }
            else if (unitState.DecreasedPriorityRoundsRemaining > 0)
            {
                unitState.PriorityModifierNextRound = -1;
            }
        }
    }

    public void ConsumeRoundPriorityFlags(CombatFlowState combatState)
    {
        foreach (var unitState in combatState.UnitStates.Where(state => state.IsAlive))
        {
            unitState.HasBreakingRecoveryPriorityThisRound = false;
            unitState.HasDefenderPriorityNextRound = false;
        }
    }

    public bool HasPendingTurnThisRound(CombatFlowState combatState, UnitReference target)
    {
        return combatState.CurrentRound is not null
            && combatState.CurrentRound.CurrentQueue.Entries.Any(entry => ReferenceEquals(entry.UnitReference.Unit, target.Unit));
    }

    public void ReorderRemainingCurrentRoundQueue(CombatFlowState combatState, UnitReference target)
    {
        if (combatState.CurrentRound is null)
        {
            return;
        }

        var currentQueue = combatState.CurrentRound.CurrentQueue;
        var targetEntry = currentQueue.Entries
            .FirstOrDefault(entry => ReferenceEquals(entry.UnitReference.Unit, target.Unit));
        if (targetEntry is null)
        {
            return;
        }

        var reorderedEntries = currentQueue.Entries
            .Where(entry => !ReferenceEquals(entry.UnitReference.Unit, target.Unit))
            .Concat(new[] { targetEntry })
            .ToList();

        while (!currentQueue.IsEmpty)
        {
            _ = currentQueue.PopFirst();
        }

        foreach (var entry in reorderedEntries)
        {
            currentQueue.Add(entry);
        }
    }
}

