using Octopath_Traveler_Model;

namespace Octopath_Traveler_View;

public class MainConsoleView
{
    private View _view;
    private string _teamsFolder;
    // We have a reference to GameState to consume the model's information, but we can't update it since the view is not responsible for that
    private GameState _gameState;

    public MainConsoleView(GameState gameState, View view, string teamsFolder)
    {
        _gameState = gameState;
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
        string[] files = Directory.GetFiles(_teamsFolder);
        files = files.Order().ToArray(); 
        for (int i = 0; i < files.Length; i++)
            _view.WriteLine($"{i}: {Path.GetFileName(files[i])}");
        _view.ReadLine();

        // For now we will just return an empty teams info, since there is nothing implemented
        return new TeamsInfo();
    }

    // Both of the following methods can be used for adding new features
    // But after refactoring, they should be removed 
    // And the Controller only should interact with the view with abstract methods like those above

    public void WriteLine(string message)
    {
        _view.WriteLine(message);
    }

    public string ReadLine()
    {
        return _view.ReadLine();
    }
}