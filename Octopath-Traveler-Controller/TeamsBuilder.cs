using System.Text.Json;
using Octopath_Traveler_Model;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

public sealed class TeamsBuilder
{
    private const string PlayerTeamHeader = "Player Team";
    private const string EnemyTeamHeader = "Enemy Team";
    private const int MinTravelers = 1;
    private const int MaxTravelers = 4;
    private const int MinBeasts = 1;
    private const int MaxBeasts = 5;

    private readonly TeamsInfo _teamsInfo;

    public TeamsBuilder(TeamsInfo teamsInfo)
    {
        _teamsInfo = teamsInfo;
    }

    public GameState Build()
    {
        var catalogs = LoadCatalogs();
        var parsedTeam = LoadAndValidateTeam(catalogs);
        var playerTeam = BuildPlayerTeam(parsedTeam, catalogs);
        var enemyTeam = BuildEnemyTeam(parsedTeam, catalogs);
        return new GameState(playerTeam, enemyTeam);
    }

    private ParsedTeam LoadAndValidateTeam(Catalogs catalogs)
    {
        var teamFileLines = File.ReadAllLines(_teamsInfo.TeamFilePath);
        var parsedTeam = ParseTeamFile(teamFileLines);
        ValidateTeam(parsedTeam, catalogs);
        return parsedTeam;
    }

    private static PlayerTeam BuildPlayerTeam(ParsedTeam parsedTeam, Catalogs catalogs)
    {
        var travelers = parsedTeam.Travelers
            .Select(travelerLine => BuildTraveler(travelerLine, catalogs))
            .ToList();
        return new PlayerTeam(travelers);
    }

    private static EnemyTeam BuildEnemyTeam(ParsedTeam parsedTeam, Catalogs catalogs)
    {
        var beasts = parsedTeam.Beasts
            .Select(beastName => BuildBeast(beastName, catalogs))
            .ToList();
        return new EnemyTeam(beasts);
    }

    private Catalogs LoadCatalogs()
    {
        var dataFolder = GetDataFolderPath();
        var characters = ReadCatalog<List<CharacterDto>>(dataFolder, "characters.json");
        var enemies = ReadCatalog<List<EnemyDto>>(dataFolder, "enemies.json");
        var activeSkills = ReadCatalog<List<SkillDto>>(dataFolder, "skills.json");
        var passiveSkills = ReadCatalog<List<SkillDto>>(dataFolder, "passive_skills.json");

        return BuildCatalogs(characters, enemies, activeSkills, passiveSkills);
    }

    private string GetDataFolderPath()
    {
        return Path.GetDirectoryName(Path.GetDirectoryName(_teamsInfo.TeamFilePath))
               ?? throw new InvalidOperationException("Invalid teams folder path.");
    }

    private static T ReadCatalog<T>(string dataFolder, string fileName)
    {
        return ReadJson<T>(Path.Combine(dataFolder, fileName));
    }

