namespace Octopath_Traveler_Model.CombatFlow;

public sealed class TurnQueue
{
    private readonly List<TurnEntry> _entries;

    public IReadOnlyList<TurnEntry> Entries => _entries;
    public bool IsEmpty => _entries.Count == 0;
    public int Count => _entries.Count;

    public TurnQueue(IEnumerable<TurnEntry>? entries = null)
    {
        _entries = entries?.ToList() ?? new List<TurnEntry>();
    }

    public TurnEntry? PeekFirst()
    {
        return IsEmpty ? null : _entries[0];
    }

    public TurnEntry? PopFirst()
    {
        if (IsEmpty)
        {
            return null;
        }

        var first = _entries[0];
        _entries.RemoveAt(0);
        return first;
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

