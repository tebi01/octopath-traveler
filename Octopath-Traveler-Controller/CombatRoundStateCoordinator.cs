using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler;

internal sealed class CombatRoundStateCoordinator
{
    private readonly TurnPriorityCoordinator _turnPriorityCoordinator;

    public CombatRoundStateCoordinator(TurnPriorityCoordinator turnPriorityCoordinator)
    {
        _turnPriorityCoordinator = turnPriorityCoordinator;
    }

    public void ApplyRoundStart(CombatFlowState combatState, int roundNumber)
    {
        RechargeBpIfNeeded(combatState, roundNumber);
        UpdateBreakingPointStates(combatState);
        _turnPriorityCoordinator.UpdatePriorityModifiers(combatState);
        StartRoundQueues(combatState);
    }

    public void RefreshProjectedNextRoundQueue(CombatFlowState combatState)
    {
        if (combatState.CurrentRound is null)
        {
            return;
        }

        var projectedNextQueue = TurnQueueFactory.BuildNextRoundQueue(combatState);
        combatState.CurrentRound.ReplaceNextQueue(projectedNextQueue);
    }

    private void StartRoundQueues(CombatFlowState combatState)
    {
        var currentRoundQueue = BuildCurrentRoundQueueForRoundStart(combatState);
        _turnPriorityCoordinator.ConsumeRoundPriorityFlags(combatState);
        var nextRoundQueue = TurnQueueFactory.BuildNextRoundQueue(combatState);
        combatState.StartRound(currentRoundQueue, nextRoundQueue);
    }

    private static TurnQueue BuildCurrentRoundQueueForRoundStart(CombatFlowState combatState)
    {
        if (combatState.CurrentRound is null)
        {
            return TurnQueueFactory.BuildCurrentRoundQueue(combatState);
        }

        return combatState.CurrentRound.NextQueue.Clone();
    }

    private static void RechargeBpIfNeeded(CombatFlowState combatState, int roundNumber)
    {
        if (roundNumber > 1)
        {
            RechargeBpForAliveTravelers(combatState);
        }
    }

    private static void RechargeBpForAliveTravelers(CombatFlowState combatState)
    {
        foreach (var travelerState in combatState.UnitStates.Where(state => state.UnitReference.Kind == CombatantKind.Traveler && state.IsAlive))
        {
            travelerState.CurrentBp = Math.Min(5, travelerState.CurrentBp + 1);
        }
    }

    private static void UpdateBreakingPointStates(CombatFlowState combatState)
    {
        foreach (var beastState in combatState.UnitStates.Where(state => state.UnitReference.Kind == CombatantKind.Beast && state.IsAlive))
        {
            beastState.HasBreakingRecoveryPriorityThisRound = false;
            beastState.HasBreakingRecoveryPriorityNextRound = false;

            if (beastState.BreakingRoundsRemaining <= 0)
            {
                beastState.CanActThisRound = true;
                beastState.CanActNextRound = true;
                continue;
            }

            beastState.BreakingRoundsRemaining--;
            if (beastState.BreakingRoundsRemaining == 0)
            {
                beastState.CurrentShields = beastState.MaxShields;
                beastState.CanActThisRound = true;
                beastState.CanActNextRound = true;
                beastState.HasBreakingRecoveryPriorityThisRound = true;
                continue;
            }

            beastState.CanActThisRound = false;
            beastState.CanActNextRound = true;
            beastState.HasBreakingRecoveryPriorityNextRound = true;
        }
    }
}

