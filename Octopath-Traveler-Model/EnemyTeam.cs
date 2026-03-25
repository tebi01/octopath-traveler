namespace Octopath_Traveler_Model;

public sealed class EnemyTeam
{
    public const int MinBeasts = 1;
    public const int MaxBeasts = 5;

    public IReadOnlyList<Beast> Beasts { get; }

    public EnemyTeam(IEnumerable<Beast>? beasts)
    {
        var team = beasts?.ToList() ?? throw new ArgumentNullException(nameof(beasts));

        if (team.Count < MinBeasts || team.Count > MaxBeasts)
        {
            throw new ArgumentException(
                $"The enemy team must contain between {MinBeasts} and {MaxBeasts} beasts.",
                nameof(beasts));
        }

        ValidationRules.EnsureUniqueUnitNames(team.Select(b => b.Name), nameof(beasts));

        Beasts = team;
    }
}

