using CollabSpace.Models.DTOs.WorkSpace;

namespace CollabSpace.Services.Interfaces
{
    public interface IWorkspaceService
    {
        Task<WorkspaceResponseDto> CreateWorkspaceAsync(
            CreateWorkspaceDto request, Guid ownerId);

        Task<WorkspaceResponseDto> GetWorkspaceAsync(
            Guid workspaceId, Guid requestingUserId);

        Task<List<WorkspaceResponseDto>> GetMyWorkspacesAsync(Guid userId);

        Task<WorkspaceResponseDto> JoinByCodeAsync(
            string inviteCode, Guid userId);

        Task<List<WorkspaceMemberDto>> GetMembersAsync(
            Guid workspaceId, Guid requestingUserId);

        Task AssignRoleAsync(
            Guid workspaceId, Guid targetUserId,
            string newRole, Guid requestingUserId);

        Task RemoveMemberAsync(
            Guid workspaceId, Guid targetUserId,
            Guid requestingUserId);
    }
}
