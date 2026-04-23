namespace Octopath_Traveler;

internal sealed class TeamParserAdapter : TeamParser
{
    public ParsedTeam ParseTeamFile(string[] lines)
    {
        return TeamFileParser.ParseTeamFile(lines);
    }
}

