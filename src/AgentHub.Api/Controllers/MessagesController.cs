using AgentHub.Api.ViewModels;
using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgentHub.Api.Controllers;

public class MessagesController(
    IMessageRepository messageRepository,
    IAgentRepository agentRepository,
    IAttachmentRepository attachmentRepository,
    IAttachmentStorageService storageService) : Controller
{
    public async Task<IActionResult> View(Guid id, CancellationToken cancellationToken)
    {
        var message = await messageRepository.GetByIdAsync(id, cancellationToken);
        if (message is null) return NotFound();

        var viewModel = new MessageDetailViewModel
        {
            Id = message.Id,
            SenderName = message.Sender?.Name ?? "Unknown",
            SenderId = message.SenderId,
            RecipientName = message.Recipient?.Name,
            RecipientId = message.RecipientId,
            Subject = message.Subject,
            Body = message.Body,
            IsBroadcast = message.IsBroadcast,
            IsRead = message.IsRead,
            SentAt = message.SentAt,
            ReadAt = message.ReadAt,
            Attachments = message.Attachments.Select(a => new AttachmentViewModel
            {
                Id = a.Id,
                MessageId = a.MessageId,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSizeBytes = a.FileSizeBytes,
                UploadedAt = a.UploadedAt
            }).ToList()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Send(Guid? fromAgentId, CancellationToken cancellationToken)
    {
        var agents = await agentRepository.GetAllAsync(cancellationToken);
        var viewModel = new SendMessageViewModel
        {
            FromAgentId = fromAgentId ?? Guid.Empty,
            Agents = agents.Select(a => new AgentSummaryViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                IsSystemAgent = a.IsSystemAgent
            }).ToList()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(SendMessageViewModel model, CancellationToken cancellationToken)
    {
        if (model.FromAgentId == Guid.Empty)
            ModelState.AddModelError("FromAgentId", "Please select a sender.");

        if (string.IsNullOrWhiteSpace(model.Subject))
            ModelState.AddModelError("Subject", "Subject is required.");

        if (string.IsNullOrWhiteSpace(model.Body))
            ModelState.AddModelError("Body", "Message body is required.");

        if (!ModelState.IsValid)
        {
            var agents = await agentRepository.GetAllAsync(cancellationToken);
            model.Agents = agents.Select(a => new AgentSummaryViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                IsSystemAgent = a.IsSystemAgent
            }).ToList();
            return View(model);
        }

        bool isBroadcast = string.Equals(model.ToAgentId, "all", StringComparison.OrdinalIgnoreCase);
        Guid? recipientId = null;

        if (!isBroadcast && Guid.TryParse(model.ToAgentId, out var recipientGuid))
        {
            recipientId = recipientGuid;
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = model.FromAgentId,
            RecipientId = recipientId,
            Subject = model.Subject,
            Body = model.Body,
            IsBroadcast = isBroadcast,
            IsRead = false,
            SentAt = DateTimeOffset.UtcNow
        };

        var created = await messageRepository.CreateAsync(message, cancellationToken);
        TempData["Success"] = "Message sent successfully.";
        return RedirectToAction("View", new { id = created.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var message = await messageRepository.GetByIdAsync(id, cancellationToken);
        if (message is null) return NotFound();

        if (!message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTimeOffset.UtcNow;
            await messageRepository.UpdateAsync(message, cancellationToken);
        }

        return RedirectToAction("View", new { id });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(Guid messageId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var attachment = await attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);
        if (attachment is null || attachment.MessageId != messageId)
            return NotFound();

        var stream = await storageService.ReadAsync(attachment.StoragePath, cancellationToken);
        return File(stream, attachment.ContentType, attachment.FileName);
    }
}
