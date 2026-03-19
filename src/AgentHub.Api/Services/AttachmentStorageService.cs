using AgentHub.Core.Interfaces;

namespace AgentHub.Api.Services;

public class AttachmentStorageService(IConfiguration configuration, ILogger<AttachmentStorageService> logger)
    : IAttachmentStorageService
{
    private string AttachmentsDirectory =>
        configuration["AgentHub:AttachmentsDirectory"] ?? "attachments";

    public async Task<string> SaveAsync(
        Guid messageId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var messageDirectory = Path.Combine(AttachmentsDirectory, messageId.ToString());
        Directory.CreateDirectory(messageDirectory);

        // Sanitize filename to prevent path traversal
        var sanitizedFileName = Path.GetFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
        var storagePath = Path.Combine(messageDirectory, uniqueFileName);

        await using var fileStream = File.Create(storagePath);
        await content.CopyToAsync(fileStream, cancellationToken);

        logger.LogInformation("Saved attachment {FileName} to {StoragePath}", fileName, storagePath);
        return storagePath;
    }

    public Task<Stream> ReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(storagePath))
            throw new FileNotFoundException($"Attachment not found at path: {storagePath}", storagePath);

        Stream fileStream = File.OpenRead(storagePath);
        return Task.FromResult(fileStream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(storagePath))
        {
            File.Delete(storagePath);
            logger.LogInformation("Deleted attachment at {StoragePath}", storagePath);
        }

        return Task.CompletedTask;
    }
}
