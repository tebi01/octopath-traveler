namespace Octopath_Traveler;

internal sealed class TeamFileLineReader : TeamLineReader
{
    public string[] ReadAllLines(string teamFilePath)
    {
        return File.ReadAllLines(teamFilePath);
    }
}

