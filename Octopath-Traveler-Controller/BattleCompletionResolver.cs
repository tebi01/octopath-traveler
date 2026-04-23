using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class BattleCompletionResolver
{
    private readonly MainConsoleView _view;

    public BattleCompletionResolver(MainConsoleView view)
    {
        _view = view;
    }

    public bool TryFinishBattle(CombatFlowState combatState)
    {
        var battleOutcome = EvaluateBattleResult(combatState);
        if (battleOutcome == BattleResult.Ongoing)
        {
            return false;
        }

        combatState.FinishBattle(battleOutcome);
        ShowWinner(battleOutcome);
        return true;
    }

    private static BattleResult EvaluateBattleResult(CombatFlowState combatState)
    {
        if (!combatState.GetAliveBeasts().Any())
        {
            return BattleResult.PlayerVictory;
        }

        if (!combatState.GetAliveTravelers().Any())
        {
            return BattleResult.PlayerDefeat;
        }

        return BattleResult.Ongoing;
    }

    private void ShowWinner(BattleResult result)
    {
        if (result == BattleResult.PlayerVictory)
        {
            _view.ShowPlayerWinMessage();
            return;
        }

        _view.ShowEnemyWinMessage();
    }
}

