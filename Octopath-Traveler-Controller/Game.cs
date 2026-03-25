using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

public sealed class Game
{
    private const double BasicAttackModifier = 1.3;

    private readonly MainConsoleView _view;
    private GameState _state = new();

    public Game(View view, string teamsFolder)
    {
        _view = new MainConsoleView(_state, view, teamsFolder);
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
    {
        return _state.CombatFlow ?? throw new InvalidOperationException("Combat flow is not initialized.");
    }

    private static bool IsBattleOngoing(CombatFlowState combatState)
    {
        return combatState.Result == BattleResult.Ongoing;
    }

    private void PrepareRound(CombatFlowState combatState, int roundNumber)
    {
        RechargeBpIfNeeded(combatState, roundNumber);
        StartRoundQueues(combatState);
        _view.ShowRoundStart(roundNumber);
        _view.ShowCombatStatus(combatState.BuildViewSnapshot());
    }

    private static void RechargeBpIfNeeded(CombatFlowState combatState, int roundNumber)
    {
        if (roundNumber > 1)
        {
            RechargeBpForAliveTravelers(combatState);
        }
    }

    private static void StartRoundQueues(CombatFlowState combatState)
    {
        var currentRoundQueue = TurnQueueFactory.BuildCurrentRoundQueue(combatState);
        var nextRoundQueue = TurnQueueFactory.BuildNextRoundQueue(combatState);
        combatState.StartRound(currentRoundQueue, nextRoundQueue);
    }

    private void ExecuteRoundTurns(CombatFlowState combatState)
    {
        while (HasPendingTurns(combatState) && IsBattleOngoing(combatState))
        {
            var currentTurn = GetCurrentTurn(combatState);
            ResolveTurn(combatState, currentTurn);

            if (!IsBattleOngoing(combatState))
            {
                break;
            }

            if (TryFinishBattle(combatState))
            {
                break;
            }

            ShowRoundStateAfterTurn(combatState);
        }
    }

    private static bool HasPendingTurns(CombatFlowState combatState)
    {
        return combatState.CurrentRound is not null && !combatState.CurrentRound.CurrentQueue.IsEmpty;
    }

    private static TurnEntry GetCurrentTurn(CombatFlowState combatState)
    {
        return combatState.PeekTurn() ?? throw new InvalidOperationException("Round queue is inconsistent.");
    }

    private void ResolveTurn(CombatFlowState combatState, TurnEntry currentTurn)
    {
        if (currentTurn.UnitReference.Kind == CombatantKind.Traveler)
        {
            ResolveTravelerTurn(combatState, currentTurn);
            return;
        }

        ResolveBeastTurn(combatState, currentTurn);
    }

    private bool TryFinishBattle(CombatFlowState combatState)
    {
        var battleOutcome = EvaluateBattleResult(combatState);
        if (battleOutcome == BattleResult.Ongoing)
        {
            return false;
        }

        combatState.FinishBattle(battleOutcome);
        ShowWinner(battleOutcome);
        return true;
    }

    private void ShowRoundStateAfterTurn(CombatFlowState combatState)
    {
        if (combatState.CurrentRound is not null && !combatState.CurrentRound.CurrentQueue.IsEmpty)
        {
            _view.ShowCombatStatus(combatState.BuildViewSnapshot(), includeLeadingSeparator: true);
        }
    }

    private void ResolveTravelerTurn(CombatFlowState combatState, TurnEntry travelerTurn)
    {
        var travelerTurnContext = new TravelerTurnContext(combatState, travelerTurn);

        while (IsBattleOngoing(travelerTurnContext.CombatState))
        {
            var selectedAction = AskTravelerAction(travelerTurnContext);
            if (TryHandleTravelerAction(travelerTurnContext, selectedAction))
            {
                return;
            }
        }
    }

    private int AskTravelerAction(TravelerTurnContext travelerTurnContext)
    {
        return _view.AskTravelerMainAction(travelerTurnContext.Traveler.Name);
    }

    private bool TryHandleTravelerAction(TravelerTurnContext travelerTurnContext, int selectedAction)
    {
        return selectedAction switch
        {
            1 => TryHandleTravelerBasicAttack(travelerTurnContext),
            2 => ShowTravelerSkills(travelerTurnContext),
            3 => CompleteTravelerTurn(travelerTurnContext),
            4 => TravelerFlees(travelerTurnContext),
            _ => false
        };
    }

    private bool TryHandleTravelerBasicAttack(TravelerTurnContext travelerTurnContext)
    {
        if (!TryResolveTravelerBasicAttack(travelerTurnContext))
        {
            return false;
        }

        travelerTurnContext.CombatState.CompleteTurn();
        return true;
    }

    private bool ShowTravelerSkills(TravelerTurnContext travelerTurnContext)
    {
        _ = _view.AskTravelerSkill(travelerTurnContext.Traveler.Name, travelerTurnContext.Traveler.ActiveSkills);
        return false;
    }

    private static bool CompleteTravelerTurn(TravelerTurnContext travelerTurnContext)
    {
        travelerTurnContext.CombatState.CompleteTurn();
        return true;
    }

    private bool TravelerFlees(TravelerTurnContext travelerTurnContext)
    {
        _view.ShowFleeMessage();
        travelerTurnContext.CombatState.FinishBattle(BattleResult.PlayerDefeat);
        _view.ShowEnemyWinMessage();
        return true;
    }

    private bool TryResolveTravelerBasicAttack(TravelerTurnContext travelerTurnContext)
    {
        var selectedWeapon = TrySelectWeapon(travelerTurnContext);
        if (selectedWeapon is null)
        {
            return false;
        }

        var selectedTarget = TrySelectTravelerTarget(travelerTurnContext);
        if (selectedTarget is null)
        {
            return false;
        }

        AskBoostPointsIfAvailable(travelerTurnContext);
        var basicAttackContext = new TravelerBasicAttackContext(travelerTurnContext, selectedWeapon, selectedTarget);
        ExecuteTravelerBasicAttack(basicAttackContext);
        return true;
    }

    private string? TrySelectWeapon(TravelerTurnContext travelerTurnContext)
    {
        var selectedWeapon = _view.AskWeaponSelection(travelerTurnContext.Traveler.Weapons);
        if (selectedWeapon == travelerTurnContext.Traveler.Weapons.Count + 1)
        {
            return null;
        }

        return travelerTurnContext.Traveler.Weapons[selectedWeapon - 1];
    }

    private UnitReference? TrySelectTravelerTarget(TravelerTurnContext travelerTurnContext)
    {
        var aliveBeasts = travelerTurnContext.CombatState.GetAliveBeasts();
        var enemySnapshots = aliveBeasts
            .Select(reference => BuildEnemySnapshot(travelerTurnContext.CombatState, reference))
            .ToList();

        var selectedTarget = _view.AskTravelerTarget(travelerTurnContext.Traveler.Name, enemySnapshots);
        if (selectedTarget == enemySnapshots.Count + 1)
        {
            return null;
        }

        return aliveBeasts[selectedTarget - 1];
    }

    private void AskBoostPointsIfAvailable(TravelerTurnContext travelerTurnContext)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        if (travelerState.CurrentBp > 0)
        {
            _ = _view.AskBoostPointsToUse();
        }
    }

