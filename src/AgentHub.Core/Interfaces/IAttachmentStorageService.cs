namespace AgentHub.Core.Interfaces;

public interface IAttachmentStorageService
{
    Task<string> SaveAsync(Guid messageId, string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> ReadAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
