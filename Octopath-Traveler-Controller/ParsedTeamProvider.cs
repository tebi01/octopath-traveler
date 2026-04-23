namespace Octopath_Traveler;

internal interface ParsedTeamProvider
{
    ParsedTeam Load(string teamFilePath, Catalogs catalogs);
}
