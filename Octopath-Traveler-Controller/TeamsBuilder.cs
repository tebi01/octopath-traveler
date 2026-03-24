using System.Text.Json;
using System.Text.RegularExpressions;
using Octopath_Traveler_Model;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

// This class can't leave in the model, since TeamsInfo is defined in the view
public sealed class TeamsBuilder
{
    private static readonly Regex TravelerLineRegex = new(
        "^(?<name>[^\\(\\[\\]]+?)\\s*(\\((?<active>[^\\)]*)\\))?\\s*(\\[(?<passive>[^\\]]*)\\])?\\s*$",
        RegexOptions.Compiled);

    private readonly TeamsInfo _teamsInfo;

    public TeamsBuilder(TeamsInfo teamsInfo)
    {
        _teamsInfo = teamsInfo;
    }

    public GameState Build()
    {
        var catalogs = LoadCatalogs();
        var teamFileLines = File.ReadAllLines(_teamsInfo.TeamFilePath);
        var parsedTeam = ParseTeamFile(teamFileLines);

        ValidateTeam(parsedTeam, catalogs);

        var travelers = parsedTeam.Travelers
            .Select(travelerLine => BuildTraveler(travelerLine, catalogs))
            .ToList();

        var beasts = parsedTeam.Beasts
            .Select(beastName => BuildBeast(beastName, catalogs))
            .ToList();

        var playerTeam = new PlayerTeam(travelers);
        var enemyTeam = new EnemyTeam(beasts);
        return new GameState(playerTeam, enemyTeam);
    }

    private Catalogs LoadCatalogs()
    {
        var dataFolder = Path.GetDirectoryName(Path.GetDirectoryName(_teamsInfo.TeamFilePath))
                         ?? throw new InvalidOperationException("Invalid teams folder path.");

        var characters = ReadJson<List<CharacterDto>>(Path.Combine(dataFolder, "characters.json"));
        var enemies = ReadJson<List<EnemyDto>>(Path.Combine(dataFolder, "enemies.json"));
        var skills = ReadJson<List<SkillDto>>(Path.Combine(dataFolder, "skills.json"));
        var passiveSkills = ReadJson<List<SkillDto>>(Path.Combine(dataFolder, "passive_skills.json"));

        return new Catalogs(
            characters.ToDictionary(character => character.Name, StringComparer.OrdinalIgnoreCase),
            enemies.ToDictionary(enemy => enemy.Name, StringComparer.OrdinalIgnoreCase),
            skills.Select(skill => skill.Name).ToHashSet(StringComparer.OrdinalIgnoreCase),
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
        if (lines.Length < 4)
        {
            throw new InvalidOperationException("Invalid team file format.");
        }

        var normalizedLines = lines.Select(line => line.Trim()).Where(line => line.Length > 0).ToList();
        if (normalizedLines.Count < 4 || !string.Equals(normalizedLines[0], "Player Team", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid team file format.");
        }

        var enemyTeamIndex = normalizedLines.FindIndex(line => string.Equals(line, "Enemy Team", StringComparison.Ordinal));
        if (enemyTeamIndex <= 1 || enemyTeamIndex == normalizedLines.Count - 1)
        {
            throw new InvalidOperationException("Invalid team file format.");
        }

        var travelerLines = normalizedLines.Skip(1).Take(enemyTeamIndex - 1).Select(ParseTravelerLine).ToList();
        var beastNames = normalizedLines.Skip(enemyTeamIndex + 1).ToList();

        return new ParsedTeam(travelerLines, beastNames);
    }

    private static ParsedTravelerLine ParseTravelerLine(string line)
    {
        var match = TravelerLineRegex.Match(line);
        if (!match.Success)
        {
            throw new InvalidOperationException("Invalid traveler line format.");
        }

        var name = match.Groups["name"].Value.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Traveler name is required.");
        }

        var activeSkills = SplitSkills(match.Groups["active"].Value);
        var passiveSkills = SplitSkills(match.Groups["passive"].Value);

        return new ParsedTravelerLine(name, activeSkills, passiveSkills);
    }

    private static IReadOnlyList<string> SplitSkills(string raw)
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
        if (parsedTeam.Travelers.Count is < 1 or > 4)
        {
            throw new InvalidOperationException("Invalid traveler count.");
        }

        if (parsedTeam.Beasts.Count is < 1 or > 5)
        {
            throw new InvalidOperationException("Invalid beast count.");
        }

        EnsureNoDuplicates(parsedTeam.Travelers.Select(t => t.Name));
        EnsureNoDuplicates(parsedTeam.Beasts);

        foreach (var traveler in parsedTeam.Travelers)
        {
            if (!catalogs.Characters.ContainsKey(traveler.Name))
            {
                throw new InvalidOperationException("Unknown traveler.");
            }

            if (traveler.ActiveSkills.Count > Traveler.MaxActiveSkills || traveler.PassiveSkills.Count > Traveler.MaxPassiveSkills)
            {
                throw new InvalidOperationException("Too many skills.");
            }

            EnsureNoDuplicates(traveler.ActiveSkills);
            EnsureNoDuplicates(traveler.PassiveSkills);

            foreach (var skill in traveler.ActiveSkills)
            {
                if (!catalogs.ActiveSkills.Contains(skill))
                {
                    throw new InvalidOperationException("Unknown active skill.");
                }
            }

            foreach (var passiveSkill in traveler.PassiveSkills)
            {
                if (!catalogs.PassiveSkills.Contains(passiveSkill))
                {
                    throw new InvalidOperationException("Unknown passive skill.");
                }
            }
        }

        foreach (var beast in parsedTeam.Beasts)
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

        var stats = new CombatStats(
            character.Stats.HP,
            character.Stats.HP,
            character.Stats.PhysAtk,
            character.Stats.PhysDef,
            character.Stats.ElemAtk,
            character.Stats.ElemDef,
            character.Stats.Speed);

        var sp = new SkillPoints(character.Stats.SP, character.Stats.SP);

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

        var stats = new CombatStats(
            enemy.Stats.HP,
            enemy.Stats.HP,
            enemy.Stats.PhysAtk,
            enemy.Stats.PhysDef,
            enemy.Stats.ElemAtk,
            enemy.Stats.ElemDef,
            enemy.Stats.Speed);

        return new Beast(enemy.Name, stats, enemy.Skill, enemy.Shields, enemy.Weaknesses);
    }

    private sealed record ParsedTeam(IReadOnlyList<ParsedTravelerLine> Travelers, IReadOnlyList<string> Beasts);

    private sealed record ParsedTravelerLine(
        string Name,
        IReadOnlyList<string> ActiveSkills,
        IReadOnlyList<string> PassiveSkills);

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