namespace Octopath_Traveler_Model.CombatFlow;

public sealed class TurnQueue
{
    private readonly List<TurnEntry> _entries;

    public IReadOnlyList<TurnEntry> Entries => _entries;
    public bool IsEmpty => _entries.Count == 0;
    public int Count => _entries.Count;

    public TurnQueue()
        : this(Array.Empty<TurnEntry>())
    {
    }

    public TurnQueue(IEnumerable<TurnEntry> entries)
    {
        _entries = entries?.ToList() ?? throw new ArgumentNullException(nameof(entries));
    }

    public bool TryPeekFirst(out TurnEntry? firstEntry)
    {
        if (IsEmpty)
        {
            firstEntry = default;
            return false;
        }

        firstEntry = _entries[0];
        return true;
    }

    public bool TryPopFirst(out TurnEntry? firstEntry)
    {
        if (IsEmpty)
        {
            firstEntry = default;
            return false;
        }

        firstEntry = _entries[0];
        _entries.RemoveAt(0);
        return true;
    }

    public void Add(TurnEntry entry)
    {
        _entries.Add(entry ?? throw new ArgumentNullException(nameof(entry)));
    }

    public void RemoveAllForUnit(UnitReference unitReference)
    {
        if (unitReference is null)
        {
            throw new ArgumentNullException(nameof(unitReference));
        }

        _entries.RemoveAll(entry => ReferenceEquals(entry.UnitReference.Unit, unitReference.Unit));
    }

    public TurnQueue Clone()
    {
        return new TurnQueue(_entries);
    }
}

