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
        var traveler = GetTraveler(travelerTurn);

        while (IsBattleOngoing(combatState))
        {
            var selectedAction = AskTravelerAction(traveler);
            if (TryHandleTravelerAction(combatState, travelerTurn, traveler, selectedAction))
            {
                return;
            }
        }
    }

    private static Traveler GetTraveler(TurnEntry travelerTurn)
    {
        return (Traveler)travelerTurn.UnitReference.Unit;
    }

    private int AskTravelerAction(Traveler traveler)
    {
        return _view.AskTravelerMainAction(traveler.Name);
    }

    private bool TryHandleTravelerAction(CombatFlowState combatState, TurnEntry travelerTurn, Traveler traveler, int selectedAction)
    {
        return selectedAction switch
        {
            1 => TryHandleTravelerBasicAttack(combatState, travelerTurn, traveler),
            2 => ShowTravelerSkills(traveler),
            3 => CompleteTravelerTurn(combatState),
            4 => TravelerFlees(combatState),
            _ => false
        };
    }

    private bool TryHandleTravelerBasicAttack(CombatFlowState combatState, TurnEntry travelerTurn, Traveler traveler)
    {
        if (!TryResolveTravelerBasicAttack(combatState, travelerTurn, traveler))
        {
            return false;
        }

        combatState.CompleteTurn();
        return true;
    }

    private bool ShowTravelerSkills(Traveler traveler)
    {
        _ = _view.AskTravelerSkill(traveler.Name, traveler.ActiveSkills);
        return false;
    }

    private static bool CompleteTravelerTurn(CombatFlowState combatState)
    {
        combatState.CompleteTurn();
        return true;
    }

    private bool TravelerFlees(CombatFlowState combatState)
    {
        _view.ShowFleeMessage();
        combatState.FinishBattle(BattleResult.PlayerDefeat);
        _view.ShowEnemyWinMessage();
        return true;
    }

    private bool TryResolveTravelerBasicAttack(CombatFlowState combatState, TurnEntry travelerTurn, Traveler traveler)
    {
        var selectedWeapon = _view.AskWeaponSelection(traveler.Weapons);
        if (selectedWeapon == traveler.Weapons.Count + 1)
        {
            return false;
        }

        var weapon = traveler.Weapons[selectedWeapon - 1];

        var aliveBeasts = combatState.GetAliveBeasts();
        var enemySnapshots = aliveBeasts
            .Select(reference => BuildEnemySnapshot(combatState, reference))
            .ToList();

        var selectedTarget = _view.AskTravelerTarget(traveler.Name, enemySnapshots);
        if (selectedTarget == enemySnapshots.Count + 1)
        {
            return false;
        }

        var targetReference = aliveBeasts[selectedTarget - 1];

        var actorState = combatState.GetUnitState(travelerTurn.UnitReference);
        if (actorState.CurrentBp > 0)
        {
            _ = _view.AskBoostPointsToUse();
        }

        var damage = CalculatePhysicalDamage(traveler.Stats.PhysicalAttack, targetReference.Unit.Stats.PhysicalDefense, BasicAttackModifier);
        var targetCurrentHp = combatState.ApplyDamage(targetReference, damage);

        _view.ShowTravelerAttackResult(traveler.Name, targetReference.Unit.Name, weapon, damage, targetCurrentHp);
        return true;
    }

    private void ResolveBeastTurn(CombatFlowState combatState, TurnEntry beastTurn)
    {
        var beast = GetBeast(beastTurn);
        var targetTraveler = SelectBeastTarget(combatState);
        var dealtDamage = CalculateBeastAttackDamage(beast, targetTraveler);
        var targetCurrentHp = combatState.ApplyDamage(targetTraveler, dealtDamage);

        combatState.CompleteTurn();
        _view.ShowBeastAttackResult(beast.Name, targetTraveler.Unit.Name, dealtDamage, targetCurrentHp);
    }

    private static Beast GetBeast(TurnEntry beastTurn)
    {
        return (Beast)beastTurn.UnitReference.Unit;
    }

    private static int CalculateBeastAttackDamage(Beast beast, UnitReference targetTraveler)
    {
        return CalculatePhysicalDamage(beast.Stats.PhysicalAttack, targetTraveler.Unit.Stats.PhysicalDefense, BasicAttackModifier);
    }

    private static UnitReference SelectBeastTarget(CombatFlowState flow)
    {
        var travelers = flow.GetAliveTravelers();
        return travelers
            .OrderByDescending(reference => flow.GetUnitState(reference).CurrentHp)
            .ThenBy(reference => reference.BoardPosition)
            .First();
    }

    private static BattleResult EvaluateBattleResult(CombatFlowState flow)
    {
        if (!flow.GetAliveBeasts().Any())
        {
            return BattleResult.PlayerVictory;
        }

        if (!flow.GetAliveTravelers().Any())
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

    private static UnitDisplaySnapshot BuildEnemySnapshot(CombatFlowState flow, UnitReference reference)
    {
        var state = flow.GetUnitState(reference);
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