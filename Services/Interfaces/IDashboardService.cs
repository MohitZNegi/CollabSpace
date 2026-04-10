using CollabSpace.Models.DTOs.Dashboard;

namespace CollabSpace.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetDashboardAsync(
            Guid workspaceId, Guid requestingUserId);
    }
}