using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class TravelerSpecialOffensiveSkillResolver
{
    private const string NightmareChimeraSkillName = "Nightmare Chimera";
    private const string ShootingStarsSkillName = "Shooting Stars";

    private static readonly IReadOnlyList<string> PhysicalSkillWeapons = new[]
    {
        "Sword", "Spear", "Dagger", "Axe", "Bow", "Stave"
    };

    private static readonly IReadOnlyList<string> ShootingStarsHitTypes = new[]
    {
        "Wind", "Light", "Dark"
    };

    private readonly MainConsoleView _view;
    private readonly Func<TravelerTurnContext, UnitReference?> _trySelectTravelerTarget;
    private readonly Func<TravelerTurnContext, int, bool> _tryConsumeTravelerSpWithBoostPrompt;
    private readonly Func<TravelerTurnContext, bool> _completeTravelerTurn;
    private readonly Func<UnitReference, string, bool> _hasWeaknessAgainstAttackType;
    private readonly Func<bool, bool, double> _getTravelerDamageMultiplier;
    private readonly Func<double, double, int> _applyMultiplier;
    private readonly Func<int, int, double, double> _calculateElementalDamageRaw;
    private readonly Action<TravelerTurnContext, UnitReference, string, OffensiveSkillSpec> _executeSingleTargetOffensiveSkill;

    public TravelerSpecialOffensiveSkillResolver(
        MainConsoleView view,
        Func<TravelerTurnContext, UnitReference?> trySelectTravelerTarget,
        Func<TravelerTurnContext, int, bool> tryConsumeTravelerSpWithBoostPrompt,
        Func<TravelerTurnContext, bool> completeTravelerTurn,
        Func<UnitReference, string, bool> hasWeaknessAgainstAttackType,
        Func<bool, bool, double> getTravelerDamageMultiplier,
        Func<double, double, int> applyMultiplier,
        Func<int, int, double, double> calculateElementalDamageRaw,
        Action<TravelerTurnContext, UnitReference, string, OffensiveSkillSpec> executeSingleTargetOffensiveSkill)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _trySelectTravelerTarget = trySelectTravelerTarget ?? throw new ArgumentNullException(nameof(trySelectTravelerTarget));
        _tryConsumeTravelerSpWithBoostPrompt = tryConsumeTravelerSpWithBoostPrompt
            ?? throw new ArgumentNullException(nameof(tryConsumeTravelerSpWithBoostPrompt));
        _completeTravelerTurn = completeTravelerTurn ?? throw new ArgumentNullException(nameof(completeTravelerTurn));
        _hasWeaknessAgainstAttackType = hasWeaknessAgainstAttackType
            ?? throw new ArgumentNullException(nameof(hasWeaknessAgainstAttackType));
        _getTravelerDamageMultiplier = getTravelerDamageMultiplier
            ?? throw new ArgumentNullException(nameof(getTravelerDamageMultiplier));
        _applyMultiplier = applyMultiplier ?? throw new ArgumentNullException(nameof(applyMultiplier));
        _calculateElementalDamageRaw = calculateElementalDamageRaw ?? throw new ArgumentNullException(nameof(calculateElementalDamageRaw));
        _executeSingleTargetOffensiveSkill = executeSingleTargetOffensiveSkill
            ?? throw new ArgumentNullException(nameof(executeSingleTargetOffensiveSkill));
    }

    public bool TryResolveNightmareChimera(TravelerTurnContext travelerTurnContext)
    {
        var selectedWeapon = _view.AskWeaponSelection(PhysicalSkillWeapons);
        if (selectedWeapon == PhysicalSkillWeapons.Count + 1)
        {
            return false;
        }

        var selectedTarget = _trySelectTravelerTarget(travelerTurnContext);
        if (selectedTarget is null)
        {
            return false;
        }

        if (!_tryConsumeTravelerSpWithBoostPrompt(travelerTurnContext, 35))
        {
            return false;
        }

        var damageType = PhysicalSkillWeapons[selectedWeapon - 1];
        _executeSingleTargetOffensiveSkill(
            travelerTurnContext,
            selectedTarget,
            NightmareChimeraSkillName,
            new OffensiveSkillSpec(35, 1.9, damageType, IsElemental: false, IsMercyStrike: false));

        return _completeTravelerTurn(travelerTurnContext);
    }

    public bool TryResolveShootingStars(TravelerTurnContext travelerTurnContext)
    {
        if (!_tryConsumeTravelerSpWithBoostPrompt(travelerTurnContext, 35))
        {
            return false;
        }

        var combatState = travelerTurnContext.CombatState;
        var targets = combatState.GetAliveBeasts();
        var hitResults = new List<TravelerSkillAreaHitViewData>();

        foreach (var target in targets)
        {
            foreach (var hitType in ShootingStarsHitTypes)
            {
                var targetState = combatState.GetUnitState(target);
                var hasWeakness = _hasWeaknessAgainstAttackType(target, hitType);
                var wasInBreakingPoint = targetState.IsInBreakingPoint;
                var baseDamage = _calculateElementalDamageRaw(
                    travelerTurnContext.Traveler.Stats.ElementalAttack,
                    target.Unit.Stats.ElementalDefense,
                    0.9);

                var dealtDamage = _applyMultiplier(baseDamage, _getTravelerDamageMultiplier(hasWeakness, wasInBreakingPoint));
                var enteredBreakingPoint = false;
                if (hasWeakness && dealtDamage > 0 && !wasInBreakingPoint)
                {
                    enteredBreakingPoint = combatState.TryConsumeBeastShield(target);
                }

                _ = combatState.ApplyDamage(target, dealtDamage);
                hitResults.Add(new TravelerSkillAreaHitViewData(
                    target.Unit.Name,
                    dealtDamage,
                    hitType,
                    hasWeakness,
                    enteredBreakingPoint));
            }
        }

        var finalHpByTarget = targets
            .Select(target => new TravelerSkillFinalHpViewData(target.Unit.Name, combatState.GetUnitState(target).CurrentHp))
            .ToList();

        _view.ShowTravelerSkillMultiHitAreaAttackResult(new TravelerSkillMultiHitAreaAttackViewData(
            travelerTurnContext.Traveler.Name,
            ShootingStarsSkillName,
            hitResults,
            finalHpByTarget));

        return _completeTravelerTurn(travelerTurnContext);
    }
}