    private void ExecuteTravelerBasicAttack(TravelerBasicAttackContext basicAttackContext)
    {
        var dealtDamage = CalculateTravelerAttackDamage(basicAttackContext);
        var targetCurrentHp = basicAttackContext.TravelerTurnContext.CombatState.ApplyDamage(basicAttackContext.TargetBeast, dealtDamage);
        _view.ShowTravelerAttackResult(
            basicAttackContext.TravelerTurnContext.Traveler.Name,
            basicAttackContext.TargetBeast.Unit.Name,
            basicAttackContext.WeaponType,
            dealtDamage,
            targetCurrentHp);
    }

    private static int CalculateTravelerAttackDamage(TravelerBasicAttackContext basicAttackContext)
    {
        return CalculatePhysicalDamage(
            basicAttackContext.TravelerTurnContext.Traveler.Stats.PhysicalAttack,
            basicAttackContext.TargetBeast.Unit.Stats.PhysicalDefense,
            BasicAttackModifier);
    }

    private void ResolveBeastTurn(CombatFlowState combatState, TurnEntry beastTurn)
    {
        var targetTraveler = SelectBeastTarget(combatState);
        var beastTurnContext = new BeastTurnContext(combatState, beastTurn, targetTraveler);
        ExecuteBeastAttack(beastTurnContext);
        beastTurnContext.CombatState.CompleteTurn();
    }

