using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler;

public sealed class TravelerBasicAttackContext
{
    public TravelerTurnContext TravelerTurnContext { get; }
    public string WeaponType { get; }
    public UnitReference TargetBeast { get; }

    public TravelerBasicAttackContext(TravelerTurnContext travelerTurnContext, string weaponType, UnitReference targetBeast)
    {
        TravelerTurnContext = travelerTurnContext ?? throw new ArgumentNullException(nameof(travelerTurnContext));
        if (string.IsNullOrWhiteSpace(weaponType))
        {
            throw new ArgumentException("Weapon type cannot be null or empty.", nameof(weaponType));
        }

        WeaponType = weaponType;
        TargetBeast = targetBeast ?? throw new ArgumentNullException(nameof(targetBeast));
    }
}
