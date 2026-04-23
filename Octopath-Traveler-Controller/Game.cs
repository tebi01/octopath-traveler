using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

public sealed class Game
{
    private readonly MainConsoleView _view;
    private readonly TravelerEnemyTargetingService _enemyTargetingService;
    private readonly TravelerTurnResolver _travelerTurnResolver;
    private readonly BeastTurnResolver _beastTurnResolver;
    private readonly BattleCompletionResolver _battleCompletionResolver;
    private readonly CombatRoundStateCoordinator _roundStateCoordinator;
    private GameState _state = new();

    public Game(View view, string teamsFolder)
    {
        _view = new MainConsoleView(view, teamsFolder);
        var turnPriorityCoordinator = new TurnPriorityCoordinator();
        _roundStateCoordinator = new CombatRoundStateCoordinator(turnPriorityCoordinator);
        var combatMath = new TravelerCombatMathService();
        var allyTargetingService = new TravelerAllyTargetingService(_view);
        _enemyTargetingService = new TravelerEnemyTargetingService(_view);
        var supportSkillResolver = CreateSupportSkillResolver(allyTargetingService);
        var tacticalSkillResolver = CreateTacticalSkillResolver(turnPriorityCoordinator, combatMath);
        var basicAttackResolver = CreateBasicAttackResolver(combatMath);
        var travelerTurnOutcomeResolver = CreateTravelerTurnOutcomeResolver();
        var offensiveSkillResolver = CreateOffensiveSkillResolver(combatMath);
        _beastTurnResolver = CreateBeastTurnResolver(combatMath);
        var travelerSkillDispatchResolver = CreateTravelerSkillDispatchResolver(
            tacticalSkillResolver,
            offensiveSkillResolver,
            supportSkillResolver);
        _travelerTurnResolver = CreateTravelerTurnResolver(
            travelerSkillDispatchResolver,
            basicAttackResolver,
            travelerTurnOutcomeResolver);
        _battleCompletionResolver = new BattleCompletionResolver(_view);
    }

    private TravelerSupportSkillResolver CreateSupportSkillResolver(TravelerAllyTargetingService allyTargetingService)
    {
        return new TravelerSupportSkillResolver(
            _view,
            allyTargetingService,
            TryConsumeTravelerSpWithBoostPrompt,
            CompleteTravelerTurn);
    }

    private TravelerTacticalSkillResolver CreateTacticalSkillResolver(
        TurnPriorityCoordinator turnPriorityCoordinator,
        TravelerCombatMathService combatMath)
    {
        return new TravelerTacticalSkillResolver(
            _view,
            turnPriorityCoordinator,
            combatMath,
            TrySelectTravelerTarget,
            TryConsumeTravelerSpWithBoostPrompt,
            CompleteTravelerTurn);
    }

    private TravelerBasicAttackResolver CreateBasicAttackResolver(TravelerCombatMathService combatMath)
    {
        return new TravelerBasicAttackResolver(
            _view,
            combatMath,
            TrySelectTravelerTarget,
            AskBoostPointsIfAvailable);
    }

    private TravelerTurnOutcomeResolver CreateTravelerTurnOutcomeResolver()
    {
        return new TravelerTurnOutcomeResolver(
            _view,
            CompleteTravelerTurn);
    }

    private TravelerOffensiveSkillResolver CreateOffensiveSkillResolver(TravelerCombatMathService combatMath)
    {
        return new TravelerOffensiveSkillResolver(
            _view,
            TrySelectTravelerTarget,
            TryConsumeTravelerSpWithBoostPrompt,
            CompleteTravelerTurn,
            combatMath.HasWeaknessAgainstAttackType,
            combatMath.GetTravelerDamageMultiplier,
            combatMath.ApplyMultiplier,
            combatMath.CalculatePhysicalDamageRaw,
            combatMath.CalculateElementalDamageRaw,
            combatMath.CalculateLastStandRawDamage);
    }

    private BeastTurnResolver CreateBeastTurnResolver(TravelerCombatMathService combatMath)
    {
        var beastSkillCatalog = BeastSkillCatalog.LoadDefault();
        return new BeastTurnResolver(
            _view,
            beastSkillCatalog,
            combatMath.CalculatePhysicalDamageRaw,
            combatMath.CalculateElementalDamageRaw);
    }

    private TravelerSkillDispatchResolver CreateTravelerSkillDispatchResolver(
        TravelerTacticalSkillResolver tacticalSkillResolver,
        TravelerOffensiveSkillResolver offensiveSkillResolver,
        TravelerSupportSkillResolver supportSkillResolver)
    {
        return new TravelerSkillDispatchResolver(
            _view,
            tacticalSkillResolver,
            offensiveSkillResolver,
            supportSkillResolver);
    }

