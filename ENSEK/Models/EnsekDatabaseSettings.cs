namespace Models;

public class EnsekDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string[] CollectionNames { get; set; } = [];
}