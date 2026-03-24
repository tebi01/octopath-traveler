namespace Octopath_Traveler_Model.CombatFlow;

public sealed class RoundState
{
    public int Number { get; }
    public TurnQueue CurrentQueue { get; }
    public TurnQueue NextQueue { get; private set; }
    public IReadOnlyList<TurnEntry> ResolvedTurns => _resolvedTurns;

    private readonly List<TurnEntry> _resolvedTurns;

    public RoundState(int number, TurnQueue currentQueue, TurnQueue nextQueue)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Round number must be greater than zero.");
        }

        Number = number;
        CurrentQueue = currentQueue ?? throw new ArgumentNullException(nameof(currentQueue));
        NextQueue = nextQueue ?? throw new ArgumentNullException(nameof(nextQueue));
        _resolvedTurns = new List<TurnEntry>();
    }

    public TurnEntry? PeekCurrentTurn()
    {
        return CurrentQueue.PeekFirst();
    }

    public TurnEntry? ConsumeCurrentTurn()
    {
        var turn = CurrentQueue.PopFirst();
        if (turn is not null)
        {
            _resolvedTurns.Add(turn);
        }

        return turn;
    }

    public void ReplaceNextQueue(TurnQueue nextQueue)
    {
        NextQueue = nextQueue ?? throw new ArgumentNullException(nameof(nextQueue));
    }
}

