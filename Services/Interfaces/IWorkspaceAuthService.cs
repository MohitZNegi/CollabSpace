namespace CollabSpace.Services.Interfaces
{
    public interface IWorkspaceAuthService
    {
        // Is the user any kind of member of this workspace?
        Task<bool> IsMemberAsync(Guid workspaceId, Guid userId);

        // Is the user a Lead or Owner in this workspace?
        Task<bool> IsLeadOrAboveAsync(Guid workspaceId, Guid userId);

        // Is the user the Owner of this workspace?
        Task<bool> IsOwnerAsync(Guid workspaceId, Guid userId);

        // Throws if the user is not a member. Use at the start of
        // any workspace-scoped action to gate access cleanly.
        Task RequireMemberAsync(Guid workspaceId, Guid userId);
        Task RequireLeadOrAboveAsync(Guid workspaceId, Guid userId);
    }
}
