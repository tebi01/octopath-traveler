namespace Octopath_Traveler;

internal sealed record ParsedTravelerLine(
    string Name,
    IReadOnlyList<string> ActiveSkills,
    IReadOnlyList<string> PassiveSkills);
