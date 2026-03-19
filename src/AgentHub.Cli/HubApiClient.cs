using System.Net.Http.Json;
using System.Text.Json;

namespace AgentHub.Cli;

/// <summary>
/// Typed HTTP client for the Agent Hub REST API.
/// </summary>
public class HubApiClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public HubApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
        };
    }

    // --- Agents ---

    public async Task<JsonElement> GetAgentsAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("api/agents", cancellationToken);
    }

    public async Task<JsonElement> RegisterAgentAsync(string name, string description, CancellationToken cancellationToken = default)
    {
        var payload = new { name, description };
        return await PostJsonAsync("api/agents", payload, cancellationToken);
    }

    public async Task<JsonElement> CheckInAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await PostJsonAsync($"api/agents/{agentId}/checkin", new { }, cancellationToken);
    }

    public async Task<JsonElement> SetAgentTaskAsync(Guid agentId, string description, CancellationToken cancellationToken = default)
    {
        var payload = new { description };
        return await PostJsonAsync($"api/agents/{agentId}/task", payload, cancellationToken);
    }

    public async Task SetAgentIdleAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/agents/{agentId}/task", cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task<JsonElement> GetAgentActivitiesAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"api/agents/{agentId}/activities", cancellationToken);
    }

    public async Task<JsonElement> GetAgentSkillsAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"api/agents/{agentId}/skills", cancellationToken);
    }

    public async Task<JsonElement> SetAgentSkillsAsync(Guid agentId, string skillsJson, CancellationToken cancellationToken = default)
    {
        var skills = JsonSerializer.Deserialize<JsonElement>(skillsJson, SerializerOptions);
        var payload = new { skills };
        var response = await _httpClient.PutAsJsonAsync($"api/agents/{agentId}/skills", payload, SerializerOptions, cancellationToken);
        await EnsureSuccessAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<JsonElement>(json, SerializerOptions);
    }

    // --- Messages ---

    public async Task<JsonElement> GetInboxAsync(Guid agentId, bool includeRead, CancellationToken cancellationToken = default)
    {
        var path = includeRead ? $"api/messages/inbox/{agentId}/all" : $"api/messages/inbox/{agentId}";
        return await GetJsonAsync(path, cancellationToken);
    }

    public async Task<JsonElement> GetOutboxAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"api/messages/outbox/{agentId}", cancellationToken);
    }

    public async Task<JsonElement> SendMessageAsync(
        Guid fromAgentId,
        string toAgentId,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var payload = new { fromAgentId, toAgentId, subject, body };
        return await PostJsonAsync("api/messages", payload, cancellationToken);
    }

    public async Task<JsonElement> MarkReadAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return await PostJsonAsync($"api/messages/{messageId}/read", new { }, cancellationToken);
    }

    public async Task<JsonElement> AttachFileAsync(Guid messageId, string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}", filePath);

        await using var fileStream = File.OpenRead(filePath);
        var fileName = Path.GetFileName(filePath);
        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            GetContentType(fileName));
        content.Add(fileContent, "file", fileName);

        var response = await _httpClient.PostAsync($"api/messages/{messageId}/attachments", content, cancellationToken);
        await EnsureSuccessAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<JsonElement>(json, SerializerOptions);
    }

    public async Task DownloadAttachmentAsync(Guid messageId, Guid attachmentId, string outputPath, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"api/messages/{messageId}/attachments/{attachmentId}",
            cancellationToken);
        await EnsureSuccessAsync(response);

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        await File.WriteAllBytesAsync(outputPath, bytes, cancellationToken);
    }

    private async Task<JsonElement> GetJsonAsync(string path, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<JsonElement>(json, SerializerOptions);
    }

    private async Task<JsonElement> PostJsonAsync(string path, object payload, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(path, payload, SerializerOptions, cancellationToken);
        await EnsureSuccessAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<JsonElement>(json, SerializerOptions);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HubApiException((int)response.StatusCode, body);
        }
    }

    private static string GetContentType(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}

public class HubApiException(int statusCode, string responseBody)
    : Exception($"API error {statusCode}: {responseBody}")
{
    public int StatusCode { get; } = statusCode;
    public string ResponseBody { get; } = responseBody;
}
