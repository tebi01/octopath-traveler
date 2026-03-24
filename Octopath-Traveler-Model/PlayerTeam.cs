namespace Octopath_Traveler_Model;

public sealed class PlayerTeam
{
    public const int MinTravelers = 1;
    public const int MaxTravelers = 4;

    public IReadOnlyList<Traveler> Travelers { get; }

    public PlayerTeam(IEnumerable<Traveler>? travelers)
    {
        var team = travelers?.ToList() ?? throw new ArgumentNullException(nameof(travelers));

        if (team.Count < MinTravelers || team.Count > MaxTravelers)
        {
            throw new ArgumentException(
                $"The player team must contain between {MinTravelers} and {MaxTravelers} travelers.",
                nameof(travelers));
        }

        EnsureNoDuplicatedUnits(team.Select(t => t.Name), nameof(travelers));

        Travelers = team;
    }

    private static void EnsureNoDuplicatedUnits(IEnumerable<string> names, string paramName)
    {
        var normalizedNames = names.ToList();
        var unique = new HashSet<string>(normalizedNames, StringComparer.OrdinalIgnoreCase);

        if (unique.Count != normalizedNames.Count)
        {
            throw new ArgumentException("Team cannot contain duplicated units.", paramName);
        }
    }
}

