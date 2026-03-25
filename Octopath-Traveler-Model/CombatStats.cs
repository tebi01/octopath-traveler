namespace Octopath_Traveler_Model;

public sealed class CombatStats
{
    public int MaxHp { get; }
    public int CurrentHp { get; }
    public int PhysicalAttack { get; }
    public int PhysicalDefense { get; }
    public int ElementalAttack { get; }
    public int ElementalDefense { get; }
    public int Speed { get; }

    public CombatStats(
        int maxHp,
        int currentHp,
        int physicalAttack,
        int physicalDefense,
        int elementalAttack,
        int elementalDefense,
        int speed)
    {
        ValidateHp(maxHp, currentHp);
        ValidateCombatValues(physicalAttack, physicalDefense, elementalAttack, elementalDefense, speed);

        MaxHp = maxHp;
        CurrentHp = currentHp;
        PhysicalAttack = physicalAttack;
        PhysicalDefense = physicalDefense;
        ElementalAttack = elementalAttack;
        ElementalDefense = elementalDefense;
        Speed = speed;
    }

    private static void ValidateHp(int maxHp, int currentHp)
    {
        if (maxHp <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHp), "Max HP must be greater than zero.");
        }

        if (currentHp < 0 || currentHp > maxHp)
        {
            throw new ArgumentOutOfRangeException(nameof(currentHp), "Current HP must be between 0 and Max HP.");
        }
    }

    private static void ValidateCombatValues(
        int physicalAttack,
        int physicalDefense,
        int elementalAttack,
        int elementalDefense,
        int speed)
    {
        ValidateNonNegative(physicalAttack, nameof(physicalAttack));
        ValidateNonNegative(physicalDefense, nameof(physicalDefense));
        ValidateNonNegative(elementalAttack, nameof(elementalAttack));
        ValidateNonNegative(elementalDefense, nameof(elementalDefense));
        ValidateNonNegative(speed, nameof(speed));
    }

    private static void ValidateNonNegative(int value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Stat values cannot be negative.");
        }
    }
}
