using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class TravelerSupportSkillResolver
{
    private readonly MainConsoleView _view;
    private readonly TravelerAllyTargetingService _allyTargetingService;
    private readonly Func<TravelerTurnContext, int, bool> _tryConsumeTravelerSpWithBoostPrompt;
    private readonly Func<TravelerTurnContext, bool> _completeTravelerTurn;

    public TravelerSupportSkillResolver(
        MainConsoleView view,
        TravelerAllyTargetingService allyTargetingService,
        Func<TravelerTurnContext, int, bool> tryConsumeTravelerSpWithBoostPrompt,
        Func<TravelerTurnContext, bool> completeTravelerTurn)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _allyTargetingService = allyTargetingService ?? throw new ArgumentNullException(nameof(allyTargetingService));
        _tryConsumeTravelerSpWithBoostPrompt = tryConsumeTravelerSpWithBoostPrompt
            ?? throw new ArgumentNullException(nameof(tryConsumeTravelerSpWithBoostPrompt));
        _completeTravelerTurn = completeTravelerTurn ?? throw new ArgumentNullException(nameof(completeTravelerTurn));
    }

    public bool TryResolvePartyHeal(TravelerTurnContext travelerTurnContext, string skillName, int spCost, double modifier)
    {
        if (!_tryConsumeTravelerSpWithBoostPrompt(travelerTurnContext, spCost))
        {
            return false;
        }

        var casterReference = travelerTurnContext.TravelerTurn.UnitReference;
        var targets = _allyTargetingService.GetOrderedTargets(
            travelerTurnContext.CombatState,
            casterReference,
            AllyTargetFilter.Alive);

        var recoveredAmount = Convert.ToInt32(Math.Floor(travelerTurnContext.Traveler.Stats.ElementalDefense * modifier));
        var healingResults = new List<TravelerHealingTargetViewData>();
        foreach (var target in targets)
        {
            var targetState = travelerTurnContext.CombatState.GetUnitState(target);
            targetState.CurrentHp = Math.Min(targetState.MaxHp, targetState.CurrentHp + recoveredAmount);
            healingResults.Add(new TravelerHealingTargetViewData(target.Unit.Name, recoveredAmount, targetState.CurrentHp));
        }

        _view.ShowTravelerHealingSkillResult(new TravelerHealingSkillViewData(
            travelerTurnContext.Traveler.Name,
            skillName,
            healingResults));
        return _completeTravelerTurn(travelerTurnContext);
    }

    public bool TryResolveFirstAid(TravelerTurnContext travelerTurnContext)
    {
        var selectedTarget = _allyTargetingService.TrySelectTarget(travelerTurnContext, AllyTargetFilter.Alive);
        if (selectedTarget is null)
        {
            return false;
        }

        if (!_tryConsumeTravelerSpWithBoostPrompt(travelerTurnContext, 4))
        {
            return false;
        }

        var targetState = travelerTurnContext.CombatState.GetUnitState(selectedTarget);
        var recoveredAmount = Convert.ToInt32(Math.Floor(travelerTurnContext.Traveler.Stats.ElementalDefense * 1.5));
        targetState.CurrentHp = Math.Min(targetState.MaxHp, targetState.CurrentHp + recoveredAmount);

        _view.ShowTravelerHealingSkillResult(new TravelerHealingSkillViewData(
            travelerTurnContext.Traveler.Name,
            "First Aid",
            new[] { new TravelerHealingTargetViewData(selectedTarget.Unit.Name, recoveredAmount, targetState.CurrentHp) }));
        return _completeTravelerTurn(travelerTurnContext);
    }

    public bool TryResolveRevive(TravelerTurnContext travelerTurnContext)
    {
        if (!_tryConsumeTravelerSpWithBoostPrompt(travelerTurnContext, 50))
        {
            return false;
        }

        var casterReference = travelerTurnContext.TravelerTurn.UnitReference;
        var deadTargets = _allyTargetingService.GetOrderedTargets(
            travelerTurnContext.CombatState,
            casterReference,
            AllyTargetFilter.Dead);

        var reviveResults = new List<TravelerReviveTargetViewData>();
        foreach (var target in deadTargets)
        {
            var targetState = travelerTurnContext.CombatState.GetUnitState(target);
            var wasRevived = TryApplyReviveBaseline(targetState);
            if (!wasRevived)
            {
                continue;
            }

            reviveResults.Add(new TravelerReviveTargetViewData(target.Unit.Name, IsRevived: true, RecoveredHp: 0, TargetCurrentHp: 1));
        }

        _view.ShowTravelerReviveSkillResult(new TravelerReviveSkillViewData(
            travelerTurnContext.Traveler.Name,
            "Revive",
            reviveResults));
        return _completeTravelerTurn(travelerTurnContext);
    }

    public bool TryResolveVivify(TravelerTurnContext travelerTurnContext)
    {
        var selectedTarget = _allyTargetingService.TrySelectTarget(travelerTurnContext, AllyTargetFilter.Dead);
        if (selectedTarget is null)
        {
            return false;
        }

        if (!_tryConsumeTravelerSpWithBoostPrompt(travelerTurnContext, 16))
        {
            return false;
        }

        var targetState = travelerTurnContext.CombatState.GetUnitState(selectedTarget);
        var wasDead = TryApplyReviveBaseline(targetState);

        var recoveredAmount = Convert.ToInt32(Math.Floor(travelerTurnContext.Traveler.Stats.ElementalDefense * 1.5));
        targetState.CurrentHp = Math.Min(targetState.MaxHp, targetState.CurrentHp + recoveredAmount);

        _view.ShowTravelerReviveSkillResult(new TravelerReviveSkillViewData(
            travelerTurnContext.Traveler.Name,
            "Vivify",
            new[]
            {
                new TravelerReviveTargetViewData(
                    selectedTarget.Unit.Name,
                    IsRevived: wasDead,
                    RecoveredHp: recoveredAmount,
                    TargetCurrentHp: targetState.CurrentHp)
            }));
        return _completeTravelerTurn(travelerTurnContext);
    }

    private static bool TryApplyReviveBaseline(CombatUnitState targetState)
    {
        if (targetState.IsAlive)
        {
            return false;
        }

        targetState.IsAlive = true;
        targetState.CurrentHp = 1;
        targetState.CanActThisRound = false;
        targetState.CanActNextRound = true;
        ResetRevivedTravelerRoundFlags(targetState);
        return true;
    }

    private static void ResetRevivedTravelerRoundFlags(CombatUnitState revivedTravelerState)
    {
        revivedTravelerState.IsDefending = false;
        revivedTravelerState.HasDefenderPriorityNextRound = false;
        revivedTravelerState.HasBreakingRecoveryPriorityThisRound = false;
        revivedTravelerState.HasBreakingRecoveryPriorityNextRound = false;
        revivedTravelerState.HasIncreasedPriorityThisRound = false;
        revivedTravelerState.HasDecreasedPriorityThisRound = false;
    }
}

