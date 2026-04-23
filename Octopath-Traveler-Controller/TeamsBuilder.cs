using Octopath_Traveler_Model;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

public sealed class TeamsBuilder
{
    private readonly TeamsInfo _teamsInfo;
    private readonly TeamCatalogProvider _catalogProvider;
    private readonly ParsedTeamProvider _parsedTeamProvider;
    private readonly TeamGameStateBuilder _gameStateBuilder;

    public TeamsBuilder(TeamsInfo teamsInfo)
        : this(
            teamsInfo,
            new TeamCatalogLoader(),
            new ParsedTeamLoader(new TeamFileLineReader(), new TeamParserAdapter(), new TeamValidatorAdapter()),
            new TeamGameStateAssembler())
    {
    }

    internal TeamsBuilder(
        TeamsInfo teamsInfo,
        TeamCatalogProvider catalogProvider,
        ParsedTeamProvider parsedTeamProvider,
        TeamGameStateBuilder gameStateBuilder)
    {
        _teamsInfo = teamsInfo ?? throw new ArgumentNullException(nameof(teamsInfo));
        _catalogProvider = catalogProvider ?? throw new ArgumentNullException(nameof(catalogProvider));
        _parsedTeamProvider = parsedTeamProvider ?? throw new ArgumentNullException(nameof(parsedTeamProvider));
        _gameStateBuilder = gameStateBuilder ?? throw new ArgumentNullException(nameof(gameStateBuilder));
    }

    public GameState Build()
    {
        var catalogs = LoadCatalogs();
        var parsedTeam = LoadParsedTeam(catalogs);
        return _gameStateBuilder.Build(parsedTeam, catalogs);
    }

    private Catalogs LoadCatalogs()
        => _catalogProvider.Load(_teamsInfo.TeamFilePath);

    private ParsedTeam LoadParsedTeam(Catalogs catalogs)
        => _parsedTeamProvider.Load(_teamsInfo.TeamFilePath, catalogs);
}