namespace Octopath_Traveler_Model.CombatFlow;

public sealed class TravelerTurnDecision
{
    public bool IsCancelled { get; }
    public TravelerDeclaredAction? Action { get; }

    private TravelerTurnDecision(bool isCancelled, TravelerDeclaredAction? action)
    {
        IsCancelled = isCancelled;
        Action = action;
    }

    public static TravelerTurnDecision Cancel()
    {
        return new TravelerTurnDecision(true, null);
    }

    public static TravelerTurnDecision Commit(TravelerDeclaredAction action)
    {
        return new TravelerTurnDecision(false, action ?? throw new ArgumentNullException(nameof(action)));
    }
}

public interface ITravelerActionSelector
{
    TravelerTurnDecision SelectAction(CombatFlowState flowState, TurnEntry turnEntry);
}

public interface IBeastActionPolicy
{
    BeastDeclaredAction SelectAction(CombatFlowState flowState, TurnEntry turnEntry);
}

// This is a model-side abstraction. Concrete View adapters can implement this later.
public interface ICombatEventPort
{
    void OnRoundStarted(RoundState roundState);
    void OnTurnStarted(TurnEntry turnEntry);
    void OnTurnCancelled(TurnEntry turnEntry);
    void OnActionDeclared(DeclaredAction action);
    void OnTurnResolved(TurnEntry turnEntry, DeclaredAction action);
    void OnRoundEnded(RoundState roundState);
    void OnBattleFinished(BattleResult result);
}