    private static Catalogs BuildCatalogs(
        List<CharacterDto> characters,
        List<EnemyDto> enemies,
        List<SkillDto> activeSkills,
        List<SkillDto> passiveSkills)
    {
        return new Catalogs(
            characters.ToDictionary(character => character.Name, StringComparer.OrdinalIgnoreCase),
            enemies.ToDictionary(enemy => enemy.Name, StringComparer.OrdinalIgnoreCase),
            activeSkills.Select(skill => skill.Name).ToHashSet(StringComparer.OrdinalIgnoreCase),
            passiveSkills.Select(skill => skill.Name).ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private static T ReadJson<T>(string path)
    {
        var json = File.ReadAllText(path);
        var value = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return value ?? throw new InvalidOperationException($"Unable to parse {path}");
    }

    private static ParsedTeam ParseTeamFile(string[] lines)
    {
        EnsureMinimumTeamFileLength(lines);
        var normalizedLines = NormalizeTeamLines(lines);
        EnsurePlayerTeamHeader(normalizedLines);

        var enemyTeamIndex = FindEnemyTeamIndex(normalizedLines);
        var travelerLines = ParseTravelerLines(normalizedLines, enemyTeamIndex);
        var beastNames = ParseBeastNames(normalizedLines, enemyTeamIndex);
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

    private static List<ParsedTravelerLine> ParseTravelerLines(IReadOnlyList<string> normalizedLines, int enemyTeamIndex)
    {
        return normalizedLines.Skip(1).Take(enemyTeamIndex - 1).Select(ParseTravelerLine).ToList();
    }

    private static List<string> ParseBeastNames(IReadOnlyList<string> normalizedLines, int enemyTeamIndex)
    {
        return normalizedLines.Skip(enemyTeamIndex + 1).ToList();
    }

    private static ParsedTravelerLine ParseTravelerLine(string line)
    {
        var normalizedLine = line.Trim();
        var travelerSections = ExtractTravelerSections(normalizedLine);
        return BuildParsedTravelerLine(travelerSections);
    }

    private static TravelerSections ExtractTravelerSections(string normalizedLine)
    {
        var travelerName = ExtractTravelerName(normalizedLine);
        var contentAfterName = normalizedLine.Substring(travelerName.Length).Trim();

        var (afterActiveSkills, activeSkillsContent) = ConsumeOptionalSection(contentAfterName, '(', ')');
        var (afterPassiveSkills, passiveSkillsContent) = ConsumeOptionalSection(afterActiveSkills, '[', ']');
        EnsureNoUnexpectedSymbols(afterPassiveSkills);

        return new TravelerSections(travelerName.Trim(), activeSkillsContent, passiveSkillsContent);
    }

    private static ParsedTravelerLine BuildParsedTravelerLine(TravelerSections travelerSections)
    {
        EnsureTravelerName(travelerSections.Name);
        var activeSkills = SplitSkills(travelerSections.ActiveSkillsContent);
        var passiveSkills = SplitSkills(travelerSections.PassiveSkillsContent);
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
        EnsureSingleSectionPair(text, sectionStart, sectionEnd, startIndex, endIndex);

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

    private static void EnsureSingleSectionPair(string text, char sectionStart, char sectionEnd, int startIndex, int endIndex)
    {
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

    private static IReadOnlyList<string> SplitSkills(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(skill => skill.Trim())
            .Where(skill => skill.Length > 0)
            .ToList();
    }

    private static void ValidateTeam(ParsedTeam parsedTeam, Catalogs catalogs)
    {
        ValidateTeamSize(parsedTeam);
        ValidateTeamDuplicates(parsedTeam);
        ValidateTravelers(parsedTeam.Travelers, catalogs);
        ValidateBeasts(parsedTeam.Beasts, catalogs);
    }

    private static void ValidateTeamSize(ParsedTeam parsedTeam)
    {
        if (parsedTeam.Travelers.Count is < MinTravelers or > MaxTravelers)
        {
            throw new InvalidOperationException("Invalid traveler count.");
        }

        if (parsedTeam.Beasts.Count is < MinBeasts or > MaxBeasts)
        {
            throw new InvalidOperationException("Invalid beast count.");
        }
    }

    private static void ValidateTeamDuplicates(ParsedTeam parsedTeam)
    {
        EnsureNoDuplicates(parsedTeam.Travelers.Select(t => t.Name));
        EnsureNoDuplicates(parsedTeam.Beasts);
    }

    private static void ValidateTravelers(IEnumerable<ParsedTravelerLine> travelers, Catalogs catalogs)
    {
        foreach (var traveler in travelers)
        {
            ValidateTravelerExists(traveler, catalogs);
            ValidateTravelerSkillCounts(traveler);
            ValidateTravelerSkillDuplicates(traveler);
            ValidateTravelerSkillsExist(traveler, catalogs);
        }
    }

    private static void ValidateTravelerExists(ParsedTravelerLine traveler, Catalogs catalogs)
    {
        if (!catalogs.Characters.ContainsKey(traveler.Name))
        {
            throw new InvalidOperationException("Unknown traveler.");
        }
    }

    private static void ValidateTravelerSkillCounts(ParsedTravelerLine traveler)
    {
        if (traveler.ActiveSkills.Count > Traveler.MaxActiveSkills || traveler.PassiveSkills.Count > Traveler.MaxPassiveSkills)
        {
            throw new InvalidOperationException("Too many skills.");
        }
    }

    private static void ValidateTravelerSkillDuplicates(ParsedTravelerLine traveler)
    {
        EnsureNoDuplicates(traveler.ActiveSkills);
        EnsureNoDuplicates(traveler.PassiveSkills);
    }

    private static void ValidateTravelerSkillsExist(ParsedTravelerLine traveler, Catalogs catalogs)
    {
        ValidateKnownSkills(traveler.ActiveSkills, catalogs.ActiveSkills, "Unknown active skill.");
        ValidateKnownSkills(traveler.PassiveSkills, catalogs.PassiveSkills, "Unknown passive skill.");
    }

    private static void ValidateKnownSkills(IEnumerable<string> skills, HashSet<string> catalogSkills, string errorMessage)
    {
        foreach (var skill in skills)
        {
            if (!catalogSkills.Contains(skill))
            {
                throw new InvalidOperationException(errorMessage);
            }
        }
    }

    private static void ValidateBeasts(IEnumerable<string> beasts, Catalogs catalogs)
    {
        foreach (var beast in beasts)
        {
            if (!catalogs.Enemies.ContainsKey(beast))
            {
                throw new InvalidOperationException("Unknown beast.");
            }
        }
    }

    private static void EnsureNoDuplicates(IEnumerable<string> names)
    {
        var list = names.ToList();
        var unique = new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
        if (list.Count != unique.Count)
        {
            throw new InvalidOperationException("Duplicated entries are not allowed.");
        }
    }

    private static Traveler BuildTraveler(ParsedTravelerLine travelerLine, Catalogs catalogs)
    {
        var character = catalogs.Characters[travelerLine.Name];
        var stats = BuildCharacterCombatStats(character);
        var sp = BuildCharacterSkillPoints(character);

        return new Traveler(
            character.Name,
            stats,
            sp,
            character.Weapons,
            travelerLine.ActiveSkills,
            travelerLine.PassiveSkills);
    }

    private static Beast BuildBeast(string beastName, Catalogs catalogs)
    {
        var enemy = catalogs.Enemies[beastName];
        var stats = BuildEnemyCombatStats(enemy);

        return new Beast(enemy.Name, stats, enemy.Skill, enemy.Shields, enemy.Weaknesses);
    }

    private static CombatStats BuildCharacterCombatStats(CharacterDto character)
    {
        return new CombatStats(
            character.Stats.HP,
            character.Stats.HP,
            character.Stats.PhysAtk,
            character.Stats.PhysDef,
            character.Stats.ElemAtk,
            character.Stats.ElemDef,
            character.Stats.Speed);
    }

    private static SkillPoints BuildCharacterSkillPoints(CharacterDto character)
    {
        return new SkillPoints(character.Stats.SP, character.Stats.SP);
    }

    private static CombatStats BuildEnemyCombatStats(EnemyDto enemy)
    {
        return new CombatStats(
            enemy.Stats.HP,
            enemy.Stats.HP,
            enemy.Stats.PhysAtk,
            enemy.Stats.PhysDef,
            enemy.Stats.ElemAtk,
            enemy.Stats.ElemDef,
            enemy.Stats.Speed);
    }

    private sealed record ParsedTeam(IReadOnlyList<ParsedTravelerLine> Travelers, IReadOnlyList<string> Beasts);

    private sealed record ParsedTravelerLine(
        string Name,
        IReadOnlyList<string> ActiveSkills,
        IReadOnlyList<string> PassiveSkills);

    private sealed record TravelerSections(
        string Name,
        string? ActiveSkillsContent,
        string? PassiveSkillsContent);

    private sealed record Catalogs(
        Dictionary<string, CharacterDto> Characters,
        Dictionary<string, EnemyDto> Enemies,
        HashSet<string> ActiveSkills,
        HashSet<string> PassiveSkills);

    private sealed class CharacterDto
    {
        public required string Name { get; init; }
        public required CharacterStatsDto Stats { get; init; }
        public required List<string> Weapons { get; init; }
    }

    private sealed class CharacterStatsDto
    {
        public int HP { get; init; }
        public int SP { get; init; }
        public int PhysAtk { get; init; }
        public int PhysDef { get; init; }
        public int ElemAtk { get; init; }
        public int ElemDef { get; init; }
        public int Speed { get; init; }
    }

    private sealed class EnemyDto
    {
        public required string Name { get; init; }
        public required EnemyStatsDto Stats { get; init; }
        public required string Skill { get; init; }
        public int Shields { get; init; }
        public required List<string> Weaknesses { get; init; }
    }

    private sealed class EnemyStatsDto
    {
        public int HP { get; init; }
        public int PhysAtk { get; init; }
        public int PhysDef { get; init; }
        public int ElemAtk { get; init; }
        public int ElemDef { get; init; }
        public int Speed { get; init; }
    }

    private sealed class SkillDto
    {
        public required string Name { get; init; }
    }
}