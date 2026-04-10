using CollabSpace.Models.DTOs.Dashboard;

namespace CollabSpace.Services.Interfaces
{
    public interface IActivityService
    {
        // Record a new activity event. Called from other services.
        Task RecordAsync(
            Guid workspaceId, Guid actorId,
            string actionType, string description,
            Guid? entityId = null, string? entityType = null);

        // Retrieve recent activity for a workspace
        Task<List<ActivityResponseDto>> GetRecentActivityAsync(
            Guid workspaceId, Guid requestingUserId, int limit = 20);
    }
}