using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class TravelerOffensiveSkillResolver
{
    private readonly MainConsoleView _view;
    private readonly TravelerTargetSelector _trySelectTravelerTarget;
    private readonly Func<TravelerTurnContext, int, bool> _tryConsumeTravelerSpWithBoostPrompt;
    private readonly Func<TravelerTurnContext, bool> _completeTravelerTurn;
    private readonly Func<UnitReference, string, bool> _hasWeaknessAgainstAttackType;
    private readonly Func<TravelerDamageMultiplierContext, double> _getTravelerDamageMultiplier;
    private readonly Func<double, double, int> _applyMultiplier;
    private readonly Func<int, int, double, double> _calculatePhysicalDamageRaw;
    private readonly Func<int, int, double, double> _calculateElementalDamageRaw;
    private readonly Func<TravelerTurnContext, UnitReference, double, double> _calculateLastStandRawDamage;
    private readonly TravelerSpecialOffensiveSkillResolver _specialSkillResolver;

    public TravelerOffensiveSkillResolver(
        MainConsoleView view,
        TravelerTargetSelector trySelectTravelerTarget,
        Func<TravelerTurnContext, int, bool> tryConsumeTravelerSpWithBoostPrompt,
        Func<TravelerTurnContext, bool> completeTravelerTurn,
        Func<UnitReference, string, bool> hasWeaknessAgainstAttackType,
        Func<TravelerDamageMultiplierContext, double> getTravelerDamageMultiplier,
        Func<double, double, int> applyMultiplier,
        Func<int, int, double, double> calculatePhysicalDamageRaw,
        Func<int, int, double, double> calculateElementalDamageRaw,
        Func<TravelerTurnContext, UnitReference, double, double> calculateLastStandRawDamage)
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
        _calculatePhysicalDamageRaw = calculatePhysicalDamageRaw ?? throw new ArgumentNullException(nameof(calculatePhysicalDamageRaw));
        _calculateElementalDamageRaw = calculateElementalDamageRaw ?? throw new ArgumentNullException(nameof(calculateElementalDamageRaw));
        _calculateLastStandRawDamage = calculateLastStandRawDamage
            ?? throw new ArgumentNullException(nameof(calculateLastStandRawDamage));

        _specialSkillResolver = new TravelerSpecialOffensiveSkillResolver(
            _view,
            _trySelectTravelerTarget,
            _tryConsumeTravelerSpWithBoostPrompt,
            _completeTravelerTurn,
            _hasWeaknessAgainstAttackType,
            _getTravelerDamageMultiplier,
            _applyMultiplier,
            _calculateElementalDamageRaw,
            ExecuteSingleTargetOffensiveSkill);
    }

    public bool TryResolveSingleTargetOffensiveSkill(TravelerTurnContext travelerTurnContext, string skillName)
    {
        if (!TravelerSkillSpecs.TryGetSingleTargetOffensiveSkill(skillName, out var skill))
        {
            return false;
        }

        if (!_trySelectTravelerTarget(travelerTurnContext, out var selectedTarget))
        {
            return false;
        }

        if (!_tryConsumeTravelerSpWithBoostPrompt(travelerTurnContext, skill.SpCost))
        {
            return false;
        }

        ExecuteSingleTargetOffensiveSkill(travelerTurnContext, selectedTarget, skillName, skill);
        return _completeTravelerTurn(travelerTurnContext);
    }

    public bool TryResolveEnemiesTargetOffensiveSkill(TravelerTurnContext travelerTurnContext, string skillName)
    {
        if (!TravelerSkillSpecs.TryGetEnemiesTargetOffensiveSkill(skillName, out var skill))
        {
            return false;
        }

        if (!_tryConsumeTravelerSpWithBoostPrompt(travelerTurnContext, skill.SpCost))
        {
            return false;
        }

        var areaResults = ExecuteEnemiesTargetOffensiveSkill(travelerTurnContext, skillName, skill);
        _view.ShowTravelerSkillAreaAttackResult(new TravelerSkillAreaAttackViewData(
            travelerTurnContext.Traveler.Name,
            skillName,
            skill.DamageType,
            areaResults));
        return _completeTravelerTurn(travelerTurnContext);
    }

    public bool TryResolveNightmareChimera(TravelerTurnContext travelerTurnContext)
    {
        return _specialSkillResolver.TryResolveNightmareChimera(travelerTurnContext);
    }

    public bool TryResolveShootingStars(TravelerTurnContext travelerTurnContext)
    {
        return _specialSkillResolver.TryResolveShootingStars(travelerTurnContext);
    }

    private IReadOnlyList<TravelerSkillAreaTargetViewData> ExecuteEnemiesTargetOffensiveSkill(
        TravelerTurnContext travelerTurnContext,
        string skillName,
        OffensiveSkillSpec skill)
    {
        var results = new List<TravelerSkillAreaTargetViewData>();
        var combatState = travelerTurnContext.CombatState;
        var targets = combatState.GetAliveBeasts();

        foreach (var target in targets)
        {
            var baseDamage = ResolveBaseDamageForAreaSkill(travelerTurnContext, target, skillName, skill);
            var resolution = ResolveOffensiveHit(travelerTurnContext, target, skill, baseDamage);
            results.Add(new TravelerSkillAreaTargetViewData(
                target.Unit.Name,
                resolution.DealtDamage,
                resolution.HasWeakness,
                resolution.EnteredBreakingPoint,
                resolution.TargetCurrentHp));
        }

        return results;
    }

    private void ExecuteSingleTargetOffensiveSkill(
        TravelerTurnContext travelerTurnContext,
        UnitReference target,
        string skillName,
        OffensiveSkillSpec skill)
    {
        var baseDamage = ResolveBaseDamage(travelerTurnContext, target, skill);
        var resolution = ResolveOffensiveHit(travelerTurnContext, target, skill, baseDamage);
        _view.ShowTravelerSkillAttackResult(new TravelerSkillAttackViewData(
            travelerTurnContext.Traveler.Name,
            skillName,
            target.Unit.Name,
            skill.DamageType,
            resolution.DealtDamage,
            resolution.HasWeakness,
            resolution.EnteredBreakingPoint,
            resolution.TargetCurrentHp));
    }

    private (int DealtDamage, bool HasWeakness, bool EnteredBreakingPoint, int TargetCurrentHp) ResolveOffensiveHit(
        TravelerTurnContext travelerTurnContext,
        UnitReference target,
        OffensiveSkillSpec skill,
        double baseDamage)
    {
        var combatState = travelerTurnContext.CombatState;
        var targetState = combatState.GetUnitState(target);
        var hasWeakness = _hasWeaknessAgainstAttackType(target, skill.DamageType);
        var wasInBreakingPoint = targetState.IsInBreakingPoint;

        var damageContext = hasWeakness
            ? (wasInBreakingPoint
                ? TravelerDamageMultiplierContext.WeaknessAndBreakingPoint
                : TravelerDamageMultiplierContext.WeaknessOnly)
            : (wasInBreakingPoint
                ? TravelerDamageMultiplierContext.BreakingPointOnly
                : TravelerDamageMultiplierContext.None);
        var dealtDamage = _applyMultiplier(baseDamage, _getTravelerDamageMultiplier(damageContext));
        if (skill.IsMercyStrike)
        {
            dealtDamage = Math.Min(dealtDamage, Math.Max(0, targetState.CurrentHp - 1));
        }

        var enteredBreakingPoint = hasWeakness
                                   && dealtDamage > 0
                                   && !wasInBreakingPoint
                                   && combatState.TryConsumeBeastShield(target);

        var targetCurrentHp = combatState.ApplyDamage(target, dealtDamage);
        return (dealtDamage, hasWeakness, enteredBreakingPoint, targetCurrentHp);
    }

    private double ResolveBaseDamageForAreaSkill(
        TravelerTurnContext travelerTurnContext,
        UnitReference target,
        string skillName,
        OffensiveSkillSpec skill)
    {
        if (string.Equals(skillName, "Last Stand", StringComparison.Ordinal))
        {
            return _calculateLastStandRawDamage(travelerTurnContext, target, skill.Modifier);
        }

        return ResolveBaseDamage(travelerTurnContext, target, skill);
    }

    private double ResolveBaseDamage(
        TravelerTurnContext travelerTurnContext,
        UnitReference target,
        OffensiveSkillSpec skill)
    {
        return skill.IsElemental
            ? _calculateElementalDamageRaw(travelerTurnContext.Traveler.Stats.ElementalAttack, target.Unit.Stats.ElementalDefense, skill.Modifier)
            : _calculatePhysicalDamageRaw(travelerTurnContext.Traveler.Stats.PhysicalAttack, target.Unit.Stats.PhysicalDefense, skill.Modifier);
    }

}

