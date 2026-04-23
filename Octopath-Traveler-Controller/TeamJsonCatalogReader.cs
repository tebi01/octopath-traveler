namespace Octopath_Traveler;

internal interface TeamJsonCatalogReader
{
    T Read<T>(string path);
}

