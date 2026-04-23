namespace Octopath_Traveler;

internal sealed class ParsedTeamLoader : ParsedTeamProvider
{
    private readonly TeamLineReader _teamLineReader;
    private readonly TeamParser _teamParser;
    private readonly TeamValidator _teamValidator;

    public ParsedTeamLoader(TeamLineReader teamLineReader, TeamParser teamParser, TeamValidator teamValidator)
    {
        _teamLineReader = teamLineReader ?? throw new ArgumentNullException(nameof(teamLineReader));
        _teamParser = teamParser ?? throw new ArgumentNullException(nameof(teamParser));
        _teamValidator = teamValidator ?? throw new ArgumentNullException(nameof(teamValidator));
    }

    public ParsedTeam Load(string teamFilePath, Catalogs catalogs)
    {
        var teamFileLines = _teamLineReader.ReadAllLines(teamFilePath);
        var parsedTeam = _teamParser.ParseTeamFile(teamFileLines);
        _teamValidator.ValidateTeam(parsedTeam, catalogs);
        return parsedTeam;
    }
}

