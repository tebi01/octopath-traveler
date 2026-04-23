namespace Octopath_Traveler;

internal static class TeamTravelerLineParser
{
    public static ParsedTravelerLine Parse(string line)
    {
        var normalizedLine = line.Trim();
        var travelerSections = ExtractTravelerSections(normalizedLine);
        return BuildParsedTravelerLine(travelerSections);
    }

    private static (string Name, string? ActiveSkillsContent, string? PassiveSkillsContent)
        ExtractTravelerSections(string normalizedLine)
    {
        var travelerName = ExtractTravelerName(normalizedLine);
        var contentAfterName = normalizedLine.Substring(travelerName.Length).Trim();

        var (afterActiveSkills, activeSkillsContent) = ConsumeOptionalSection(contentAfterName, '(', ')');
        var (afterPassiveSkills, passiveSkillsContent) = ConsumeOptionalSection(afterActiveSkills, '[', ']');
        EnsureNoUnexpectedSymbols(afterPassiveSkills);

        return (travelerName.Trim(), activeSkillsContent, passiveSkillsContent);
    }

    private static ParsedTravelerLine BuildParsedTravelerLine(
        (string Name, string? ActiveSkillsContent, string? PassiveSkillsContent) travelerSections)
    {
        EnsureTravelerName(travelerSections.Name);
        var activeSkills = TeamSkillListParser.Parse(travelerSections.ActiveSkillsContent);
        var passiveSkills = TeamSkillListParser.Parse(travelerSections.PassiveSkillsContent);
        return new ParsedTravelerLine(travelerSections.Name, activeSkills, passiveSkills);
    }

    private static void EnsureTravelerName(string travelerName)
    {
        if (string.IsNullOrWhiteSpace(travelerName))
        {
            throw new InvalidOperationException("Traveler name is required.");
        }
    }

    private static string ExtractTravelerName(string line)
    {
        var activeSkillsStart = line.IndexOf('(');
        var passiveSkillsStart = line.IndexOf('[');
        var firstSectionStart = GetFirstPositiveIndex(activeSkillsStart, passiveSkillsStart);

        return firstSectionStart < 0
            ? line
            : line[..firstSectionStart];
    }

    private static (string Remaining, string? Content) ConsumeOptionalSection(string text, char sectionStart, char sectionEnd)
    {
        var startIndex = text.IndexOf(sectionStart);
        var endIndex = text.IndexOf(sectionEnd);

        if (SectionIsMissing(startIndex, endIndex))
        {
            return (text.Trim(), null);
        }

        EnsureValidSectionBounds(startIndex, endIndex);
        EnsureSingleSectionPair(text, sectionStart, sectionEnd);

        var content = text[(startIndex + 1)..endIndex];
        var remaining = (text[..startIndex] + text[(endIndex + 1)..]).Trim();
        return (remaining, content);
    }

    private static bool SectionIsMissing(int startIndex, int endIndex)
    {
        return startIndex < 0 && endIndex < 0;
    }

    private static void EnsureValidSectionBounds(int startIndex, int endIndex)
    {
        if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
        {
            throw new InvalidOperationException("Invalid traveler line format.");
        }
    }

    private static void EnsureSingleSectionPair(string text, char sectionStart, char sectionEnd)
    {
        var startIndex = text.IndexOf(sectionStart);
        var endIndex = text.IndexOf(sectionEnd);
        if (HasExtraDelimiter(text, sectionStart, startIndex) || HasExtraDelimiter(text, sectionEnd, endIndex))
        {
            throw new InvalidOperationException("Invalid traveler line format.");
        }
    }

    private static bool HasExtraDelimiter(string text, char delimiter, int firstDelimiterIndex)
    {
        return text.IndexOf(delimiter, firstDelimiterIndex + 1) >= 0;
    }

    private static void EnsureNoUnexpectedSymbols(string remaining)
    {
        if (remaining.Contains('(') || remaining.Contains(')') || remaining.Contains('[') || remaining.Contains(']'))
        {
            throw new InvalidOperationException("Invalid traveler line format.");
        }

        if (!string.IsNullOrWhiteSpace(remaining))
        {
            throw new InvalidOperationException("Invalid traveler line format.");
        }
    }

    private static int GetFirstPositiveIndex(int first, int second)
    {
        if (first < 0)
        {
            return second;
        }

        if (second < 0)
        {
            return first;
        }

        return Math.Min(first, second);
    }

}


