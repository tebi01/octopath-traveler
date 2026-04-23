using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler_View;

public sealed class MainConsoleView
{
    private readonly MainConsoleActionResultPresenter _actionResultPresenter;
    private readonly MainConsoleTeamSelectionPresenter _teamSelectionPresenter;
    private readonly MainConsolePromptPresenter _promptPresenter;
    private readonly MainConsoleCombatStatusPresenter _combatStatusPresenter;

    public MainConsoleView(View view, string teamsFolder)
    {
        var inputReader = new MainConsoleInputReader(view);
        _actionResultPresenter = new MainConsoleActionResultPresenter(view);
        _teamSelectionPresenter = new MainConsoleTeamSelectionPresenter(view, inputReader, teamsFolder);
        _promptPresenter = new MainConsolePromptPresenter(view, inputReader);
        _combatStatusPresenter = new MainConsoleCombatStatusPresenter(view);
    }

    public void ShowInvalidTeamMessage()
    {
        _actionResultPresenter.ShowInvalidTeamMessage();
    }

    public TeamsInfo SelectTeamInfo()
    {
        return _teamSelectionPresenter.SelectTeamInfo();
    }

    public void ShowRoundStart(int roundNumber)
    {
        _actionResultPresenter.ShowRoundStart(roundNumber);
    }

    public void ShowCombatStatus(CombatViewSnapshot snapshot)
    {
        _combatStatusPresenter.ShowCombatStatus(snapshot);
    }

    public void ShowCombatStatusWithLeadingSeparator(CombatViewSnapshot snapshot)
    {
        _combatStatusPresenter.ShowCombatStatusWithLeadingSeparator(snapshot);
    }

    public int AskTravelerMainAction(string travelerName)
    {
        return _promptPresenter.AskTravelerMainAction(travelerName);
    }

    public int AskWeaponSelection(IReadOnlyList<string> weapons)
    {
        return _promptPresenter.AskWeaponSelection(weapons);
    }

    public int AskTravelerTarget(string travelerName, IReadOnlyList<UnitDisplaySnapshot> enemies)
    {
        return _promptPresenter.AskTravelerTarget(travelerName, enemies);
    }

    public int AskAllyTarget(string travelerName, IReadOnlyList<UnitDisplaySnapshot> allies)
    {
        return _promptPresenter.AskAllyTarget(travelerName, allies);
    }

    public int AskTravelerSkill(string travelerName, IReadOnlyList<string> activeSkills)
    {
        return _promptPresenter.AskTravelerSkill(travelerName, activeSkills);
    }

    public int AskBoostPointsToUse()
    {
        return _promptPresenter.AskBoostPointsToUse();
    }

    public void ShowTravelerAttackResult(TravelerAttackViewData attackData)
    {
        _actionResultPresenter.ShowTravelerAttackResult(attackData);
    }

    public void ShowBeastAttackResult(BeastAttackViewData attackData)
    {
        _actionResultPresenter.ShowBeastAttackResult(attackData);
    }

    public void ShowBeastAreaAttackResult(BeastAreaAttackViewData attackData)
    {
        _actionResultPresenter.ShowBeastAreaAttackResult(attackData);
    }

    public void ShowTravelerSkillAttackResult(TravelerSkillAttackViewData attackData)
    {
        _actionResultPresenter.ShowTravelerSkillAttackResult(attackData);
    }

    public void ShowLegholdTrapResult(LegholdTrapViewData trapData)
    {
        _actionResultPresenter.ShowLegholdTrapResult(trapData);
    }

    public void ShowTravelerSkillAreaAttackResult(TravelerSkillAreaAttackViewData attackData)
    {
        _actionResultPresenter.ShowTravelerSkillAreaAttackResult(attackData);
    }

    public void ShowTravelerSkillMultiHitAreaAttackResult(TravelerSkillMultiHitAreaAttackViewData attackData)
    {
        _actionResultPresenter.ShowTravelerSkillMultiHitAreaAttackResult(attackData);
    }

    public void ShowTravelerHealingSkillResult(TravelerHealingSkillViewData healingData)
    {
        _actionResultPresenter.ShowTravelerHealingSkillResult(healingData);
    }

    public void ShowTravelerReviveSkillResult(TravelerReviveSkillViewData reviveData)
    {
        _actionResultPresenter.ShowTravelerReviveSkillResult(reviveData);
    }

    public void ShowFleeMessage()
    {
        _actionResultPresenter.ShowFleeMessage();
    }

    public void ShowPlayerWinMessage()
    {
        _actionResultPresenter.ShowPlayerWinMessage();
    }

    public void ShowEnemyWinMessage()
    {
        _actionResultPresenter.ShowEnemyWinMessage();
    }
}