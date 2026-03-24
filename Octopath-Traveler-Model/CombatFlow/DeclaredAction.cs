namespace Octopath_Traveler_Model.CombatFlow;

public abstract class DeclaredAction
{
    public UnitReference Actor { get; }
    public CombatActionType Type { get; }
    public UnitReference? Target { get; }
    public int BoostPointsSpent { get; }

    protected DeclaredAction(UnitReference actor, CombatActionType type, UnitReference? target = null, int boostPointsSpent = 0)
    {
        Actor = actor ?? throw new ArgumentNullException(nameof(actor));
        Type = type;
        Target = target;

        if (boostPointsSpent < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(boostPointsSpent), "Boost points spent cannot be negative.");
        }

        BoostPointsSpent = boostPointsSpent;
    }
}

public sealed class TravelerDeclaredAction : DeclaredAction
{
    public string? WeaponType { get; }
    public string? ActiveSkillName { get; }

    public TravelerDeclaredAction(
        UnitReference actor,
        CombatActionType type,
        UnitReference? target = null,
        int boostPointsSpent = 0,
        string? weaponType = null,
        string? activeSkillName = null)
        : base(actor, type, target, boostPointsSpent)
    {
        WeaponType = weaponType;
        ActiveSkillName = activeSkillName;
    }

    public static TravelerDeclaredAction BasicAttack(UnitReference actor, UnitReference target, string weaponType, int bpSpent = 0)
    {
        return new TravelerDeclaredAction(actor, CombatActionType.BasicAttack, target, bpSpent, weaponType: weaponType);
    }

    public static TravelerDeclaredAction UseSkill(UnitReference actor, UnitReference target, string skillName)
    {
        return new TravelerDeclaredAction(actor, CombatActionType.UseActiveSkill, target, activeSkillName: skillName);
    }

    public static TravelerDeclaredAction Defend(UnitReference actor)
    {
        return new TravelerDeclaredAction(actor, CombatActionType.Defend);
    }

    public static TravelerDeclaredAction Flee(UnitReference actor)
    {
        return new TravelerDeclaredAction(actor, CombatActionType.Flee);
    }

    public static TravelerDeclaredAction Cancel(UnitReference actor)
    {
        return new TravelerDeclaredAction(actor, CombatActionType.Cancel);
    }
}

public sealed class BeastDeclaredAction : DeclaredAction
{
    public string? SkillName { get; }

    public BeastDeclaredAction(UnitReference actor, UnitReference? target = null, string? skillName = null)
        : base(actor, CombatActionType.BeastSkill, target)
    {
        SkillName = skillName;
    }

    public static BeastDeclaredAction Attack(UnitReference actor, UnitReference target)
    {
        return new BeastDeclaredAction(actor, target, "Attack");
    }
}

