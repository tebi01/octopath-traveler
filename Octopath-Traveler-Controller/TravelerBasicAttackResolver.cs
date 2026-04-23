using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class TravelerBasicAttackResolver
{
    private const double BasicAttackModifier = 1.3;

    private readonly MainConsoleView _view;
    private readonly TravelerCombatMathService _combatMath;
    private readonly Func<TravelerTurnContext, UnitReference?> _trySelectTravelerTarget;
    private readonly Action<TravelerTurnContext> _askBoostPointsIfAvailable;

    public TravelerBasicAttackResolver(
        MainConsoleView view,
        TravelerCombatMathService combatMath,
        Func<TravelerTurnContext, UnitReference?> trySelectTravelerTarget,
        Action<TravelerTurnContext> askBoostPointsIfAvailable)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _combatMath = combatMath ?? throw new ArgumentNullException(nameof(combatMath));
        _trySelectTravelerTarget = trySelectTravelerTarget ?? throw new ArgumentNullException(nameof(trySelectTravelerTarget));
        _askBoostPointsIfAvailable = askBoostPointsIfAvailable ?? throw new ArgumentNullException(nameof(askBoostPointsIfAvailable));
    }

    public bool TryResolveBasicAttack(TravelerTurnContext travelerTurnContext)
    {
        var selectedWeapon = TrySelectWeapon(travelerTurnContext);
        if (selectedWeapon is null)
        {
            return false;
        }

        var selectedTarget = _trySelectTravelerTarget(travelerTurnContext);
        if (selectedTarget is null)
        {
            return false;
        }

        _askBoostPointsIfAvailable(travelerTurnContext);
        var basicAttackContext = new TravelerBasicAttackContext(travelerTurnContext, selectedWeapon, selectedTarget);
        ExecuteTravelerBasicAttack(basicAttackContext);
        return true;
    }

    private string? TrySelectWeapon(TravelerTurnContext travelerTurnContext)
    {
        var selectedWeapon = _view.AskWeaponSelection(travelerTurnContext.Traveler.Weapons);
        if (selectedWeapon == travelerTurnContext.Traveler.Weapons.Count + 1)
        {
            return null;
        }

        return travelerTurnContext.Traveler.Weapons[selectedWeapon - 1];
    }

    private void ExecuteTravelerBasicAttack(TravelerBasicAttackContext basicAttackContext)
    {
        var combatState = basicAttackContext.TravelerTurnContext.CombatState;
        var targetState = combatState.GetUnitState(basicAttackContext.TargetBeast);
        var hasWeakness = _combatMath.HasWeaknessAgainstAttackType(basicAttackContext.TargetBeast, basicAttackContext.WeaponType);
        var wasInBreakingPoint = targetState.IsInBreakingPoint;

        var baseDamage = _combatMath.CalculateTravelerBasicAttackRawDamage(basicAttackContext, BasicAttackModifier);
        var damageMultiplier = _combatMath.GetTravelerDamageMultiplier(hasWeakness, wasInBreakingPoint);
        var dealtDamage = _combatMath.ApplyMultiplier(baseDamage, damageMultiplier);

        var enteredBreakingPoint = false;
        if (hasWeakness && dealtDamage > 0 && !wasInBreakingPoint)
        {
            enteredBreakingPoint = combatState.TryConsumeBeastShield(basicAttackContext.TargetBeast);
        }

        var targetCurrentHp = combatState.ApplyDamage(basicAttackContext.TargetBeast, dealtDamage);
        _view.ShowTravelerAttackResult(new TravelerAttackViewData(
            basicAttackContext.TravelerTurnContext.Traveler.Name,
            basicAttackContext.TargetBeast.Unit.Name,
            basicAttackContext.WeaponType,
            dealtDamage,
            hasWeakness,
            enteredBreakingPoint,
            targetCurrentHp));
    }
}

