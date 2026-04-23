namespace Octopath_Traveler;

internal static class TeamSkillListParser
{
    public static IReadOnlyList<string> Parse(string? rawSkills)
    {
        if (string.IsNullOrWhiteSpace(rawSkills))
        {
            return Array.Empty<string>();
        }

        return rawSkills
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(skill => skill.Trim())
            .Where(skill => skill.Length > 0)
            .ToList();
    }
}

