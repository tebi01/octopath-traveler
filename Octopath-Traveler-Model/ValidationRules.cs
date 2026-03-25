namespace Octopath_Traveler_Model;

internal static class ValidationRules
{
    public static IReadOnlyList<string> NormalizeNonEmptyStrings(IEnumerable<string>? values, string paramName)
    {
        if (values is null)
        {
            return Array.Empty<string>();
        }

        var normalizedValues = new List<string>();
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Values cannot contain null or empty entries.", paramName);
            }

            normalizedValues.Add(value.Trim());
        }

        return normalizedValues;
    }

    public static void EnsureUniqueStrings(IReadOnlyList<string> values, string paramName)
    {
        var uniqueValues = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
        if (uniqueValues.Count != values.Count)
        {
            throw new ArgumentException("Values cannot contain duplicates.", paramName);
        }
    }

    public static void EnsureUniqueUnitNames(IEnumerable<string> names, string paramName)
    {
        var normalizedNames = names.ToList();
        var uniqueNames = new HashSet<string>(normalizedNames, StringComparer.OrdinalIgnoreCase);
        if (uniqueNames.Count != normalizedNames.Count)
        {
            throw new ArgumentException("Team cannot contain duplicated units.", paramName);
        }
    }
}

