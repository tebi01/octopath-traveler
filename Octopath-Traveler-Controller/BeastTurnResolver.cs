using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class BeastTurnResolver
{
    private readonly MainConsoleView _view;
    private readonly BeastSkillCatalog _beastSkillCatalog;
    private readonly Func<int, int, double, double> _calculatePhysicalDamageRaw;
    private readonly Func<int, int, double, double> _calculateElementalDamageRaw;

    public BeastTurnResolver(
        MainConsoleView view,
        BeastSkillCatalog beastSkillCatalog,
        Func<int, int, double, double> calculatePhysicalDamageRaw,
        Func<int, int, double, double> calculateElementalDamageRaw)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _beastSkillCatalog = beastSkillCatalog ?? throw new ArgumentNullException(nameof(beastSkillCatalog));
        _calculatePhysicalDamageRaw = calculatePhysicalDamageRaw ?? throw new ArgumentNullException(nameof(calculatePhysicalDamageRaw));
        _calculateElementalDamageRaw = calculateElementalDamageRaw ?? throw new ArgumentNullException(nameof(calculateElementalDamageRaw));
    }

    public void ResolveTurn(CombatFlowState combatState, TurnEntry beastTurn)
    {
        var actingBeast = GetBeastFromTurn(beastTurn);
        var beastSkill = _beastSkillCatalog.GetByName(actingBeast.Skill);

        if (beastSkill.IsArea)
        {
            ExecuteAreaBeastSkill(combatState, actingBeast, beastSkill);
        }
        else
        {
            ExecuteSingleTargetBeastSkill(combatState, actingBeast, beastSkill);
        }

        combatState.CompleteTurn();
    }

    private static Beast GetBeastFromTurn(TurnEntry beastTurn)
    {
        return beastTurn.UnitReference.Unit as Beast
            ?? throw new InvalidOperationException("Beast turn expected a beast unit.");
    }

    private void ExecuteSingleTargetBeastSkill(CombatFlowState combatState, Beast beast, BeastSkillSpec beastSkill)
    {
        var targetTraveler = SelectBeastTarget(combatState, beastSkill.TargetRule);
        var attackResult = ResolveBeastAttackAgainstTarget(combatState, beast, beastSkill, targetTraveler);

        _view.ShowBeastAttackResult(new BeastAttackViewData(
            beast.Name,
            beastSkill.Name,
            targetTraveler.Unit.Name,
            ResolveBeastDamageLabel(beastSkill.AttackKind),
            attackResult.Damage,
            attackResult.TargetWasDefending,
            attackResult.TargetCurrentHp));
    }

    private void ExecuteAreaBeastSkill(CombatFlowState combatState, Beast beast, BeastSkillSpec beastSkill)
    {
        var targets = combatState.GetAliveTravelers();
        var results = new List<BeastAreaTargetViewData>();

        foreach (var target in targets)
        {
            var targetResult = ResolveBeastAttackAgainstTarget(combatState, beast, beastSkill, target);
            results.Add(new BeastAreaTargetViewData(
                target.Unit.Name,
                targetResult.Damage,
                targetResult.TargetWasDefending,
                targetResult.TargetCurrentHp));
        }

        _view.ShowBeastAreaAttackResult(new BeastAreaAttackViewData(
            beast.Name,
            beastSkill.Name,
            ResolveBeastDamageLabel(beastSkill.AttackKind),
            results));
    }

    private (int Damage, bool TargetWasDefending, int TargetCurrentHp) ResolveBeastAttackAgainstTarget(
        CombatFlowState combatState,
        Beast beast,
        BeastSkillSpec beastSkill,
        UnitReference targetTraveler)
    {
        var targetState = combatState.GetUnitState(targetTraveler);
        var targetWasDefending = targetState.IsDefending && !beastSkill.IgnoresDefend;

        var totalDamage = 0;
        var hitCount = Math.Max(1, beastSkill.Hits);

        for (var hitIndex = 0; hitIndex < hitCount; hitIndex++)
        {
            var hitDamage = CalculateBeastHitDamage(beast, targetTraveler, targetState, beastSkill, targetWasDefending);
            if (hitDamage <= 0)
            {
                continue;
            }

            totalDamage += hitDamage;
            _ = combatState.ApplyDamage(targetTraveler, hitDamage);

            if (targetState.CurrentHp <= 0)
            {
                break;
            }
        }

        return (totalDamage, targetWasDefending, targetState.CurrentHp);
    }

    private int CalculateBeastHitDamage(
        Beast beast,
        UnitReference targetTraveler,
        CombatUnitState targetState,
        BeastSkillSpec beastSkill,
        bool targetWasDefending)
    {
        if (beastSkill.AttackKind == BeastAttackKind.HalveCurrentHp)
        {
            var remainingHp = targetState.CurrentHp / 2;
            return Math.Max(0, targetState.CurrentHp - remainingHp);
        }

        var rawDamage = beastSkill.AttackKind == BeastAttackKind.Elemental
            ? _calculateElementalDamageRaw(beast.Stats.ElementalAttack, targetTraveler.Unit.Stats.ElementalDefense, beastSkill.Modifier)
            : _calculatePhysicalDamageRaw(beast.Stats.PhysicalAttack, targetTraveler.Unit.Stats.PhysicalDefense, beastSkill.Modifier);

        var mitigatedDamage = targetWasDefending ? rawDamage * 0.5 : rawDamage;
        return Math.Max(0, Convert.ToInt32(Math.Floor(mitigatedDamage)));
    }

    private static string ResolveBeastDamageLabel(BeastAttackKind attackKind)
    {
        if (attackKind == BeastAttackKind.HalveCurrentHp)
        {
            return string.Empty;
        }

        return attackKind == BeastAttackKind.Elemental ? "elemental" : "físico";
    }

    private static UnitReference SelectBeastTarget(CombatFlowState combatState, BeastTargetRule targetRule)
    {
        var travelers = combatState.GetAliveTravelers();

        return targetRule switch
        {
            BeastTargetRule.HighestElementalAttack => travelers
                .OrderByDescending(reference => reference.Unit.Stats.ElementalAttack)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            BeastTargetRule.LowestPhysicalDefense => travelers
                .OrderBy(reference => reference.Unit.Stats.PhysicalDefense)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            BeastTargetRule.HighestSpeed => travelers
                .OrderByDescending(reference => reference.Unit.Stats.Speed)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            BeastTargetRule.LowestElementalDefense => travelers
                .OrderBy(reference => reference.Unit.Stats.ElementalDefense)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            BeastTargetRule.HighestPhysicalAttack => travelers
                .OrderByDescending(reference => reference.Unit.Stats.PhysicalAttack)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            BeastTargetRule.HighestPhysicalDefense => travelers
                .OrderByDescending(reference => reference.Unit.Stats.PhysicalDefense)
                .ThenBy(reference => reference.BoardPosition)
                .First(),
            _ => travelers
                .OrderByDescending(reference => combatState.GetUnitState(reference).CurrentHp)
                .ThenBy(reference => reference.BoardPosition)
                .First()
        };
    }

}

