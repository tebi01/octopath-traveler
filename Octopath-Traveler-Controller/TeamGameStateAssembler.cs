using Octopath_Traveler_Model;

namespace Octopath_Traveler;

internal sealed class TeamGameStateAssembler : TeamGameStateBuilder
{
    public GameState Build(ParsedTeam parsedTeam, Catalogs catalogs)
    {
        var playerTeam = TeamUnitFactory.BuildPlayerTeam(parsedTeam, catalogs);
        var enemyTeam = TeamUnitFactory.BuildEnemyTeam(parsedTeam, catalogs);
        return new GameState(playerTeam, enemyTeam);
    }
}

