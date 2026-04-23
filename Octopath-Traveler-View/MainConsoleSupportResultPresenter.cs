namespace Octopath_Traveler_View;

internal sealed class MainConsoleSupportResultPresenter
{
    private readonly View _view;

    public MainConsoleSupportResultPresenter(View view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
    }

    public void ShowTravelerHealingSkillResult(TravelerHealingSkillViewData healingData)
    {
        PrintSeparator();
        _view.WriteLine($"{healingData.TravelerName} usa {healingData.SkillName}");

        foreach (var target in healingData.Targets)
        {
            _view.WriteLine($"{target.TargetName} recupera {target.RecoveredHp} de vida");
        }

        ShowFinalHpByTarget(healingData.Targets.Select(target => new TravelerSkillFinalHpViewData(target.TargetName, target.TargetCurrentHp)).ToList());
    }

    public void ShowTravelerReviveSkillResult(TravelerReviveSkillViewData reviveData)
    {
        PrintSeparator();
        _view.WriteLine($"{reviveData.TravelerName} usa {reviveData.SkillName}");

        foreach (var target in reviveData.Targets)
        {
            if (target.IsRevived)
            {
                _view.WriteLine($"{target.TargetName} revive");
            }
        }

        foreach (var target in reviveData.Targets)
        {
            if (target.RecoveredHp > 0)
            {
                _view.WriteLine($"{target.TargetName} recupera {target.RecoveredHp} de vida");
            }
        }

        ShowFinalHpByTarget(reviveData.Targets.Select(target => new TravelerSkillFinalHpViewData(target.TargetName, target.TargetCurrentHp)).ToList());
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

