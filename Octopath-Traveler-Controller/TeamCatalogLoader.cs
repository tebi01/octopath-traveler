namespace Octopath_Traveler;

internal sealed class TeamCatalogLoader : TeamCatalogProvider
{
    private const string CharactersFileName = "characters.json";
    private const string EnemiesFileName = "enemies.json";
    private const string ActiveSkillsFileName = "skills.json";
    private const string PassiveSkillsFileName = "passive_skills.json";
    private const string BeastSkillsFileName = "beast_skills.json";

    private readonly TeamDataFolderPathResolver _pathResolver;
    private readonly TeamJsonCatalogReader _jsonCatalogReader;
    private readonly TeamCatalogsAssembler _catalogsAssembler;

    public TeamCatalogLoader()
        : this(
            new DefaultTeamDataFolderPathResolver(),
            new DefaultTeamJsonCatalogReader(),
            new DefaultTeamCatalogsAssembler())
    {
    }

    internal TeamCatalogLoader(
        TeamDataFolderPathResolver pathResolver,
        TeamJsonCatalogReader jsonCatalogReader,
        TeamCatalogsAssembler catalogsAssembler)
    {
        _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
        _jsonCatalogReader = jsonCatalogReader ?? throw new ArgumentNullException(nameof(jsonCatalogReader));
        _catalogsAssembler = catalogsAssembler ?? throw new ArgumentNullException(nameof(catalogsAssembler));
    }

    public Catalogs Load(string teamFilePath)
    {
        var dataFolder = _pathResolver.ResolveFromTeamFilePath(teamFilePath);
        var characters = LoadCharacters(dataFolder);
        var enemies = LoadEnemies(dataFolder);
        var activeSkills = LoadSkillCatalog(dataFolder, ActiveSkillsFileName);
        var passiveSkills = LoadSkillCatalog(dataFolder, PassiveSkillsFileName);
        var beastSkills = LoadSkillCatalog(dataFolder, BeastSkillsFileName);

        return _catalogsAssembler.Build(characters, enemies, activeSkills, passiveSkills, beastSkills);
    }

    private List<CharacterDto> LoadCharacters(string dataFolder)
        => ReadCatalog<List<CharacterDto>>(dataFolder, CharactersFileName);

    private List<EnemyDto> LoadEnemies(string dataFolder)
        => ReadCatalog<List<EnemyDto>>(dataFolder, EnemiesFileName);

    private List<SkillDto> LoadSkillCatalog(string dataFolder, string fileName)
        => ReadCatalog<List<SkillDto>>(dataFolder, fileName);

    private T ReadCatalog<T>(string dataFolder, string fileName)
    {
        return _jsonCatalogReader.Read<T>(Path.Combine(dataFolder, fileName));
    }


}
