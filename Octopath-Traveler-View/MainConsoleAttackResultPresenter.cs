namespace Octopath_Traveler_View;

internal sealed class MainConsoleAttackResultPresenter
{
    private readonly View _view;

    public MainConsoleAttackResultPresenter(View view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
    }

    public void ShowTravelerAttackResult(TravelerAttackViewData attackData)
    {
        PrintSeparator();
        _view.WriteLine($"{attackData.AttackerName} ataca");
        var weaknessSuffix = attackData.HasWeakness ? " con debilidad" : string.Empty;
        _view.WriteLine($"{attackData.TargetName} recibe {attackData.Damage} de daño de tipo {attackData.WeaponType}{weaknessSuffix}");
        if (attackData.TargetEnteredBreakingPoint)
        {
            _view.WriteLine($"{attackData.TargetName} entra en Breaking Point");
        }

        _view.WriteLine($"{attackData.TargetName} termina con HP:{attackData.TargetCurrentHp}");
    }

    public void ShowBeastAttackResult(BeastAttackViewData attackData)
    {
        PrintSeparator();
        _view.WriteLine($"{attackData.BeastName} usa {attackData.SkillName}");
        if (attackData.TargetIsDefending)
        {
            _view.WriteLine($"{attackData.TargetName} se defiende");
        }

        var damageTypeSuffix = string.IsNullOrEmpty(attackData.DamageType) ? string.Empty : $" {attackData.DamageType}";
        _view.WriteLine($"{attackData.TargetName} recibe {attackData.Damage} de daño{damageTypeSuffix}");
        _view.WriteLine($"{attackData.TargetName} termina con HP:{attackData.TargetCurrentHp}");
    }

    public void ShowBeastAreaAttackResult(BeastAreaAttackViewData attackData)
    {
        PrintSeparator();
        _view.WriteLine($"{attackData.BeastName} usa {attackData.SkillName}");
        var damageTypeSuffix = string.IsNullOrEmpty(attackData.DamageType) ? string.Empty : $" {attackData.DamageType}";

        foreach (var target in attackData.Targets)
        {
            if (target.TargetIsDefending)
            {
                _view.WriteLine($"{target.TargetName} se defiende");
            }

            _view.WriteLine($"{target.TargetName} recibe {target.Damage} de daño{damageTypeSuffix}");
        }

        ShowFinalHpByTarget(attackData.Targets.Select(target => new TravelerSkillFinalHpViewData(target.TargetName, target.TargetCurrentHp)).ToList());
    }

    public void ShowTravelerSkillAttackResult(TravelerSkillAttackViewData attackData)
    {
        PrintSeparator();
        _view.WriteLine($"{attackData.TravelerName} usa {attackData.SkillName}");
        var weaknessSuffix = attackData.HasWeakness ? " con debilidad" : string.Empty;
        _view.WriteLine($"{attackData.TargetName} recibe {attackData.Damage} de daño de tipo {attackData.DamageType}{weaknessSuffix}");
        if (attackData.TargetEnteredBreakingPoint)
        {
            _view.WriteLine($"{attackData.TargetName} entra en Breaking Point");
        }

        _view.WriteLine($"{attackData.TargetName} termina con HP:{attackData.TargetCurrentHp}");
    }

    public void ShowLegholdTrapResult(LegholdTrapViewData trapData)
    {
        PrintSeparator();
        _view.WriteLine($"{trapData.TravelerName} usa Leghold Trap");
        _view.WriteLine($"{trapData.TargetName} tendrá menor prioridad de turno durante 2 rondas");
    }

    public void ShowTravelerSkillAreaAttackResult(TravelerSkillAreaAttackViewData attackData)
    {
        PrintSeparator();
        _view.WriteLine($"{attackData.TravelerName} usa {attackData.SkillName}");

        foreach (var target in attackData.Targets)
        {
            var weaknessSuffix = target.HasWeakness ? " con debilidad" : string.Empty;
            _view.WriteLine($"{target.TargetName} recibe {target.Damage} de daño de tipo {attackData.DamageType}{weaknessSuffix}");
            if (target.TargetEnteredBreakingPoint)
            {
                _view.WriteLine($"{target.TargetName} entra en Breaking Point");
            }
        }

        ShowFinalHpByTarget(attackData.Targets.Select(target => new TravelerSkillFinalHpViewData(target.TargetName, target.TargetCurrentHp)).ToList());
    }

    public void ShowTravelerSkillMultiHitAreaAttackResult(TravelerSkillMultiHitAreaAttackViewData attackData)
    {
        PrintSeparator();
        _view.WriteLine($"{attackData.TravelerName} usa {attackData.SkillName}");

        foreach (var hit in attackData.Hits)
        {
            var weaknessSuffix = hit.HasWeakness ? " con debilidad" : string.Empty;
            _view.WriteLine($"{hit.TargetName} recibe {hit.Damage} de daño de tipo {hit.DamageType}{weaknessSuffix}");
            if (hit.TargetEnteredBreakingPoint)
            {
                _view.WriteLine($"{hit.TargetName} entra en Breaking Point");
            }
        }

        ShowFinalHpByTarget(attackData.FinalHpByTarget);
    }

    private void ShowFinalHpByTarget(IReadOnlyList<TravelerSkillFinalHpViewData> targets)
    {
        foreach (var target in targets)
        {
            _view.WriteLine($"{target.TargetName} termina con HP:{target.TargetCurrentHp}");
        }
    }

    private void PrintSeparator()
    {
        _view.WriteLine(MainConsoleUiConstants.Separator);
    }
}

