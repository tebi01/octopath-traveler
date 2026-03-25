namespace Octopath_Traveler_Model;

public sealed class GameState
{
    public PlayerTeam? PlayerTeam { get; init; }
    public EnemyTeam? EnemyTeam { get; init; }
    public BattleBoard? Board { get; init; }
    public CombatFlow.CombatFlowState? CombatFlow { get; private set; }

    public GameState()
    {
    }

    public GameState(PlayerTeam playerTeam, EnemyTeam enemyTeam)
    {
        PlayerTeam = playerTeam ?? throw new ArgumentNullException(nameof(playerTeam));
        EnemyTeam = enemyTeam ?? throw new ArgumentNullException(nameof(enemyTeam));
        Board = new BattleBoard(PlayerTeam, EnemyTeam);
        CombatFlow = new CombatFlow.CombatFlowState(PlayerTeam, EnemyTeam, Board);
    }
}