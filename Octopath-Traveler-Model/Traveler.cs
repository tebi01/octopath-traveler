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
        IEnumerable<string> weapons)
        : this(name, stats, skillPoints, weapons, Array.Empty<string>(), Array.Empty<string>())
    {
    }

    public Traveler(
        string name,
        CombatStats stats,
        SkillPoints skillPoints,
        IEnumerable<string> weapons,
        IEnumerable<string> activeSkills)
        : this(name, stats, skillPoints, weapons, activeSkills, Array.Empty<string>())
    {
    }

    public Traveler(
        string name,
        CombatStats stats,
        SkillPoints skillPoints,
        IEnumerable<string> weapons,
        IEnumerable<string> activeSkills,
        IEnumerable<string> passiveSkills)
        : base(name, stats)
    {
        SkillPoints = skillPoints ?? throw new ArgumentNullException(nameof(skillPoints));
        ArgumentNullException.ThrowIfNull(weapons);
        ArgumentNullException.ThrowIfNull(activeSkills);
        ArgumentNullException.ThrowIfNull(passiveSkills);

        var normalizedWeapons = ValidationRules.NormalizeNonEmptyStrings(weapons, nameof(weapons));
        ValidationRules.EnsureUniqueStrings(normalizedWeapons, nameof(weapons));

        var normalizedActiveSkills = ValidationRules.NormalizeNonEmptyStrings(activeSkills, nameof(activeSkills));
        var normalizedPassiveSkills = ValidationRules.NormalizeNonEmptyStrings(passiveSkills, nameof(passiveSkills));

        if (normalizedActiveSkills.Count > MaxActiveSkills)
        {
            throw new ArgumentException($"A traveler can have at most {MaxActiveSkills} active skills.", nameof(activeSkills));
        }

        if (normalizedPassiveSkills.Count > MaxPassiveSkills)
        {
            throw new ArgumentException($"A traveler can have at most {MaxPassiveSkills} passive skills.", nameof(passiveSkills));
        }

        ValidationRules.EnsureUniqueStrings(normalizedActiveSkills, nameof(activeSkills));
        ValidationRules.EnsureUniqueStrings(normalizedPassiveSkills, nameof(passiveSkills));

        Weapons = normalizedWeapons;
        ActiveSkills = normalizedActiveSkills;
        PassiveSkills = normalizedPassiveSkills;
    }
}


