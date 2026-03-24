using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler_View;

public sealed class MainConsoleView
{
    private const string Separator = "----------------------------------------";

    private readonly View _view;
    private readonly string _teamsFolder;

    public MainConsoleView(GameState gameState, View view, string teamsFolder)
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
        for (var index = 0; index < files.Length; index++)
        {
            _view.WriteLine($"{index}: {Path.GetFileName(files[index])}");
        }

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

        _view.WriteLine("Equipo del jugador");
        foreach (var unit in snapshot.PlayerTeam)
        {
            _view.WriteLine($"{unit.BoardSlot}-{unit.Name} - HP:{unit.CurrentHp}/{unit.MaxHp} SP:{unit.CurrentSp}/{unit.MaxSp} BP:{unit.CurrentBp}");
        }

        _view.WriteLine("Equipo del enemigo");
        foreach (var unit in snapshot.EnemyTeam)
        {
            _view.WriteLine($"{unit.BoardSlot}-{unit.Name} - HP:{unit.CurrentHp}/{unit.MaxHp} Shields:{unit.CurrentShields}");
        }

        PrintSeparator();
        _view.WriteLine("Turnos de la ronda");
        ShowTurns(snapshot.CurrentRoundTurns);

        PrintSeparator();
        _view.WriteLine("Turnos de la siguiente ronda");
        ShowTurns(snapshot.NextRoundTurns);
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
        PrintSeparator();
        _view.WriteLine("Seleccione un arma");
        for (var index = 0; index < weapons.Count; index++)
        {
            _view.WriteLine($"{index + 1}: {weapons[index]}");
        }

        var cancelOption = weapons.Count + 1;
        _view.WriteLine($"{cancelOption}: Cancelar");
        return ReadOption(1, cancelOption);
    }

    public int AskTravelerTarget(string travelerName, IReadOnlyList<UnitDisplaySnapshot> enemies)
    {
        PrintSeparator();
        _view.WriteLine($"Seleccione un objetivo para {travelerName}");
        for (var index = 0; index < enemies.Count; index++)
        {
            var enemy = enemies[index];
            _view.WriteLine($"{index + 1}: {enemy.Name} - HP:{enemy.CurrentHp}/{enemy.MaxHp} Shields:{enemy.CurrentShields}");
        }

        var cancelOption = enemies.Count + 1;
        _view.WriteLine($"{cancelOption}: Cancelar");
        return ReadOption(1, cancelOption);
    }

    public int AskTravelerSkill(string travelerName, IReadOnlyList<string> activeSkills)
    {
        PrintSeparator();
        _view.WriteLine($"Seleccione una habilidad para {travelerName}");
        for (var index = 0; index < activeSkills.Count; index++)
        {
            _view.WriteLine($"{index + 1}: {activeSkills[index]}");
        }

        var cancelOption = activeSkills.Count + 1;
        _view.WriteLine($"{cancelOption}: Cancelar");
        return ReadOption(1, cancelOption);
    }

    public int AskBoostPointsToUse()
    {
        PrintSeparator();
        _view.WriteLine("Seleccione cuantos BP utilizar");
        return ReadNonNegativeInt();
    }

    public void ShowTravelerAttackResult(string attackerName, string targetName, string weaponType, int damage, int targetCurrentHp)
    {
        PrintSeparator();
        _view.WriteLine($"{attackerName} ataca");
        _view.WriteLine($"{targetName} recibe {damage} de daño de tipo {weaponType}");
        _view.WriteLine($"{targetName} termina con HP:{targetCurrentHp}");
    }

    public void ShowBeastAttackResult(string beastName, string targetName, int damage, int targetCurrentHp)
    {
        PrintSeparator();
        _view.WriteLine($"{beastName} usa Attack");
        _view.WriteLine($"{targetName} recibe {damage} de daño físico");
        _view.WriteLine($"{targetName} termina con HP:{targetCurrentHp}");
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