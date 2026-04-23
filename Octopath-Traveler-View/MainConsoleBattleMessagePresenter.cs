namespace Octopath_Traveler_View;

internal sealed class MainConsoleBattleMessagePresenter
{
    private readonly View _view;

    public MainConsoleBattleMessagePresenter(View view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
    }

    public void ShowInvalidTeamMessage()
    {
        _view.WriteLine("Archivo de equipos no válido");
    }

    public void ShowRoundStart(int roundNumber)
    {
        PrintSeparator();
        _view.WriteLine($"INICIA RONDA {roundNumber}");
        PrintSeparator();
    }

    public void ShowFleeMessage()
    {
        PrintSeparator();
        _view.WriteLine("El equipo de viajeros ha huido!");
    }

    public void ShowPlayerWinMessage()
    {
        PrintSeparator();
        _view.WriteLine("Gana equipo del jugador");
    }

    public void ShowEnemyWinMessage()
    {
        PrintSeparator();
        _view.WriteLine("Gana equipo del enemigo");
    }

    private void PrintSeparator()
    {
        _view.WriteLine(MainConsoleUiConstants.Separator);
    }
}

