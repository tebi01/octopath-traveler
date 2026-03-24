namespace Octopath_Traveler_Model;

public sealed class BattleBoard
{
    public const int PlayerSlotsCount = 4;
    public const int EnemySlotsCount = 5;

    public IReadOnlyList<Traveler?> PlayerSlots { get; }
    public IReadOnlyList<Beast?> EnemySlots { get; }

    public BattleBoard(PlayerTeam playerTeam, EnemyTeam enemyTeam)
    {
        if (playerTeam is null)
        {
            throw new ArgumentNullException(nameof(playerTeam));
        }

        if (enemyTeam is null)
        {
            throw new ArgumentNullException(nameof(enemyTeam));
        }

        PlayerSlots = BuildSlots(playerTeam.Travelers, PlayerSlotsCount, nameof(playerTeam));
        EnemySlots = BuildSlots(enemyTeam.Beasts, EnemySlotsCount, nameof(enemyTeam));
    }

    private static IReadOnlyList<TUnit?> BuildSlots<TUnit>(
        IReadOnlyList<TUnit>? units,
        int slotsCount,
        string paramName)
        where TUnit : Unit
    {
        if (units is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (units.Count > slotsCount)
        {
            throw new ArgumentException($"Team cannot exceed {slotsCount} slots.", paramName);
        }

        var slots = new TUnit?[slotsCount];

        for (var index = 0; index < units.Count; index++)
        {
            slots[index] = units[index];
        }

        return slots;
    }
}


