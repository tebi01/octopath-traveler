namespace Octopath_Traveler;

internal sealed record ParsedTeam(IReadOnlyList<ParsedTravelerLine> Travelers, IReadOnlyList<string> Beasts);
