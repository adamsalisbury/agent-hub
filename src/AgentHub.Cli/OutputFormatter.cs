using System.Text.Json;

namespace AgentHub.Cli;

/// <summary>
/// Handles JSON output formatting.
/// </summary>
public static class OutputFormatter
{
    private static readonly JsonSerializerOptions PrettyOptions = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions CompactOptions = new() { WriteIndented = false };

    public static void WriteSuccess(JsonElement data, bool pretty)
    {
        Console.WriteLine(pretty
            ? JsonSerializer.Serialize(data, PrettyOptions)
            : JsonSerializer.Serialize(data, CompactOptions));
    }

    public static void WriteError(string message, bool pretty)
    {
        var errorObject = new { error = message };
        Console.Error.WriteLine(pretty
            ? JsonSerializer.Serialize(errorObject, PrettyOptions)
            : JsonSerializer.Serialize(errorObject, CompactOptions));
    }

    public static void WriteMessage(string message, bool pretty)
    {
        var obj = new { message };
        Console.WriteLine(pretty
            ? JsonSerializer.Serialize(obj, PrettyOptions)
            : JsonSerializer.Serialize(obj, CompactOptions));
    }
}
