using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler_View;

public sealed class MainConsoleView
{
    private const string Separator = "----------------------------------------";
    private const string CancelLabel = "Cancelar";

    private readonly View _view;
    private readonly string _teamsFolder;

    public MainConsoleView(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;
    }

    public void ShowInvalidTeamMessage()
    {
        _view.WriteLine("Archivo de equipos no válido");
    }

    public TeamsInfo SelectTeamInfo()
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        var files = Directory.GetFiles(_teamsFolder).Order().ToArray();
        ShowTeamFiles(files);

        var selectedIndex = ReadOption(0, files.Length - 1);
        return new TeamsInfo(files[selectedIndex]);
    }

    public void ShowRoundStart(int roundNumber)
    {
        PrintSeparator();
        _view.WriteLine($"INICIA RONDA {roundNumber}");
        PrintSeparator();
    }

    public void ShowCombatStatus(CombatViewSnapshot snapshot, bool includeLeadingSeparator = false)
    {
        if (includeLeadingSeparator)
        {
            PrintSeparator();
        }

        ShowPlayerTeam(snapshot.PlayerTeam);
        ShowEnemyTeam(snapshot.EnemyTeam);
        ShowRoundQueues(snapshot);
    }

    public int AskTravelerMainAction(string travelerName)
    {
        PrintSeparator();
        _view.WriteLine($"Turno de {travelerName}");
        _view.WriteLine("1: Ataque básico");
        _view.WriteLine("2: Usar habilidad");
        _view.WriteLine("3: Defender");
        _view.WriteLine("4: Huir");
        return ReadOption(1, 4);
    }

    public int AskWeaponSelection(IReadOnlyList<string> weapons)
    {
        return AskOptionWithCancel("Seleccione un arma", weapons);
    }

    public int AskTravelerTarget(string travelerName, IReadOnlyList<UnitDisplaySnapshot> enemies)
    {
        var targetOptions = enemies
            .Select(enemy => $"{enemy.Name} - HP:{enemy.CurrentHp}/{enemy.MaxHp} Shields:{enemy.CurrentShields}")
            .ToList();
        return AskOptionWithCancel($"Seleccione un objetivo para {travelerName}", targetOptions);
    }

    public int AskTravelerSkill(string travelerName, IReadOnlyList<string> activeSkills)
    {
        return AskOptionWithCancel($"Seleccione una habilidad para {travelerName}", activeSkills);
    }

    public int AskBoostPointsToUse()
    {
        PrintSeparator();
        _view.WriteLine("Seleccione cuantos BP utilizar");
        return ReadNonNegativeInt();
    }

    public void ShowTravelerAttackResult(TravelerAttackViewData attackData)
    {
        PrintSeparator();
        _view.WriteLine($"{attackData.AttackerName} ataca");
        _view.WriteLine($"{attackData.TargetName} recibe {attackData.Damage} de daño de tipo {attackData.WeaponType}");
        _view.WriteLine($"{attackData.TargetName} termina con HP:{attackData.TargetCurrentHp}");
    }

    public void ShowBeastAttackResult(BeastAttackViewData attackData)
    {
        PrintSeparator();
        _view.WriteLine($"{attackData.BeastName} usa Attack");
        _view.WriteLine($"{attackData.TargetName} recibe {attackData.Damage} de daño físico");
        _view.WriteLine($"{attackData.TargetName} termina con HP:{attackData.TargetCurrentHp}");
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

    private void ShowTeamFiles(IReadOnlyList<string> files)
    {
        for (var index = 0; index < files.Count; index++)
        {
            _view.WriteLine($"{index}: {Path.GetFileName(files[index])}");
        }
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

    private int AskOptionWithCancel(string title, IReadOnlyList<string> options)
    {
        PrintSeparator();
        _view.WriteLine(title);
        ShowIndexedOptions(options);

        var cancelOption = options.Count + 1;
        _view.WriteLine($"{cancelOption}: {CancelLabel}");
        return ReadOption(1, cancelOption);
    }

    private void ShowIndexedOptions(IReadOnlyList<string> options)
    {
        for (var index = 0; index < options.Count; index++)
        {
            _view.WriteLine($"{index + 1}: {options[index]}");
        }
    }

    private void ShowTurns(IReadOnlyList<string> turnNames)
    {
        for (var index = 0; index < turnNames.Count; index++)
        {
            _view.WriteLine($"{index + 1}.{turnNames[index]}");
        }
    }

    private int ReadOption(int min, int max)
    {
        while (true)
        {
            var input = _view.ReadLine();
            if (int.TryParse(input, out var value) && value >= min && value <= max)
            {
                return value;
            }
        }
    }

    private int ReadNonNegativeInt()
    {
        while (true)
        {
            var input = _view.ReadLine();
            if (int.TryParse(input, out var value) && value >= 0)
            {
                return value;
            }
        }
    }

    private void PrintSeparator()
    {
        _view.WriteLine(Separator);
    }
}