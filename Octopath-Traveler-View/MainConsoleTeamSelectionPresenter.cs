namespace Octopath_Traveler_View;

internal sealed class MainConsoleTeamSelectionPresenter
{
    private readonly View _view;
    private readonly MainConsoleInputReader _inputReader;
    private readonly string _teamsFolder;

    public MainConsoleTeamSelectionPresenter(View view, MainConsoleInputReader inputReader, string teamsFolder)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _inputReader = inputReader ?? throw new ArgumentNullException(nameof(inputReader));
        _teamsFolder = teamsFolder ?? throw new ArgumentNullException(nameof(teamsFolder));
    }

    public TeamsInfo SelectTeamInfo()
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        var files = Directory.GetFiles(_teamsFolder, "*.txt", SearchOption.TopDirectoryOnly).Order().ToArray();
        ShowTeamFiles(files);

        var selectedIndex = _inputReader.ReadOption(0, files.Length - 1);
        return new TeamsInfo(files[selectedIndex]);
    }

    private void ShowTeamFiles(IReadOnlyList<string> files)
    {
        for (var index = 0; index < files.Count; index++)
        {
            _view.WriteLine($"{index}: {Path.GetFileName(files[index])}");
        }
    }
}