    private TravelerTurnResolver CreateTravelerTurnResolver(
        TravelerSkillDispatchResolver travelerSkillDispatchResolver,
        TravelerBasicAttackResolver basicAttackResolver,
        TravelerTurnOutcomeResolver travelerTurnOutcomeResolver)
    {
        return new TravelerTurnResolver(
            _view,
            travelerSkillDispatchResolver,
            basicAttackResolver,
            travelerTurnOutcomeResolver);
    }

    public void Play()
    {
        try
        {
            var teamInfo = _view.SelectTeamInfo();
            var teamBuilder = new TeamsBuilder(teamInfo);
            _state = teamBuilder.Build();
            RunCombat();
        }
        catch (Exception)
        {
            _view.ShowInvalidTeamMessage();
        }
    }

    private void RunCombat()
    {
        var combatState = GetCombatState();
        var roundNumber = 1;

        while (IsBattleOngoing(combatState))
        {
            PrepareRound(combatState, roundNumber);
            ExecuteRoundTurns(combatState);
            roundNumber++;
        }
    }

    private CombatFlowState GetCombatState()
        => _state.CombatFlow ?? throw new InvalidOperationException("Combat flow is not initialized.");

    private static bool IsBattleOngoing(CombatFlowState combatState)
        => combatState.Result == BattleResult.Ongoing;

    private void PrepareRound(CombatFlowState combatState, int roundNumber)
    {
        _roundStateCoordinator.ApplyRoundStart(combatState, roundNumber);
        _view.ShowRoundStart(roundNumber);
        _view.ShowCombatStatus(combatState.BuildViewSnapshot());
    }

    private void ExecuteRoundTurns(CombatFlowState combatState)
    {
        while (HasPendingTurns(combatState) && IsBattleOngoing(combatState))
        {
            var currentTurn = GetCurrentTurn(combatState);
            ResolveTurn(combatState, currentTurn);
            _roundStateCoordinator.RefreshProjectedNextRoundQueue(combatState);

            if (!IsBattleOngoing(combatState))
            {
                break;
            }

            if (_battleCompletionResolver.TryFinishBattle(combatState))
            {
                break;
            }

            ShowRoundStateAfterTurn(combatState);
        }
    }

    private static bool HasPendingTurns(CombatFlowState combatState)
        => combatState.CurrentRound is not null && !combatState.CurrentRound.CurrentQueue.IsEmpty;

    private static TurnEntry GetCurrentTurn(CombatFlowState combatState)
        => combatState.PeekTurn() ?? throw new InvalidOperationException("Round queue is inconsistent.");

    private void ResolveTurn(CombatFlowState combatState, TurnEntry currentTurn)
    {
        if (currentTurn.UnitReference.Kind == CombatantKind.Traveler)
        {
            _travelerTurnResolver.ResolveTravelerTurn(combatState, currentTurn);
            return;
        }

        ResolveBeastTurn(combatState, currentTurn);
    }

    private void ShowRoundStateAfterTurn(CombatFlowState combatState)
    {
        if (combatState.CurrentRound is not null && !combatState.CurrentRound.CurrentQueue.IsEmpty)
        {
            _view.ShowCombatStatusWithLeadingSeparator(combatState.BuildViewSnapshot());
        }
    }


    private static bool TryConsumeTravelerSp(TravelerTurnContext travelerTurnContext, int cost)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        if (travelerState.CurrentSp < cost)
        {
            return false;
        }

        travelerState.CurrentSp -= cost;
        return true;
    }

    private bool TryConsumeTravelerSpWithBoostPrompt(TravelerTurnContext travelerTurnContext, int cost)
    {
        AskBoostPointsIfAvailable(travelerTurnContext);
        return TryConsumeTravelerSp(travelerTurnContext, cost);
    }

    private static bool CompleteTravelerTurn(TravelerTurnContext travelerTurnContext)
    {
        travelerTurnContext.CombatState.CompleteTurn();
        return true;
    }

    private bool TrySelectTravelerTarget(TravelerTurnContext travelerTurnContext, out UnitReference selectedTarget)
        => _enemyTargetingService.TrySelectTarget(travelerTurnContext, out selectedTarget);

    private void AskBoostPointsIfAvailable(TravelerTurnContext travelerTurnContext)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        if (travelerState.CurrentBp > 0)
        {
            _ = _view.AskBoostPointsToUse();
        }
    }

    private void ResolveBeastTurn(CombatFlowState combatState, TurnEntry beastTurn)
    {
        _beastTurnResolver.ResolveTurn(combatState, beastTurn);
    }


}

