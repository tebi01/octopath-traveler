using Octopath_Traveler_Model;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

// This class can't leave in the model, since TeamsInfo is defined in the view
public class TeamsBuilder
{
    private TeamsInfo _teamsInfo;
    private GameState _gameState;

    public TeamsBuilder(TeamsInfo teamsInfo, GameState gameState)
    {
        _teamsInfo = teamsInfo;
        _gameState = gameState;
    }

    public void Build()
    {
        // This method should use the _teamsInfo to build the teams
        // Is not necessary to return anything, just update the _gameState with the built teams
        // If the teams info contain information invalid just throw an exception
        // For now we will just throw the exception since there is nothing implemented
        throw new Exception(); 
        // We can't use the message of the exception to show the invalid team's message
        // Since that message should be built in the view
    }
}