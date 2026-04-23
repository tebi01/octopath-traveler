using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class TravelerBasicAttackResolver
{
    private const double BasicAttackModifier = 1.3;

    private readonly MainConsoleView _view;
    private readonly TravelerCombatMathService _combatMath;
    private readonly TravelerTargetSelector _trySelectTravelerTarget;
    private readonly Action<TravelerTurnContext> _askBoostPointsIfAvailable;

    public TravelerBasicAttackResolver(
        MainConsoleView view,
        TravelerCombatMathService combatMath,
        TravelerTargetSelector trySelectTravelerTarget,
        Action<TravelerTurnContext> askBoostPointsIfAvailable)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _combatMath = combatMath ?? throw new ArgumentNullException(nameof(combatMath));
        _trySelectTravelerTarget = trySelectTravelerTarget ?? throw new ArgumentNullException(nameof(trySelectTravelerTarget));
        _askBoostPointsIfAvailable = askBoostPointsIfAvailable ?? throw new ArgumentNullException(nameof(askBoostPointsIfAvailable));
    }

    public bool TryResolveBasicAttack(TravelerTurnContext travelerTurnContext)
    {
        if (!TrySelectWeapon(travelerTurnContext, out var selectedWeapon))
        {
            return false;
        }

        if (!_trySelectTravelerTarget(travelerTurnContext, out var selectedTarget))
        {
            return false;
        }

        _askBoostPointsIfAvailable(travelerTurnContext);
        var basicAttackContext = new TravelerBasicAttackContext(travelerTurnContext, selectedWeapon, selectedTarget);
        ExecuteTravelerBasicAttack(basicAttackContext);
        return true;
    }

    private bool TrySelectWeapon(TravelerTurnContext travelerTurnContext, out string selectedWeapon)
    {
        var selectedWeaponIndex = _view.AskWeaponSelection(travelerTurnContext.Traveler.Weapons);
        if (selectedWeaponIndex == travelerTurnContext.Traveler.Weapons.Count + 1)
        {
            selectedWeapon = string.Empty;
            return false;
        }

        selectedWeapon = travelerTurnContext.Traveler.Weapons[selectedWeaponIndex - 1];
        return true;
    }

    private void ExecuteTravelerBasicAttack(TravelerBasicAttackContext basicAttackContext)
    {
        var combatState = basicAttackContext.TravelerTurnContext.CombatState;
        var targetState = combatState.GetUnitState(basicAttackContext.TargetBeast);
        var hasWeakness = _combatMath.HasWeaknessAgainstAttackType(basicAttackContext.TargetBeast, basicAttackContext.WeaponType);
        var wasInBreakingPoint = targetState.IsInBreakingPoint;

        var baseDamage = _combatMath.CalculateTravelerBasicAttackRawDamage(basicAttackContext, BasicAttackModifier);
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

