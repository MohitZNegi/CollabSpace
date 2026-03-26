using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Services
{
    public class WorkspaceAuthService : IWorkspaceAuthService
    {
        private readonly AppDbContext _context;

        public WorkspaceAuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsMemberAsync(Guid workspaceId, Guid userId)
        {
            return await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == userId);
        }

        public async Task<bool> IsLeadOrAboveAsync(Guid workspaceId, Guid userId)
        {
            return await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == userId
                             && (wm.WorkspaceRole == "Lead"
                              || wm.WorkspaceRole == "Owner"));
        }

        public async Task<bool> IsOwnerAsync(Guid workspaceId, Guid userId)
        {
            return await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == userId
                             && wm.WorkspaceRole == "Owner");
        }

        // Throws a meaningful exception if the check fails.
        // Controllers catch these and return the correct HTTP status code.
        public async Task RequireMemberAsync(Guid workspaceId, Guid userId)
        {
            if (!await IsMemberAsync(workspaceId, userId))
                throw new ForbiddenException(
                    "You are not a member of this workspace.");
        }

        public async Task RequireLeadOrAboveAsync(Guid workspaceId, Guid userId)
        {
            if (!await IsLeadOrAboveAsync(workspaceId, userId))
                throw new ForbiddenException(
                    "This action requires Lead or Owner role in this workspace.");
        }
    }
}