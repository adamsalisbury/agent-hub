using AgentHub.Api.Models;
using AgentHub.Api.ViewModels;
using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;
using AgentHub.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentHub.Api.Controllers;

public class AgentsController(
    IAgentRepository agentRepository,
    IMessageRepository messageRepository,
    IAgentActivityRepository activityRepository) : Controller
{
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        var agent = await agentRepository.GetByIdAsync(id, cancellationToken);
        if (agent is null) return NotFound();

        var inbox = await messageRepository.GetInboxAsync(id, includeRead: true, cancellationToken);
        var outbox = await messageRepository.GetOutboxAsync(id, cancellationToken);
        var allAgents = await agentRepository.GetAllAsync(cancellationToken);
        var activities = await activityRepository.GetByAgentIdAsync(id, cancellationToken);

        var viewModel = new AgentDetailViewModel
        {
            Agent = new AgentSummaryViewModel
            {
                Id = agent.Id,
                Name = agent.Name,
                Description = agent.Description,
                Status = AgentStatusService.ComputeStatus(agent.LastCheckedInAt),
                LastCheckedInAt = agent.LastCheckedInAt,
                IsSystemAgent = agent.IsSystemAgent,
                AvatarSvg = agent.AvatarSvg,
                JobTitle = agent.JobTitle,
                CurrentTask = agent.CurrentTask
            },
            Inbox = inbox.Select(m => new MessageSummaryViewModel
            {
                Id = m.Id,
                SenderName = m.Sender?.Name ?? "Unknown",
                RecipientName = m.Recipient?.Name,
                Subject = m.Subject,
                IsBroadcast = m.IsBroadcast,
                IsRead = m.IsRead,
                SentAt = m.SentAt,
                AttachmentCount = m.Attachments.Count
            }).ToList(),
            Outbox = outbox.Select(m => new MessageSummaryViewModel
            {
                Id = m.Id,
                SenderName = agent.Name,
                RecipientName = m.Recipient?.Name,
                Subject = m.Subject,
                IsBroadcast = m.IsBroadcast,
                IsRead = m.IsRead,
                SentAt = m.SentAt,
                AttachmentCount = m.Attachments.Count
            }).ToList(),
            AllAgents = allAgents.Select(a => new AgentSummaryViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                Status = AgentStatusService.ComputeStatus(a.LastCheckedInAt),
                LastCheckedInAt = a.LastCheckedInAt,
                IsSystemAgent = a.IsSystemAgent,
                AvatarSvg = a.AvatarSvg,
                JobTitle = a.JobTitle,
                CurrentTask = a.CurrentTask
            }).ToList(),
            Activities = activities.Select(a => new AgentActivityViewModel
            {
                Id = a.Id,
                Description = a.Description,
                StartedAt = a.StartedAt,
                CompletedAt = a.CompletedAt,
                IsActive = a.IsActive
            }).ToList(),
            Skills = agent.Skills.Select(s => new AgentSkillDto(s.Name, s.Description)).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string name, string description, string? avatarSvg, string? jobTitle, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Agent name is required.";
            return RedirectToAction("Index", "Dashboard");
        }

        var existing = await agentRepository.GetByNameAsync(name, cancellationToken);
        if (existing is not null)
        {
            TempData["Error"] = $"An agent named '{name}' already exists.";
            return RedirectToAction("Index", "Dashboard");
        }

        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = AgentStatus.Offline,
            AvatarSvg = avatarSvg,
            JobTitle = jobTitle
        };

        await agentRepository.CreateAsync(agent, cancellationToken);
        TempData["Success"] = $"Agent '{name}' registered successfully.";
        return RedirectToAction("Detail", new { id = agent.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendConfigAddSkill(
        Guid id,
        string skillName,
        string skillMarkdown,
        CancellationToken cancellationToken)
    {
        var agent = await agentRepository.GetByIdAsync(id, cancellationToken);
        if (agent is null) return NotFound();

        var allAgents = await agentRepository.GetAllAsync(cancellationToken);
        var systemAgent = allAgents.FirstOrDefault(a => a.IsSystemAgent);
        if (systemAgent is null)
        {
            TempData["Error"] = "System agent not found. Cannot send config message.";
            return RedirectToAction("Detail", new { id });
        }

        var body = System.Text.Json.JsonSerializer.Serialize(new
        {
            skillName,
            skillMarkdown
        });

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = systemAgent.Id,
            RecipientId = id,
            Subject = "CONFIG:add-skill",
            Body = body,
            IsBroadcast = false,
            IsRead = false,
            SentAt = DateTimeOffset.UtcNow
        };

        await messageRepository.CreateAsync(message, cancellationToken);
        TempData["Success"] = $"Config message sent to add skill '{skillName}'.";
        return RedirectToAction("Detail", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendConfigRemoveSkill(
        Guid id,
        string skillName,
        CancellationToken cancellationToken)
    {
        var agent = await agentRepository.GetByIdAsync(id, cancellationToken);
        if (agent is null) return NotFound();

        var allAgents = await agentRepository.GetAllAsync(cancellationToken);
        var systemAgent = allAgents.FirstOrDefault(a => a.IsSystemAgent);
        if (systemAgent is null)
        {
            TempData["Error"] = "System agent not found. Cannot send config message.";
            return RedirectToAction("Detail", new { id });
        }

        var body = System.Text.Json.JsonSerializer.Serialize(new { skillName });

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = systemAgent.Id,
            RecipientId = id,
            Subject = "CONFIG:remove-skill",
            Body = body,
            IsBroadcast = false,
            IsRead = false,
            SentAt = DateTimeOffset.UtcNow
        };

        await messageRepository.CreateAsync(message, cancellationToken);
        TempData["Success"] = $"Config message sent to remove skill '{skillName}'.";
        return RedirectToAction("Detail", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var agent = await agentRepository.GetByIdAsync(id, cancellationToken);
        if (agent is null) return NotFound();

        if (agent.IsSystemAgent)
        {
            TempData["Error"] = "The System agent cannot be deleted.";
            return RedirectToAction("Detail", new { id });
        }

        await agentRepository.DeleteAsync(id, cancellationToken);
        TempData["Success"] = $"Agent '{agent.Name}' deleted.";
        return RedirectToAction("Index", "Dashboard");
    }
}
