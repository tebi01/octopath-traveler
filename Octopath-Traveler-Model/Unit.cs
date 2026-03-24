namespace Octopath_Traveler_Model;

public abstract class Unit
{
	public string Name { get; }
	public CombatStats Stats { get; }

	public int Speed => Stats.Speed;

	protected Unit(string name, CombatStats stats)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Unit name cannot be null or empty.", nameof(name));
		}

		Name = name;
		Stats = stats ?? throw new ArgumentNullException(nameof(stats));
	}
}