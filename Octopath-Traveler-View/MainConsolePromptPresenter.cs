using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler_View;

internal sealed class MainConsolePromptPresenter
{
    private readonly View _view;
    private readonly MainConsoleInputReader _inputReader;

    public MainConsolePromptPresenter(View view, MainConsoleInputReader inputReader)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _inputReader = inputReader ?? throw new ArgumentNullException(nameof(inputReader));
    }

    public int AskTravelerMainAction(string travelerName)
    {
        PrintSeparator();
        _view.WriteLine($"Turno de {travelerName}");
        _view.WriteLine("1: Ataque básico");
        _view.WriteLine("2: Usar habilidad");
        _view.WriteLine("3: Defender");
        _view.WriteLine("4: Huir");
        return _inputReader.ReadOption(1, 4);
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

    public int AskAllyTarget(string travelerName, IReadOnlyList<UnitDisplaySnapshot> allies)
    {
        var targetOptions = allies
            .Select(ally => $"{ally.Name} - HP:{ally.CurrentHp}/{ally.MaxHp} SP:{ally.CurrentSp}/{ally.MaxSp} BP:{ally.CurrentBp}")
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
        return _inputReader.ReadNonNegativeInt();
    }

    private int AskOptionWithCancel(string title, IReadOnlyList<string> options)
    {
        PrintSeparator();
        _view.WriteLine(title);
        ShowIndexedOptions(options);

        var cancelOption = options.Count + 1;
        _view.WriteLine($"{cancelOption}: {MainConsoleUiConstants.CancelLabel}");
        return _inputReader.ReadOption(1, cancelOption);
    }

    private void ShowIndexedOptions(IReadOnlyList<string> options)
    {
        for (var index = 0; index < options.Count; index++)
        {
            _view.WriteLine($"{index + 1}: {options[index]}");
        }
    }

    private void PrintSeparator()
    {
        _view.WriteLine(MainConsoleUiConstants.Separator);
    }
}

