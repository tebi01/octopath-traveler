using Octopath_Traveler_Model;
using Octopath_Traveler_Model.CombatFlow;

namespace Octopath_Traveler;

internal sealed class TravelerCombatMathService
{
    public bool HasWeaknessAgainstAttackType(UnitReference beastReference, string attackType)
    {
        if (beastReference.Unit is not Beast beast)
        {
            return false;
        }

        return beast.Weaknesses.Any(weakness => string.Equals(weakness, attackType, StringComparison.OrdinalIgnoreCase));
    }

    public double GetTravelerDamageMultiplier(bool hasWeakness, bool isTargetInBreakingPoint)
    {
        if (hasWeakness && isTargetInBreakingPoint)
        {
            return 2.0;
        }

        if (hasWeakness || isTargetInBreakingPoint)
        {
            return 1.5;
        }

        return 1.0;
    }

    public int ApplyMultiplier(double baseValue, double multiplier)
    {
        return Math.Max(0, Convert.ToInt32(Math.Floor(baseValue * multiplier)));
    }

    public double CalculatePhysicalDamageRaw(int attackerPhysicalAttack, int targetPhysicalDefense, double modifier)
    {
        var rawDamage = attackerPhysicalAttack * modifier - targetPhysicalDefense;
        return Math.Max(0, rawDamage);
    }

    public double CalculateElementalDamageRaw(int attackerElementalAttack, int targetElementalDefense, double modifier)
    {
        var rawDamage = attackerElementalAttack * modifier - targetElementalDefense;
        return Math.Max(0, rawDamage);
    }

    public double CalculateLastStandRawDamage(TravelerTurnContext travelerTurnContext, UnitReference target, double baseModifier)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        var missingHpRatio = (travelerState.MaxHp - travelerState.CurrentHp) / (double)travelerState.MaxHp;
        var baseDamage = CalculatePhysicalDamageRaw(
            travelerTurnContext.Traveler.Stats.PhysicalAttack,
            target.Unit.Stats.PhysicalDefense,
            baseModifier);

        var missingHpPercent = Math.Floor(missingHpRatio * 100);
        var scalingFactor = 1 + 0.03 * missingHpPercent;
        return baseDamage * scalingFactor;
    }

    public double CalculateTravelerBasicAttackRawDamage(TravelerBasicAttackContext basicAttackContext, double basicAttackModifier)
    {
        return CalculatePhysicalDamageRaw(
            basicAttackContext.TravelerTurnContext.Traveler.Stats.PhysicalAttack,
            basicAttackContext.TargetBeast.Unit.Stats.PhysicalDefense,
            basicAttackModifier);
    }
}

