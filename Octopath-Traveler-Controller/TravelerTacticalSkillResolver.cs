using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class TravelerTacticalSkillResolver
{
    private const int SpearheadSpCost = 6;
    private const int LegholdTrapSpCost = 6;
    private const double SpearheadModifier = 1.5;

    private readonly MainConsoleView _view;
    private readonly TurnPriorityCoordinator _turnPriorityCoordinator;
    private readonly TravelerCombatMathService _combatMath;
    private readonly TravelerTargetSelector _trySelectTravelerTarget;
    private readonly Func<TravelerTurnContext, int, bool> _tryConsumeTravelerSpWithBoostPrompt;
    private readonly Func<TravelerTurnContext, bool> _completeTravelerTurn;

    public TravelerTacticalSkillResolver(
        MainConsoleView view,
        TurnPriorityCoordinator turnPriorityCoordinator,
        TravelerCombatMathService combatMath,
        TravelerTargetSelector trySelectTravelerTarget,
        Func<TravelerTurnContext, int, bool> tryConsumeTravelerSpWithBoostPrompt,
        Func<TravelerTurnContext, bool> completeTravelerTurn)
    {
        _view = view;
        _turnPriorityCoordinator = turnPriorityCoordinator;
        _combatMath = combatMath;
        _trySelectTravelerTarget = trySelectTravelerTarget;
        _tryConsumeTravelerSpWithBoostPrompt = tryConsumeTravelerSpWithBoostPrompt;
        _completeTravelerTurn = completeTravelerTurn;
    }

    public bool TryResolveSpearhead(TravelerTurnContext travelerTurnContext)
    {
        if (!_trySelectTravelerTarget(travelerTurnContext, out var selectedTarget))
        {
            return false;
        }

        if (!_tryConsumeTravelerSpWithBoostPrompt(travelerTurnContext, SpearheadSpCost))
        {
            return false;
        }

        var targetState = travelerTurnContext.CombatState.GetUnitState(selectedTarget);
        var hasWeakness = _combatMath.HasWeaknessAgainstAttackType(selectedTarget, "Spear");
        var wasInBreakingPoint = targetState.IsInBreakingPoint;
        var baseDamage = _combatMath.CalculatePhysicalDamageRaw(
            travelerTurnContext.Traveler.Stats.PhysicalAttack,
            selectedTarget.Unit.Stats.PhysicalDefense,
            SpearheadModifier);

        var damageContext = hasWeakness
            ? (wasInBreakingPoint
                ? TravelerDamageMultiplierContext.WeaknessAndBreakingPoint
                : TravelerDamageMultiplierContext.WeaknessOnly)
            : (wasInBreakingPoint
                ? TravelerDamageMultiplierContext.BreakingPointOnly
                : TravelerDamageMultiplierContext.None);
        var damageMultiplier = _combatMath.GetTravelerDamageMultiplier(damageContext);
        var dealtDamage = _combatMath.ApplyMultiplier(baseDamage, damageMultiplier);
        var enteredBreakingPoint = false;
        if (hasWeakness && dealtDamage > 0 && !wasInBreakingPoint)
        {
            enteredBreakingPoint = travelerTurnContext.CombatState.TryConsumeBeastShield(selectedTarget);
        }

        var targetCurrentHp = travelerTurnContext.CombatState.ApplyDamage(selectedTarget, dealtDamage);
        _view.ShowTravelerSkillAttackResult(new TravelerSkillAttackViewData(
            travelerTurnContext.Traveler.Name,
            "Spearhead",
            selectedTarget.Unit.Name,
            "Spear",
            dealtDamage,
            hasWeakness,
            enteredBreakingPoint,
            targetCurrentHp));

        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        travelerState.IncreasedPriorityRoundsRemaining = Math.Max(travelerState.IncreasedPriorityRoundsRemaining, 1);
        travelerState.PriorityModifierNextRound = 1;
        return _completeTravelerTurn(travelerTurnContext);
    }

    public bool TryResolveLegholdTrap(TravelerTurnContext travelerTurnContext)
    {
        if (!_trySelectTravelerTarget(travelerTurnContext, out var selectedTarget))
        {
            return false;
        }

        if (!_tryConsumeTravelerSpWithBoostPrompt(travelerTurnContext, LegholdTrapSpCost))
        {
            return false;
        }

        var targetState = travelerTurnContext.CombatState.GetUnitState(selectedTarget);
        var targetHasPendingTurnThisRound = _turnPriorityCoordinator.HasPendingTurnThisRound(travelerTurnContext.CombatState, selectedTarget);
        var consumesCurrentRound = !targetState.HasDecreasedPriorityThisRound;

        targetState.HasDecreasedPriorityThisRound = true;

        if (targetHasPendingTurnThisRound)
        {
            // If the target has not acted yet, apply lower priority immediately for the remaining current queue.
            _turnPriorityCoordinator.ReorderRemainingCurrentRoundQueue(travelerTurnContext.CombatState, selectedTarget);
        }

        // Base duration is 2 rounds; if this round was not consumed yet, one round is spent immediately.
        targetState.DecreasedPriorityRoundsRemaining += consumesCurrentRound ? 1 : 2;
        targetState.PriorityModifierNextRound = -1;
        _view.ShowLegholdTrapResult(new LegholdTrapViewData(travelerTurnContext.Traveler.Name, selectedTarget.Unit.Name));
        return _completeTravelerTurn(travelerTurnContext);
    }
}

