namespace AgentHub.Data.JsonStore;

public class JsonStoreOptions
{
    public const string SectionName = "JsonStore";

    /// <summary>
    /// Base directory where JSON data files are stored.
    /// </summary>
    public string DataDirectory { get; set; } = "data";
}
