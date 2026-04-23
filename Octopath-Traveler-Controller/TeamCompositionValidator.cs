using Octopath_Traveler_Model;

namespace Octopath_Traveler;

internal static class TeamCompositionValidator
{
    public static void ValidateTeam(ParsedTeam parsedTeam, Catalogs catalogs)
    {
        ValidateTeamSize(parsedTeam);
        ValidateTeamDuplicates(parsedTeam);
        ValidateTravelers(parsedTeam.Travelers, catalogs);
        ValidateBeasts(parsedTeam.Beasts, catalogs);
    }

    private static void ValidateTeamSize(ParsedTeam parsedTeam)
    {
        if (parsedTeam.Travelers.Count is < PlayerTeam.MinTravelers or > PlayerTeam.MaxTravelers)
        {
            throw new InvalidOperationException("Invalid traveler count.");
        }

        if (parsedTeam.Beasts.Count is < EnemyTeam.MinBeasts or > EnemyTeam.MaxBeasts)
        {
            throw new InvalidOperationException("Invalid beast count.");
        }
    }

    private static void ValidateTeamDuplicates(ParsedTeam parsedTeam)
    {
        EnsureNoDuplicates(parsedTeam.Travelers.Select(traveler => traveler.Name));
        EnsureNoDuplicates(parsedTeam.Beasts);
    }

    private static void ValidateTravelers(IEnumerable<ParsedTravelerLine> travelers, Catalogs catalogs)
    {
        foreach (var traveler in travelers)
        {
            ValidateTraveler(traveler, catalogs);
        }
    }

    private static void ValidateTraveler(ParsedTravelerLine traveler, Catalogs catalogs)
    {
        ValidateTravelerExists(traveler, catalogs);
        ValidateTravelerSkillCounts(traveler);
        ValidateTravelerSkillDuplicates(traveler);
        ValidateTravelerSkillsExist(traveler, catalogs);
    }

    private static void ValidateTravelerExists(ParsedTravelerLine traveler, Catalogs catalogs)
    {
        if (!catalogs.Characters.ContainsKey(traveler.Name))
        {
            throw new InvalidOperationException("Unknown traveler.");
        }
    }

    private static void ValidateTravelerSkillCounts(ParsedTravelerLine traveler)
    {
        if (traveler.ActiveSkills.Count > Traveler.MaxActiveSkills || traveler.PassiveSkills.Count > Traveler.MaxPassiveSkills)
        {
            throw new InvalidOperationException("Too many skills.");
        }
    }

    private static void ValidateTravelerSkillDuplicates(ParsedTravelerLine traveler)
    {
        EnsureNoDuplicates(traveler.ActiveSkills);
        EnsureNoDuplicates(traveler.PassiveSkills);
    }

    private static void ValidateTravelerSkillsExist(ParsedTravelerLine traveler, Catalogs catalogs)
    {
        ValidateKnownSkills(traveler.ActiveSkills, catalogs.ActiveSkills, "Unknown active skill.");
        ValidateKnownSkills(traveler.PassiveSkills, catalogs.PassiveSkills, "Unknown passive skill.");
    }

    private static void ValidateKnownSkills(IEnumerable<string> skills, HashSet<string> catalogSkills, string errorMessage)
    {
        foreach (var skill in skills)
        {
            if (!catalogSkills.Contains(skill))
            {
                throw new InvalidOperationException(errorMessage);
            }
        }
    }

    private static void ValidateBeasts(IEnumerable<string> beasts, Catalogs catalogs)
    {
        foreach (var beast in beasts)
        {
            ValidateBeast(beast, catalogs);
        }
    }

    private static void ValidateBeast(string beastName, Catalogs catalogs)
    {
        if (!catalogs.Enemies.ContainsKey(beastName))
        {
            throw new InvalidOperationException("Unknown beast.");
        }

        var enemy = catalogs.Enemies[beastName];
        ValidateEnemySkillExists(enemy.Skill, catalogs);
    }

    private static void ValidateEnemySkillExists(string enemySkill, Catalogs catalogs)
    {
        if (!catalogs.BeastSkills.Contains(enemySkill))
        {
            throw new InvalidOperationException("Unknown beast skill.");
        }
    }

    private static void EnsureNoDuplicates(IEnumerable<string> names)
    {
        var list = names.ToList();
        var unique = new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
        if (list.Count != unique.Count)
        {
            throw new InvalidOperationException("Duplicated entries are not allowed.");
        }
    }
}

