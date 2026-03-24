namespace Octopath_Traveler_Model;

public sealed class Beast : Unit
{
    public string Skill { get; }
    public int MaxShields { get; }
    public int CurrentShields { get; }
    public IReadOnlyList<string> Weaknesses { get; }

    public bool IsInBreakingPoint => CurrentShields == 0;

    public Beast(
        string name,
        CombatStats stats,
        string skill,
        int maxShields,
        int currentShields,
        IEnumerable<string>? weaknesses)
        : base(name, stats)
    {
        if (string.IsNullOrWhiteSpace(skill))
        {
            throw new ArgumentException("Skill cannot be null or empty.", nameof(skill));
        }

        if (maxShields < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxShields), "Max shields cannot be negative.");
        }

        if (currentShields < 0 || currentShields > maxShields)
        {
            throw new ArgumentOutOfRangeException(nameof(currentShields), "Current shields must be between 0 and Max shields.");
        }

        var normalizedWeaknesses = NormalizeToReadOnlyList(weaknesses, nameof(weaknesses));
        EnsureUnique(normalizedWeaknesses, nameof(weaknesses));

        Skill = skill.Trim();
        MaxShields = maxShields;
        CurrentShields = currentShields;
        Weaknesses = normalizedWeaknesses;
    }

    public Beast(
        string name,
        CombatStats stats,
        string skill,
        int shields,
        IEnumerable<string>? weaknesses)
        : this(name, stats, skill, shields, shields, weaknesses)
    {
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


