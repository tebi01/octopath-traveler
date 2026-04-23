namespace Octopath_Traveler;

internal interface TeamParser
{
    ParsedTeam ParseTeamFile(string[] lines);
}
