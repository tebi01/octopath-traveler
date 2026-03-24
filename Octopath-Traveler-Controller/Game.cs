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
            var info = _view.SelectTeamInfo();
            var builder = new TeamsBuilder(info);
            _state = builder.Build();
            RunCombat();
        }
        catch (Exception)
        {
            _view.ShowInvalidTeamMessage();
        }
    }

    private void RunCombat()
    {
        var flow = _state.CombatFlow ?? throw new InvalidOperationException("Combat flow is not initialized.");
        var roundNumber = 0;

        while (flow.Result == BattleResult.Ongoing)
        {
            roundNumber++;
            if (roundNumber > 1)
            {
                IncreaseBpForAliveTravelers(flow);
            }

            var currentQueue = TurnQueueFactory.BuildCurrentRoundQueue(flow);
            var nextQueue = TurnQueueFactory.BuildNextRoundQueue(flow);
            flow.StartRound(currentQueue, nextQueue);

            _view.ShowRoundStart(roundNumber);
            _view.ShowCombatStatus(flow.BuildViewSnapshot());

            while (flow.CurrentRound is not null && !flow.CurrentRound.CurrentQueue.IsEmpty && flow.Result == BattleResult.Ongoing)
            {
                var turn = flow.PeekTurn() ?? throw new InvalidOperationException("Round queue is inconsistent.");

                if (turn.UnitReference.Kind == CombatantKind.Traveler)
                {
                    ResolveTravelerTurn(flow, turn);
                }
                else
                {
                    ResolveBeastTurn(flow, turn);
                }

                if (flow.Result != BattleResult.Ongoing)
                {
                    break;
                }

                var result = EvaluateBattleResult(flow);
                if (result != BattleResult.Ongoing)
                {
                    flow.FinishBattle(result);
                    ShowWinner(result);
                    break;
                }

                if (!flow.CurrentRound.CurrentQueue.IsEmpty)
                {
                    _view.ShowCombatStatus(flow.BuildViewSnapshot(), includeLeadingSeparator: true);
                }
            }
        }
    }

    private void ResolveTravelerTurn(CombatFlowState flow, TurnEntry turn)
    {
        var traveler = (Traveler)turn.UnitReference.Unit;

        while (flow.Result == BattleResult.Ongoing)
        {
            var action = _view.AskTravelerMainAction(traveler.Name);
            switch (action)
            {
                case 1:
                    if (TryResolveTravelerBasicAttack(flow, turn, traveler))
                    {
                        flow.CompleteTurn();
                        return;
                    }

                    break;
                case 2:
                    _ = _view.AskTravelerSkill(traveler.Name, traveler.ActiveSkills);
                    break;
                case 3:
                    flow.CompleteTurn();
                    return;
                case 4:
                    _view.ShowFleeMessage();
                    flow.FinishBattle(BattleResult.PlayerDefeat);
                    _view.ShowEnemyWinMessage();
                    return;
            }
        }
    }

    private bool TryResolveTravelerBasicAttack(CombatFlowState flow, TurnEntry turn, Traveler traveler)
    {
        var selectedWeapon = _view.AskWeaponSelection(traveler.Weapons);
        if (selectedWeapon == traveler.Weapons.Count + 1)
        {
            return false;
        }

        var weapon = traveler.Weapons[selectedWeapon - 1];

        var aliveBeasts = flow.GetAliveBeasts();
        var enemySnapshots = aliveBeasts
            .Select(reference => BuildEnemySnapshot(flow, reference))
            .ToList();

        var selectedTarget = _view.AskTravelerTarget(traveler.Name, enemySnapshots);
        if (selectedTarget == enemySnapshots.Count + 1)
        {
            return false;
        }

        var targetReference = aliveBeasts[selectedTarget - 1];

        var actorState = flow.GetUnitState(turn.UnitReference);
        if (actorState.CurrentBp > 0)
        {
            _ = _view.AskBoostPointsToUse();
        }

        var damage = CalculatePhysicalDamage(traveler.Stats.PhysicalAttack, targetReference.Unit.Stats.PhysicalDefense, BasicAttackModifier);
        var targetCurrentHp = flow.ApplyDamage(targetReference, damage);

        _view.ShowTravelerAttackResult(traveler.Name, targetReference.Unit.Name, weapon, damage, targetCurrentHp);
        return true;
    }

    private void ResolveBeastTurn(CombatFlowState flow, TurnEntry turn)
    {
        var target = SelectBeastTarget(flow);
        var beast = (Beast)turn.UnitReference.Unit;

        var damage = CalculatePhysicalDamage(beast.Stats.PhysicalAttack, target.Unit.Stats.PhysicalDefense, BasicAttackModifier);
        var targetCurrentHp = flow.ApplyDamage(target, damage);

        flow.CompleteTurn();
        _view.ShowBeastAttackResult(beast.Name, target.Unit.Name, damage, targetCurrentHp);
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

    private static void IncreaseBpForAliveTravelers(CombatFlowState flow)
    {
        foreach (var state in flow.UnitStates.Where(s => s.UnitReference.Kind == CombatantKind.Traveler && s.IsAlive))
        {
            state.CurrentBp = Math.Min(5, state.CurrentBp + 1);
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