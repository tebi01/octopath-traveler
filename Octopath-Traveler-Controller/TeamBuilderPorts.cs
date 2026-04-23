using Octopath_Traveler_Model;

namespace Octopath_Traveler;

internal interface TeamCatalogProvider
{
    Catalogs Load(string teamFilePath);
}

internal interface TeamLineReader
{
    string[] ReadAllLines(string teamFilePath);
}

internal interface TeamParser
{
    ParsedTeam ParseTeamFile(string[] lines);
}

internal interface TeamValidator
{
    void ValidateTeam(ParsedTeam parsedTeam, Catalogs catalogs);
}

internal interface ParsedTeamProvider
{
    ParsedTeam Load(string teamFilePath, Catalogs catalogs);
}

internal interface TeamGameStateBuilder
{
    GameState Build(ParsedTeam parsedTeam, Catalogs catalogs);
}

