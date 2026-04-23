namespace Octopath_Traveler;

internal sealed class TeamValidatorAdapter : TeamValidator
{
    public void ValidateTeam(ParsedTeam parsedTeam, Catalogs catalogs)
    {
        TeamCompositionValidator.ValidateTeam(parsedTeam, catalogs);
    }
}

