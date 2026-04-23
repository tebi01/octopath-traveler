using Octopath_Traveler_Model;

namespace Octopath_Traveler;

internal interface TeamGameStateBuilder
{
    GameState Build(ParsedTeam parsedTeam, Catalogs catalogs);
}
