namespace Octopath_Traveler;

internal interface TeamCatalogProvider
{
    Catalogs Load(string teamFilePath);
}
