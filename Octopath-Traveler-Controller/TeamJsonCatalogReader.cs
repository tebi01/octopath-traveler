using System.Text.Json;

namespace Octopath_Traveler;

internal interface TeamJsonCatalogReader
{
    T Read<T>(string path);
}

internal sealed class DefaultTeamJsonCatalogReader : TeamJsonCatalogReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public T Read<T>(string path)
    {
        var json = File.ReadAllText(path);
        var value = JsonSerializer.Deserialize<T>(json, JsonOptions);

        return value ?? throw new InvalidOperationException($"Unable to parse {path}");
    }
}

