using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Route("api/v1/workspaces/{workspaceId:guid}/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUser;

        public DashboardController(
            IDashboardService dashboardService,
            ICurrentUserService currentUser)
        {
            _dashboardService = dashboardService;
            _currentUser = currentUser;
        }

        // GET /api/v1/workspaces/{workspaceId}/dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboard(Guid workspaceId)
        {
            var result = await _dashboardService.GetDashboardAsync(
                workspaceId, _currentUser.GetUserId());
            return Ok(result);
        }
    }
}