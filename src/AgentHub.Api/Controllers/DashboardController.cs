using AgentHub.Api.ViewModels;
using AgentHub.Core.Interfaces;
using AgentHub.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentHub.Api.Controllers;

public class DashboardController(IAgentRepository agentRepository) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var agents = await agentRepository.GetAllAsync(cancellationToken);

        var viewModel = new DashboardViewModel
        {
            Agents = agents.Select(a => new AgentSummaryViewModel
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
            }).ToList()
        };

        return View(viewModel);
    }
}
