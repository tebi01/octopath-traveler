namespace Octopath_Traveler_Model.CombatFlow;

public sealed class CombatFlowEngine
{
    private readonly CombatFlowState _flowState;
    private readonly ITravelerActionSelector _travelerActionSelector;
    private readonly IBeastActionPolicy _beastActionPolicy;
    private readonly ICombatEventPort? _eventPort;

    public CombatFlowEngine(
        CombatFlowState flowState,
        ITravelerActionSelector travelerActionSelector,
        IBeastActionPolicy beastActionPolicy,
        ICombatEventPort? eventPort = null)
    {
        _flowState = flowState ?? throw new ArgumentNullException(nameof(flowState));
        _travelerActionSelector = travelerActionSelector ?? throw new ArgumentNullException(nameof(travelerActionSelector));
        _beastActionPolicy = beastActionPolicy ?? throw new ArgumentNullException(nameof(beastActionPolicy));
        _eventPort = eventPort;
    }

    public void StartRound(TurnQueue currentQueue, TurnQueue nextQueue)
    {
        _flowState.StartRound(currentQueue, nextQueue);
        _eventPort?.OnRoundStarted(_flowState.CurrentRound!);
    }

    public void StartRoundFromSpeedOrder()
    {
        var currentQueue = TurnQueueFactory.BuildCurrentRoundQueue(_flowState);
        var nextQueue = TurnQueueFactory.BuildNextRoundQueue(_flowState);
        StartRound(currentQueue, nextQueue);
    }

    public TurnResolution TryResolveNextTurn()
    {
        var currentTurn = _flowState.PeekTurn();
        if (currentTurn is null)
        {
            return TurnResolution.NoTurnAvailable;
        }

        _flowState.BeginTurn();
        _eventPort?.OnTurnStarted(currentTurn);

        DeclaredAction? action;
        if (currentTurn.UnitReference.Kind == CombatantKind.Traveler)
        {
            var decision = _travelerActionSelector.SelectAction(_flowState, currentTurn);
            if (decision.IsCancelled)
            {
                _flowState.CancelTurn();
                _eventPort?.OnTurnCancelled(currentTurn);
                return TurnResolution.ActionCancelled;
            }

            action = decision.Action;
        }
        else
        {
            action = _beastActionPolicy.SelectAction(_flowState, currentTurn);
        }

        if (action is null)
        {
            throw new InvalidOperationException("A committed turn must provide an action.");
        }

        _flowState.DeclareAction(action);
        _eventPort?.OnActionDeclared(action);

        _flowState.CompleteTurn();
        _eventPort?.OnTurnResolved(currentTurn, action);
        return TurnResolution.TurnConsumed;
    }

    public void EndRound()
    {
        _flowState.EndRound();

        if (_flowState.CurrentRound is not null)
        {
            _eventPort?.OnRoundEnded(_flowState.CurrentRound);
        }
    }

    public void FinishBattle(BattleResult result)
    {
        _flowState.FinishBattle(result);
        _eventPort?.OnBattleFinished(result);
    }

    public void ProcessFlee()
    {
        FinishBattle(BattleResult.PlayerDefeat);
    }

    public BattleResult EvaluateBattleResult()
    {
        if (!_flowState.GetAliveBeasts().Any())
        {
            return BattleResult.PlayerVictory;
        }

        if (!_flowState.GetAliveTravelers().Any())
        {
            return BattleResult.PlayerDefeat;
        }

        return BattleResult.Ongoing;
    }
}

