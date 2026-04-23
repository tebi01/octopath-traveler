namespace Octopath_Traveler;

internal interface TeamDataFolderPathResolver
{
    string ResolveFromTeamFilePath(string teamFilePath);
}

internal sealed class DefaultTeamDataFolderPathResolver : TeamDataFolderPathResolver
{
    public string ResolveFromTeamFilePath(string teamFilePath)
    {
        return Path.GetDirectoryName(Path.GetDirectoryName(teamFilePath))
               ?? throw new InvalidOperationException("Invalid teams folder path.");
    }
}

