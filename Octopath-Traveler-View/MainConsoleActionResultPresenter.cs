namespace Octopath_Traveler_View;

internal sealed class MainConsoleActionResultPresenter
{
    private readonly MainConsoleBattleMessagePresenter _battleMessagePresenter;
    private readonly MainConsoleAttackResultPresenter _attackResultPresenter;
    private readonly MainConsoleSupportResultPresenter _supportResultPresenter;

    public MainConsoleActionResultPresenter(View view)
    {
        _battleMessagePresenter = new MainConsoleBattleMessagePresenter(view);
        _attackResultPresenter = new MainConsoleAttackResultPresenter(view);
        _supportResultPresenter = new MainConsoleSupportResultPresenter(view);
    }

    public void ShowInvalidTeamMessage()
    {
        _battleMessagePresenter.ShowInvalidTeamMessage();
    }

    public void ShowRoundStart(int roundNumber)
    {
        _battleMessagePresenter.ShowRoundStart(roundNumber);
    }

    public void ShowTravelerAttackResult(TravelerAttackViewData attackData)
    {
        _attackResultPresenter.ShowTravelerAttackResult(attackData);
    }

    public void ShowBeastAttackResult(BeastAttackViewData attackData)
    {
        _attackResultPresenter.ShowBeastAttackResult(attackData);
    }

    public void ShowBeastAreaAttackResult(BeastAreaAttackViewData attackData)
    {
        _attackResultPresenter.ShowBeastAreaAttackResult(attackData);
    }

    public void ShowTravelerSkillAttackResult(TravelerSkillAttackViewData attackData)
    {
        _attackResultPresenter.ShowTravelerSkillAttackResult(attackData);
    }

    public void ShowLegholdTrapResult(LegholdTrapViewData trapData)
    {
        _attackResultPresenter.ShowLegholdTrapResult(trapData);
    }

    public void ShowTravelerSkillAreaAttackResult(TravelerSkillAreaAttackViewData attackData)
    {
        _attackResultPresenter.ShowTravelerSkillAreaAttackResult(attackData);
    }

    public void ShowTravelerSkillMultiHitAreaAttackResult(TravelerSkillMultiHitAreaAttackViewData attackData)
    {
        _attackResultPresenter.ShowTravelerSkillMultiHitAreaAttackResult(attackData);
    }

    public void ShowTravelerHealingSkillResult(TravelerHealingSkillViewData healingData)
    {
        _supportResultPresenter.ShowTravelerHealingSkillResult(healingData);
    }

    public void ShowTravelerReviveSkillResult(TravelerReviveSkillViewData reviveData)
    {
        _supportResultPresenter.ShowTravelerReviveSkillResult(reviveData);
    }

    public void ShowFleeMessage()
    {
        _battleMessagePresenter.ShowFleeMessage();
    }

    public void ShowPlayerWinMessage()
    {
        _battleMessagePresenter.ShowPlayerWinMessage();
    }

    public void ShowEnemyWinMessage()
    {
        _battleMessagePresenter.ShowEnemyWinMessage();
    }
}

