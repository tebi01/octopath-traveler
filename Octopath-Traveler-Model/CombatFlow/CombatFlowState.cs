namespace Octopath_Traveler_Model.CombatFlow;

public sealed class CombatFlowState
{
    private readonly Dictionary<Unit, CombatUnitState> _unitStates;

    public PlayerTeam PlayerTeam { get; }
    public EnemyTeam EnemyTeam { get; }
    public BattleBoard Board { get; }

    public CombatPhase Phase { get; private set; }
    public BattleResult Result { get; private set; }
    public RoundState? CurrentRound { get; private set; }

    public IReadOnlyCollection<CombatUnitState> UnitStates => _unitStates.Values;

    public CombatFlowState(PlayerTeam playerTeam, EnemyTeam enemyTeam, BattleBoard board)
    {
        PlayerTeam = playerTeam ?? throw new ArgumentNullException(nameof(playerTeam));
        EnemyTeam = enemyTeam ?? throw new ArgumentNullException(nameof(enemyTeam));
        Board = board ?? throw new ArgumentNullException(nameof(board));

        _unitStates = BuildUnitStates(board);
        Phase = CombatPhase.NotStarted;
        Result = BattleResult.Ongoing;
    }

    public void StartRound(TurnQueue currentQueue, TurnQueue nextQueue)
    {
        if (Result != BattleResult.Ongoing)
        {
            throw new InvalidOperationException("Cannot start a round when the battle has already finished.");
        }

        var roundNumber = CurrentRound is null ? 1 : CurrentRound.Number + 1;
        CurrentRound = new RoundState(roundNumber, currentQueue, nextQueue);
        Phase = CombatPhase.RoundSetup;

        ResetRoundFlagsForLivingUnits();
    }

    public TurnEntry? PeekTurn()
    {
        return CurrentRound?.PeekCurrentTurn();
    }

    public TurnEntry? CompleteTurn()
    {
        if (CurrentRound is null)
        {
            throw new InvalidOperationException("A round must be started before completing a turn.");
        }

        var resolvedTurn = CurrentRound.ConsumeCurrentTurn();

        if (CurrentRound.CurrentQueue.IsEmpty)
        {
            Phase = CombatPhase.RoundEnd;
            return resolvedTurn;
        }

        Phase = CombatPhase.RoundSetup;
        return resolvedTurn;
    }


    public void FinishBattle(BattleResult result)
    {
        Result = result;
        Phase = CombatPhase.Finished;
    }

    public CombatUnitState GetUnitState(UnitReference unitReference)
    {
        if (unitReference is null)
        {
            throw new ArgumentNullException(nameof(unitReference));
        }

        if (_unitStates.TryGetValue(unitReference.Unit, out var state))
        {
            return state;
        }

        throw new KeyNotFoundException($"Unit '{unitReference.Unit.Name}' is not part of the combat state.");
    }

    public void MarkUnitAsDead(UnitReference unitReference)
    {
        var unitState = GetUnitState(unitReference);
        unitState.IsAlive = false;
        unitState.CurrentHp = 0;
        unitState.CanActThisRound = false;
        unitState.CanActNextRound = false;

        CurrentRound?.CurrentQueue.RemoveAllForUnit(unitReference);
        CurrentRound?.NextQueue.RemoveAllForUnit(unitReference);
    }

