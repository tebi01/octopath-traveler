namespace Octopath_Traveler_Model;

public sealed class SkillPoints
{
    public int MaxSp { get; }
    public int CurrentSp { get; }

    public SkillPoints(int maxSp, int currentSp)
    {
        ValidateSpRange(maxSp, currentSp);

        MaxSp = maxSp;
        CurrentSp = currentSp;
    }

    private static void ValidateSpRange(int maxSp, int currentSp)
    {
        if (maxSp < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSp), "Max SP cannot be negative.");
        }

        if (currentSp < 0 || currentSp > maxSp)
        {
            throw new ArgumentOutOfRangeException(nameof(currentSp), "Current SP must be between 0 and Max SP.");
        }
    }
}