    private static int CalculateBeastAttackDamage(BeastTurnContext beastTurnContext)
    {
        return CalculatePhysicalDamage(
            beastTurnContext.Beast.Stats.PhysicalAttack,
            beastTurnContext.TargetTraveler.Unit.Stats.PhysicalDefense,
            BasicAttackModifier);
    }

    private void ExecuteBeastAttack(BeastTurnContext beastTurnContext)
    {
        var dealtDamage = CalculateBeastAttackDamage(beastTurnContext);
        var targetCurrentHp = beastTurnContext.CombatState.ApplyDamage(beastTurnContext.TargetTraveler, dealtDamage);
        _view.ShowBeastAttackResult(
            beastTurnContext.Beast.Name,
            beastTurnContext.TargetTraveler.Unit.Name,
            dealtDamage,
            targetCurrentHp);
    }

    private static UnitReference SelectBeastTarget(CombatFlowState combatState)
    {
        var travelers = combatState.GetAliveTravelers();
        return travelers
            .OrderByDescending(reference => combatState.GetUnitState(reference).CurrentHp)
            .ThenBy(reference => reference.BoardPosition)
            .First();
    }

    private static BattleResult EvaluateBattleResult(CombatFlowState combatState)
    {
        if (!combatState.GetAliveBeasts().Any())
        {
            return BattleResult.PlayerVictory;
        }

        if (!combatState.GetAliveTravelers().Any())
        {
            return BattleResult.PlayerDefeat;
        }

        return BattleResult.Ongoing;
    }

    private static int CalculatePhysicalDamage(int attackerPhysicalAttack, int targetPhysicalDefense, double modifier)
    {
        var rawDamage = attackerPhysicalAttack * modifier - targetPhysicalDefense;
        return Math.Max(0, Convert.ToInt32(Math.Floor(rawDamage)));
    }

    private static UnitDisplaySnapshot BuildEnemySnapshot(CombatFlowState combatState, UnitReference reference)
    {
        var state = combatState.GetUnitState(reference);
        var slot = ((char)('A' + reference.BoardPosition)).ToString();

        return new UnitDisplaySnapshot(
            slot,
            reference.Unit.Name,
            reference.Kind,
            state.CurrentHp,
            state.MaxHp,
            state.CurrentSp,
            state.MaxSp,
            state.CurrentBp,
            state.CurrentShields);
    }

    private static void RechargeBpForAliveTravelers(CombatFlowState combatState)
    {
        foreach (var travelerState in combatState.UnitStates.Where(s => s.UnitReference.Kind == CombatantKind.Traveler && s.IsAlive))
        {
            travelerState.CurrentBp = Math.Min(5, travelerState.CurrentBp + 1);
        }
    }

    private void ShowWinner(BattleResult result)
    {
        if (result == BattleResult.PlayerVictory)
        {
            _view.ShowPlayerWinMessage();
            return;
        }

        _view.ShowEnemyWinMessage();
    }
}