namespace Octopath_Traveler_Model;

public sealed class Traveler : Unit
{
    public const int MaxActiveSkills = 8;
    public const int MaxPassiveSkills = 4;

    public SkillPoints SkillPoints { get; }
    public IReadOnlyList<string> Weapons { get; }
    public IReadOnlyList<string> ActiveSkills { get; }
    public IReadOnlyList<string> PassiveSkills { get; }

    public Traveler(
        string name,
        CombatStats stats,
        SkillPoints skillPoints,
        IEnumerable<string>? weapons,
        IEnumerable<string>? activeSkills = null,
        IEnumerable<string>? passiveSkills = null)
        : base(name, stats)
    {
        SkillPoints = skillPoints ?? throw new ArgumentNullException(nameof(skillPoints));

        var normalizedWeapons = NormalizeToReadOnlyList(weapons, nameof(weapons));
        EnsureUnique(normalizedWeapons, nameof(weapons));

        var normalizedActiveSkills = NormalizeToReadOnlyList(activeSkills, nameof(activeSkills));
        var normalizedPassiveSkills = NormalizeToReadOnlyList(passiveSkills, nameof(passiveSkills));

        if (normalizedActiveSkills.Count > MaxActiveSkills)
        {
            throw new ArgumentException($"A traveler can have at most {MaxActiveSkills} active skills.", nameof(activeSkills));
        }

        if (normalizedPassiveSkills.Count > MaxPassiveSkills)
        {
            throw new ArgumentException($"A traveler can have at most {MaxPassiveSkills} passive skills.", nameof(passiveSkills));
        }

        EnsureUnique(normalizedActiveSkills, nameof(activeSkills));
        EnsureUnique(normalizedPassiveSkills, nameof(passiveSkills));

        Weapons = normalizedWeapons;
        ActiveSkills = normalizedActiveSkills;
        PassiveSkills = normalizedPassiveSkills;
    }

    private static IReadOnlyList<string> NormalizeToReadOnlyList(IEnumerable<string>? values, string paramName)
    {
        if (values is null)
        {
            return Array.Empty<string>();
        }

        var list = new List<string>();
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Values cannot contain null or empty entries.", paramName);
            }

            list.Add(value.Trim());
        }

        return list;
    }

    private static void EnsureUnique(IReadOnlyList<string> values, string paramName)
    {
        var unique = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
        if (unique.Count != values.Count)
        {
            throw new ArgumentException("Values cannot contain duplicates.", paramName);
        }
    }
}


