namespace TopAOAIConnector.Models;

internal class AOAISettings
{
    public static AOAISettings Instance { get; set; }

    static AOAISettings()
    {
        Instance = new();
    }

    public string? Endpoint { get; set; }
    public string? SecretKey1 { get; set; }
    public string? SecretKey2 { get; set; }
    public string? DeployModelName { get; set; }
}
