namespace Octopath_Traveler;

internal static class TeamFileParser
{
    private const string PlayerTeamHeader = "Player Team";
    private const string EnemyTeamHeader = "Enemy Team";

    public static ParsedTeam ParseTeamFile(string[] lines)
    {
        EnsureMinimumTeamFileLength(lines);
        var normalizedLines = NormalizeTeamLines(lines);
        var sections = ExtractSections(normalizedLines);
        var travelerLines = ParseTravelerLines(sections.TravelerLines);
        var beastNames = ParseBeastNames(sections.BeastLines);
        return new ParsedTeam(travelerLines, beastNames);
    }

    private static void EnsureMinimumTeamFileLength(string[] lines)
    {
        if (lines.Length < 4)
        {
            throw new InvalidOperationException("Invalid team file format.");
        }
    }

    private static List<string> NormalizeTeamLines(IEnumerable<string> lines)
    {
        return lines.Select(line => line.Trim()).Where(line => line.Length > 0).ToList();
    }

    private static void EnsurePlayerTeamHeader(IReadOnlyList<string> normalizedLines)
    {
        if (normalizedLines.Count < 4 || !string.Equals(normalizedLines[0], PlayerTeamHeader, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid team file format.");
        }
    }

    private static TeamFileSections ExtractSections(IReadOnlyList<string> normalizedLines)
    {
        EnsurePlayerTeamHeader(normalizedLines);
        var enemyTeamIndex = FindEnemyTeamIndex(normalizedLines);
        var travelerLines = normalizedLines.Skip(1).Take(enemyTeamIndex - 1).ToList();
        var beastLines = normalizedLines.Skip(enemyTeamIndex + 1).ToList();
        return new TeamFileSections(travelerLines, beastLines);
    }

    private static int FindEnemyTeamIndex(IReadOnlyList<string> normalizedLines)
    {
        var enemyTeamIndex = -1;
        for (var index = 0; index < normalizedLines.Count; index++)
        {
            if (string.Equals(normalizedLines[index], EnemyTeamHeader, StringComparison.Ordinal))
            {
                enemyTeamIndex = index;
                break;
            }
        }

        if (enemyTeamIndex <= 1 || enemyTeamIndex == normalizedLines.Count - 1)
        {
            throw new InvalidOperationException("Invalid team file format.");
        }

        return enemyTeamIndex;
    }

    private static List<ParsedTravelerLine> ParseTravelerLines(IEnumerable<string> travelerLines)
    {
        return travelerLines
            .Select(TeamTravelerLineParser.Parse)
            .ToList();
    }

    private static List<string> ParseBeastNames(IEnumerable<string> beastLines)
    {
        return beastLines.ToList();
    }

    private sealed record TeamFileSections(IReadOnlyList<string> TravelerLines, IReadOnlyList<string> BeastLines);
}