    public int ApplyDamage(UnitReference target, int damage)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative.");
        }

        var targetState = GetUnitState(target);
        targetState.CurrentHp = Math.Max(0, targetState.CurrentHp - damage);
        if (targetState.CurrentHp == 0)
        {
            MarkUnitAsDead(target);
        }

        return targetState.CurrentHp;
    }

    public bool TryConsumeBeastShield(UnitReference beastReference)
    {
        if (beastReference.Kind != CombatantKind.Beast)
        {
            throw new InvalidOperationException("Shields can only be consumed for beasts.");
        }

        var beastState = GetUnitState(beastReference);
        if (beastState.CurrentShields <= 0)
        {
            return false;
        }

        beastState.CurrentShields--;
        if (beastState.CurrentShields > 0)
        {
            return false;
        }

        EnterBreakingPoint(beastReference, beastState);
        return true;
    }

    public IReadOnlyList<UnitReference> GetAliveTravelers()
    {
        return _unitStates.Values
            .Where(state => state.UnitReference.Kind == CombatantKind.Traveler && state.IsAlive)
            .OrderBy(state => state.UnitReference.BoardPosition)
            .Select(state => state.UnitReference)
            .ToList();
    }

    public IReadOnlyList<UnitReference> GetAliveBeasts()
    {
        return _unitStates.Values
            .Where(state => state.UnitReference.Kind == CombatantKind.Beast && state.IsAlive)
            .OrderBy(state => state.UnitReference.BoardPosition)
            .Select(state => state.UnitReference)
            .ToList();
    }

    public CombatViewSnapshot BuildViewSnapshot()
    {
        var roundNumber = CurrentRound?.Number ?? 0;

        var playerTeam = _unitStates.Values
            .Where(state => state.UnitReference.Kind == CombatantKind.Traveler)
            .OrderBy(state => state.UnitReference.BoardPosition)
            .Select(BuildUnitDisplaySnapshot)
            .ToList();

        var enemyTeam = _unitStates.Values
            .Where(state => state.UnitReference.Kind == CombatantKind.Beast)
            .OrderBy(state => state.UnitReference.BoardPosition)
            .Select(BuildUnitDisplaySnapshot)
            .ToList();

        var currentRoundTurns = CurrentRound?.CurrentQueue.Entries
            .Select(entry => entry.UnitReference.Unit.Name)
            .ToList() ?? new List<string>();

        var nextRoundTurns = CurrentRound?.NextQueue.Entries
            .Select(entry => entry.UnitReference.Unit.Name)
            .ToList() ?? new List<string>();

        return new CombatViewSnapshot(roundNumber, playerTeam, enemyTeam, currentRoundTurns, nextRoundTurns);
    }

    private static Dictionary<Unit, CombatUnitState> BuildUnitStates(BattleBoard board)
    {
        var result = new Dictionary<Unit, CombatUnitState>();

        for (var index = 0; index < board.PlayerSlots.Count; index++)
        {
            var traveler = board.PlayerSlots[index];
            if (traveler is null)
            {
                continue;
            }

            var reference = new UnitReference(traveler, CombatantKind.Traveler, index);
            result[traveler] = new CombatUnitState(reference);
        }

        for (var index = 0; index < board.EnemySlots.Count; index++)
        {
            var beast = board.EnemySlots[index];
            if (beast is null)
            {
                continue;
            }

            var reference = new UnitReference(beast, CombatantKind.Beast, index);
            result[beast] = new CombatUnitState(reference);
        }

        return result;
    }

    private void ResetRoundFlagsForLivingUnits()
    {
        foreach (var state in _unitStates.Values)
        {
            state.UsedBoostingThisRound = false;
            state.IsDefending = false;

            if (!state.IsAlive)
            {
                state.CanActThisRound = false;
                state.CanActNextRound = false;
            }
        }
    }

    private static UnitDisplaySnapshot BuildUnitDisplaySnapshot(CombatUnitState state)
    {
        var slotLabel = BuildSlotLabel(state.UnitReference.BoardPosition);
        return new UnitDisplaySnapshot(
            slotLabel,
            state.UnitReference.Unit.Name,
            state.UnitReference.Kind,
            state.CurrentHp,
            state.MaxHp,
            state.CurrentSp,
            state.MaxSp,
            state.CurrentBp,
            state.CurrentShields);
    }

    private static string BuildSlotLabel(int boardPosition)
    {
        return ((char)('A' + boardPosition)).ToString();
    }

    private void EnterBreakingPoint(UnitReference beastReference, CombatUnitState beastState)
    {
        beastState.BreakingRoundsRemaining = 2;
        beastState.HasBreakingRecoveryPriorityThisRound = false;
        beastState.HasBreakingRecoveryPriorityNextRound = false;
        beastState.CanActThisRound = false;
        beastState.CanActNextRound = false;

        CurrentRound?.CurrentQueue.RemoveAllForUnit(beastReference);
        CurrentRound?.NextQueue.RemoveAllForUnit(beastReference);
    }
}

