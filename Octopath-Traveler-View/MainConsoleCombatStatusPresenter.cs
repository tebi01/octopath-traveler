using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler_View;

internal sealed class MainConsoleCombatStatusPresenter
{
    private readonly View _view;

    public MainConsoleCombatStatusPresenter(View view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
    }

    public void ShowCombatStatus(CombatViewSnapshot snapshot)
    {
        ShowCombatStatusCore(snapshot);
    }

    public void ShowCombatStatusWithLeadingSeparator(CombatViewSnapshot snapshot)
    {
        PrintSeparator();
        ShowCombatStatusCore(snapshot);
    }

    private void ShowCombatStatusCore(CombatViewSnapshot snapshot)
    {
        ShowPlayerTeam(snapshot.PlayerTeam);
        ShowEnemyTeam(snapshot.EnemyTeam);
        ShowRoundQueues(snapshot);
    }

    private void ShowPlayerTeam(IReadOnlyList<UnitDisplaySnapshot> players)
    {
        _view.WriteLine("Equipo del jugador");
        foreach (var player in players)
        {
            _view.WriteLine($"{player.BoardSlot}-{player.Name} - HP:{player.CurrentHp}/{player.MaxHp} SP:{player.CurrentSp}/{player.MaxSp} BP:{player.CurrentBp}");
        }
    }

    private void ShowEnemyTeam(IReadOnlyList<UnitDisplaySnapshot> enemies)
    {
        _view.WriteLine("Equipo del enemigo");
        foreach (var enemy in enemies)
        {
            _view.WriteLine($"{enemy.BoardSlot}-{enemy.Name} - HP:{enemy.CurrentHp}/{enemy.MaxHp} Shields:{enemy.CurrentShields}");
        }
    }

    private void ShowRoundQueues(CombatViewSnapshot snapshot)
    {
        PrintSeparator();
        _view.WriteLine("Turnos de la ronda");
        ShowTurns(snapshot.CurrentRoundTurns);

        PrintSeparator();
        _view.WriteLine("Turnos de la siguiente ronda");
        ShowTurns(snapshot.NextRoundTurns);
    }

    private void ShowTurns(IReadOnlyList<string> turnNames)
    {
        for (var index = 0; index < turnNames.Count; index++)
        {
            _view.WriteLine($"{index + 1}.{turnNames[index]}");
        }
    }

    private void PrintSeparator()
    {
        _view.WriteLine(MainConsoleUiConstants.Separator);
    }
}

