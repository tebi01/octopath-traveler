using Octopath_Traveler_Model.CombatFlow;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

internal sealed class TravelerSkillDispatchResolver
{
    private readonly MainConsoleView _view;
    private readonly TravelerTacticalSkillResolver _tacticalSkillResolver;
    private readonly TravelerOffensiveSkillResolver _offensiveSkillResolver;
    private readonly TravelerSupportSkillResolver _supportSkillResolver;

    public TravelerSkillDispatchResolver(
        MainConsoleView view,
        TravelerTacticalSkillResolver tacticalSkillResolver,
        TravelerOffensiveSkillResolver offensiveSkillResolver,
        TravelerSupportSkillResolver supportSkillResolver)
    {
        _view = view;
        _tacticalSkillResolver = tacticalSkillResolver;
        _offensiveSkillResolver = offensiveSkillResolver;
        _supportSkillResolver = supportSkillResolver;
    }

    public bool TryResolveTravelerSkills(TravelerTurnContext travelerTurnContext)
    {
        var availableSkills = GetAvailableSkillsForTraveler(travelerTurnContext);
        var selectedSkill = _view.AskTravelerSkill(travelerTurnContext.Traveler.Name, availableSkills);
        if (selectedSkill == availableSkills.Count + 1)
        {
            return false;
        }

        var skillName = availableSkills[selectedSkill - 1];
        return skillName switch
        {
            "Spearhead" => _tacticalSkillResolver.TryResolveSpearhead(travelerTurnContext),
            "Leghold Trap" => _tacticalSkillResolver.TryResolveLegholdTrap(travelerTurnContext),
            "Nightmare Chimera" => _offensiveSkillResolver.TryResolveNightmareChimera(travelerTurnContext),
            "Shooting Stars" => _offensiveSkillResolver.TryResolveShootingStars(travelerTurnContext),
            "Heal Wounds" => _supportSkillResolver.TryResolvePartyHeal(travelerTurnContext, "Heal Wounds", 6, 1.5),
            "Heal More" => _supportSkillResolver.TryResolvePartyHeal(travelerTurnContext, "Heal More", 25, 2.0),
            "First Aid" => _supportSkillResolver.TryResolveFirstAid(travelerTurnContext),
            "Revive" => _supportSkillResolver.TryResolveRevive(travelerTurnContext),
            "Vivify" => _supportSkillResolver.TryResolveVivify(travelerTurnContext),
            _ => _offensiveSkillResolver.TryResolveSingleTargetOffensiveSkill(travelerTurnContext, skillName)
                || _offensiveSkillResolver.TryResolveEnemiesTargetOffensiveSkill(travelerTurnContext, skillName)
        };
    }

    private static IReadOnlyList<string> GetAvailableSkillsForTraveler(TravelerTurnContext travelerTurnContext)
    {
        var travelerState = travelerTurnContext.CombatState.GetUnitState(travelerTurnContext.TravelerTurn.UnitReference);
        return travelerTurnContext.Traveler.ActiveSkills
            .Where(skill => TravelerSkillSpecs.GetSkillSpCost(skill) <= travelerState.CurrentSp)
            .ToList();
    }
}

