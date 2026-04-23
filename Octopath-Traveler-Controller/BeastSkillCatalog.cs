using System.Text.Json;

namespace Octopath_Traveler;

public sealed class BeastSkillCatalog
{
    private readonly Dictionary<string, BeastSkillSpec> _skillsByName;

    private BeastSkillCatalog(Dictionary<string, BeastSkillSpec> skillsByName)
    {
        _skillsByName = skillsByName;
    }

    public static BeastSkillCatalog LoadDefault()
    {
        var path = ResolveCatalogPath();
        var rawSkills = ReadCatalog(path);
        var indexedSkills = rawSkills
            .Select(Map)
            .ToDictionary(skill => skill.Name, StringComparer.OrdinalIgnoreCase);

        return new BeastSkillCatalog(indexedSkills);
    }

    public BeastSkillSpec GetByName(string skillName)
    {
        if (_skillsByName.TryGetValue(skillName, out var spec))
        {
            return spec;
        }

        throw new InvalidOperationException($"Unknown beast skill: {skillName}");
    }

    private static string ResolveCatalogPath()
    {
        var localDataPath = Path.Combine("data", "beast_skills.json");
        if (File.Exists(localDataPath))
        {
            return localDataPath;
        }

        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "data", "beast_skills.json");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException("Cannot locate data/beast_skills.json");
    }

    private static IReadOnlyList<BeastSkillDto> ReadCatalog(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<BeastSkillDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Invalid beast skills catalog.");
    }

    private static BeastSkillSpec Map(BeastSkillDto dto)
    {
        var normalizedDescription = dto.Description.Trim().ToLowerInvariant();
        var attackKind = ResolveAttackKind(normalizedDescription, dto.Name);
        var targetRule = ResolveTargetRule(normalizedDescription, dto.Target);
        var ignoresDefend = attackKind == BeastAttackKind.HalveCurrentHp;

        return new BeastSkillSpec(
            dto.Name,
            dto.Modifier,
            dto.Target,
            Math.Max(1, dto.Hits),
            attackKind,
            targetRule,
            ignoresDefend);
    }

    private static BeastAttackKind ResolveAttackKind(string description, string skillName)
    {
        if (description.Contains("reduce a la mitad", StringComparison.Ordinal))
        {
            return BeastAttackKind.HalveCurrentHp;
        }

        if (description.Contains("ataque elemental", StringComparison.Ordinal))
        {
            return BeastAttackKind.Elemental;
        }

        if (description.Contains("ataque fisico", StringComparison.Ordinal)
            || description.Contains("ataque f\u00edsico", StringComparison.Ordinal)
            || string.Equals(skillName, "Attack", StringComparison.OrdinalIgnoreCase))
        {
            return BeastAttackKind.Physical;
        }

        return BeastAttackKind.Physical;
    }

    private static BeastTargetRule ResolveTargetRule(string description, string target)
    {
        if (!string.Equals(target, "Single", StringComparison.OrdinalIgnoreCase))
        {
            return BeastTargetRule.None;
        }

        if (description.Contains("mayor hp", StringComparison.Ordinal))
        {
            return BeastTargetRule.HighestCurrentHp;
        }

        if (description.Contains("mayor elem atk", StringComparison.Ordinal))
        {
            return BeastTargetRule.HighestElementalAttack;
        }

        if (description.Contains("menor phys def", StringComparison.Ordinal))
        {
            return BeastTargetRule.LowestPhysicalDefense;
        }

        if (description.Contains("mayor speed", StringComparison.Ordinal))
        {
            return BeastTargetRule.HighestSpeed;
        }

        if (description.Contains("menor elem def", StringComparison.Ordinal))
        {
            return BeastTargetRule.LowestElementalDefense;
        }

        if (description.Contains("mayor phys atk", StringComparison.Ordinal))
        {
            return BeastTargetRule.HighestPhysicalAttack;
        }

        if (description.Contains("mayor phys def", StringComparison.Ordinal))
        {
            return BeastTargetRule.HighestPhysicalDefense;
        }

        return BeastTargetRule.HighestCurrentHp;
    }

}

