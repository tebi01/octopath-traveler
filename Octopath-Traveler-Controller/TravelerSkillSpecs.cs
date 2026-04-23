namespace Octopath_Traveler;

internal static class TravelerSkillSpecs
{
    public static int GetSkillSpCost(string skillName)
    {
        return skillName switch
        {
            "Holy Light" => 6,
            "Tradewinds" => 7,
            "Cross Strike" => 12,
            "Moonlight Waltz" => 7,
            "Icicle" => 7,
            "Amputation" => 8,
            "Wildfire" => 7,
            "True Strike" => 10,
            "Thunderbird" => 7,
            "Mercy Strike" => 4,
            "Qilin's Horn" => 35,
            "Phoenix Storm" => 35,
            "Fireball" => 8,
            "Icewind" => 8,
            "Lightning Bolt" => 8,
            "Luminescence" => 9,
            "Trade Tempest" => 10,
            "Level Slash" => 9,
            "Night Ode" => 10,
            "Tiger Rage" => 35,
            "Yatagarasu" => 35,
            "Fox Spirit" => 35,
            "Last Stand" => 16,
            "Spearhead" => 6,
            "Leghold Trap" => 6,
            "Nightmare Chimera" => 35,
            "Shooting Stars" => 35,
            "Heal Wounds" => 6,
            "Heal More" => 25,
            "First Aid" => 4,
            "Revive" => 50,
            "Vivify" => 16,
            _ => 0
        };
    }

    public static bool TryGetSingleTargetOffensiveSkill(string skillName, out OffensiveSkillSpec skill)
    {
        switch (skillName)
        {
            case "Holy Light":
                skill = new OffensiveSkillSpec(6, 1.5, "Light", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Tradewinds":
                skill = new OffensiveSkillSpec(7, 1.5, "Wind", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Cross Strike":
                skill = new OffensiveSkillSpec(12, 1.7, "Sword", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Moonlight Waltz":
                skill = new OffensiveSkillSpec(7, 1.6, "Dark", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Icicle":
                skill = new OffensiveSkillSpec(7, 1.5, "Ice", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Amputation":
                skill = new OffensiveSkillSpec(8, 1.7, "Axe", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Wildfire":
                skill = new OffensiveSkillSpec(7, 1.6, "Fire", IsElemental: true, IsMercyStrike: false);
                return true;
            case "True Strike":
                skill = new OffensiveSkillSpec(10, 2.0, "Bow", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Thunderbird":
                skill = new OffensiveSkillSpec(7, 1.6, "Lightning", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Mercy Strike":
                skill = new OffensiveSkillSpec(4, 1.5, "Bow", IsElemental: false, IsMercyStrike: true);
                return true;
            case "Qilin's Horn":
                skill = new OffensiveSkillSpec(35, 2.1, "Spear", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Phoenix Storm":
                skill = new OffensiveSkillSpec(35, 2.1, "Bow", IsElemental: false, IsMercyStrike: false);
                return true;
            default:
                skill = new OffensiveSkillSpec(0, 0, string.Empty, IsElemental: false, IsMercyStrike: false);
                return false;
        }
    }

    public static bool TryGetEnemiesTargetOffensiveSkill(string skillName, out OffensiveSkillSpec skill)
    {
        switch (skillName)
        {
            case "Fireball":
                skill = new OffensiveSkillSpec(8, 1.5, "Fire", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Icewind":
                skill = new OffensiveSkillSpec(8, 1.5, "Ice", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Lightning Bolt":
                skill = new OffensiveSkillSpec(8, 1.5, "Lightning", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Luminescence":
                skill = new OffensiveSkillSpec(9, 1.5, "Light", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Trade Tempest":
                skill = new OffensiveSkillSpec(10, 1.5, "Wind", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Level Slash":
                skill = new OffensiveSkillSpec(9, 1.5, "Sword", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Night Ode":
                skill = new OffensiveSkillSpec(10, 1.6, "Dark", IsElemental: true, IsMercyStrike: false);
                return true;
            case "Tiger Rage":
                skill = new OffensiveSkillSpec(35, 1.9, "Axe", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Yatagarasu":
                skill = new OffensiveSkillSpec(35, 1.9, "Dagger", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Fox Spirit":
                skill = new OffensiveSkillSpec(35, 1.9, "Stave", IsElemental: false, IsMercyStrike: false);
                return true;
            case "Last Stand":
                skill = new OffensiveSkillSpec(16, 1.4, "Axe", IsElemental: false, IsMercyStrike: false);
                return true;
            default:
                skill = new OffensiveSkillSpec(0, 0, string.Empty, IsElemental: false, IsMercyStrike: false);
                return false;
        }
    }
}

