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

        PlayerSlots = BuildPlayerSlots(playerTeam);
        EnemySlots = BuildEnemySlots(enemyTeam);
    }

    private static IReadOnlyList<Traveler?> BuildPlayerSlots(PlayerTeam playerTeam)
    {
        if (playerTeam.Travelers.Count > PlayerSlotsCount)
        {
            throw new ArgumentException($"Team cannot exceed {PlayerSlotsCount} slots.", nameof(playerTeam));
        }

        var playerSlots = new Traveler?[PlayerSlotsCount];
        for (var index = 0; index < playerTeam.Travelers.Count; index++)
        {
            playerSlots[index] = playerTeam.Travelers[index];
        }

        return playerSlots;
    }

    private static IReadOnlyList<Beast?> BuildEnemySlots(EnemyTeam enemyTeam)
    {
        if (enemyTeam.Beasts.Count > EnemySlotsCount)
        {
            throw new ArgumentException($"Team cannot exceed {EnemySlotsCount} slots.", nameof(enemyTeam));
        }

        var enemySlots = new Beast?[EnemySlotsCount];
        for (var index = 0; index < enemyTeam.Beasts.Count; index++)
        {
            enemySlots[index] = enemyTeam.Beasts[index];
        }

        return enemySlots;
    }
}


