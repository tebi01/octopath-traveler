using Octopath_Traveler_Model;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

public class Game
{
    private MainConsoleView _view;
    private GameState _state = new();
    public Game(View view, string teamsFolder)
    {
        _view = new MainConsoleView(_state, view, teamsFolder);
    }

    public void Play()
    {
        try
        {
            TeamsInfo info = _view.SelectTeamInfo();
            TeamsBuilder builder = new TeamsBuilder(info, _state);
            builder.Build();
        }
        catch (Exception)
        {
            _view.ShowInvalidTeamMessage();
        }
    }
}