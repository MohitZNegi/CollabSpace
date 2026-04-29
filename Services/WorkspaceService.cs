using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models;
using CollabSpace.Models.Constants;
using CollabSpace.Models.DTOs.WorkSpace;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Services
{
    public class WorkspaceService : BaseService, IWorkspaceService
    {
        private readonly AppDbContext _context;
        private readonly IActivityService _activity;

        public WorkspaceService(AppDbContext context,
            IActivityService activity)
        {
            _context = context;
            _activity = activity;
        }

        public async Task<WorkspaceResponseDto> CreateWorkspaceAsync(
            CreateWorkspaceDto request, Guid ownerId)
        {
            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                OwnerId = ownerId,
                // Generate a unique 8-character invite code.
                // We loop until we produce one that does not already
                // exist in the database. Collisions are astronomically
                // rare but we handle them correctly anyway.
                InviteCode = await GenerateUniqueInviteCodeAsync(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Workspaces.Add(workspace);

            // The creator automatically becomes the Owner member.
            // We create both the Workspace and the WorkspaceMember
            // row in the same transaction. If either fails, both
            // are rolled back. EF Core wraps SaveChangesAsync in a
            // transaction by default when you make multiple changes
            // before calling it.
            var membership = new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspace.Id,
                UserId = ownerId,
                WorkspaceRole = "Owner",
                JoinedAt = DateTime.UtcNow
            };

            _context.WorkspaceMembers.Add(membership);
            await _context.SaveChangesAsync();

            // Load the owner's username to include in the response.
            // We do a separate query here rather than including it
            // in a complex joined query above because the code is
            // cleaner and this endpoint is not performance-critical.
            var owner = await _context.Users
                .AsNoTracking()
                .FirstAsync(u => u.Id == ownerId);

            return MapToResponseDto(workspace, owner.Username);
        }

        public async Task<WorkspaceResponseDto> GetWorkspaceAsync(
            Guid workspaceId, Guid requestingUserId)
        {
            // Load the workspace and its owner together in one query.
            // .Include() tells EF Core to JOIN the Users table.
            var workspace = await _context.Workspaces
                .AsNoTracking()
                .Include(w => w.Owner)
                .FirstOrDefaultAsync(w => w.Id == workspaceId
                                       && !w.IsArchived);

            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            // Check membership. Throws ForbiddenException if not a member.
            var isMember = await _context.WorkspaceMembers
                .AsNoTracking()
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == requestingUserId);

            if (!isMember)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");

            return MapToResponseDto(workspace, workspace.Owner!.Username);
        }

        public async Task<List<WorkspaceResponseDto>> GetMyWorkspacesAsync(
            Guid userId)
        {
            // Load all workspaces this user belongs to via the join table.
            // ThenInclude lets you chain includes: WorkspaceMembers
            // -> Workspace -> Owner (two levels deep).
            return await _context.WorkspaceMembers
                .AsNoTracking()
                .Where(wm => wm.UserId == userId)
                .Include(wm => wm.Workspace)
                    .ThenInclude(w => w!.Owner)
                .Where(wm => !wm.Workspace!.IsArchived)
                .Select(wm => MapToResponseDto(
                    wm.Workspace!, wm.Workspace!.Owner!.Username))
                .ToListAsync();
        }

        public async Task<WorkspaceResponseDto> JoinByCodeAsync(
            string inviteCode, Guid userId)
        {
            var workspace = await _context.Workspaces
                .AsNoTracking()
                .Include(w => w.Owner)
                .FirstOrDefaultAsync(w => w.InviteCode == inviteCode.ToUpper()
                                       && !w.IsArchived);

            if (workspace == null)
                throw new KeyNotFoundException(
                    "Invalid invite code. No workspace found.");

            // Prevent duplicate membership. If the user is already
            // a member (perhaps they followed the link twice), we
            // just return the workspace without throwing an error.
            // This is called an idempotent operation: calling it
            // multiple times produces the same result as calling it once.
            var alreadyMember = await _context.WorkspaceMembers
                .AsNoTracking()
                .AnyAsync(wm => wm.WorkspaceId == workspace.Id
                             && wm.UserId == userId);

            if (alreadyMember)
                return MapToResponseDto(workspace, workspace.Owner!.Username);

            var membership = new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspace.Id,
                UserId = userId,
                WorkspaceRole = "Member",
                JoinedAt = DateTime.UtcNow
            };

            _context.WorkspaceMembers.Add(membership);
            await _context.SaveChangesAsync();

            var username = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();

            await _activity.RecordAsync(
                workspace.Id, userId,
                ActivityTypes.MemberJoined,
                $"{username} joined {workspace.Name}",
                workspace.Id, "Workspace");

            return MapToResponseDto(workspace, workspace.Owner!.Username);
        }

        public async Task<List<WorkspaceMemberDto>> GetMembersAsync(
            Guid workspaceId, Guid requestingUserId)
        {
            var isMember = await _context.WorkspaceMembers
                .AsNoTracking()
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == requestingUserId);

            if (!isMember)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");

            // Load all members and their user data in one query.
            // We project directly into the DTO using Select to avoid
            // loading unnecessary columns from the database.
            return await _context.WorkspaceMembers
                .AsNoTracking()
                .Where(wm => wm.WorkspaceId == workspaceId)
                .Include(wm => wm.User)
                .OrderBy(wm => wm.WorkspaceRole)
                .Select(wm => new WorkspaceMemberDto
                {
                    UserId = wm.UserId,
                    Username = wm.User!.Username,
                    AvatarUrl = wm.User.AvatarUrl,
                    WorkspaceRole = wm.WorkspaceRole,
                    JoinedAt = wm.JoinedAt,
                    LastSeenAt = wm.User.LastSeenAt
                })
                .ToListAsync();
        }

        public async Task AssignRoleAsync(
            Guid workspaceId, Guid targetUserId,
            string newRole, Guid requestingUserId)
        {
            // Only valid roles can be assigned.
            // We validate this in the service rather than only in the
            // DTO validator because it is a business rule, not just
            // an input format rule.
            var validRoles = new[] { "Lead", "Member" };
            if (!validRoles.Contains(newRole))
                throw new ArgumentException(
                    "Role must be either 'Lead' or 'Member'.");
            // Note: Owner cannot be assigned. The Owner role is only
            // set when the workspace is created. Ownership transfer
            // is a separate, more complex operation not in this MVP.

            // Load both memberships together to check permissions
            // and apply the change efficiently.
            var memberships = await _context.WorkspaceMembers
                .Where(wm => wm.WorkspaceId == workspaceId
                          && (wm.UserId == requestingUserId
                           || wm.UserId == targetUserId))
                .ToListAsync();

            var requester = memberships
                .FirstOrDefault(wm => wm.UserId == requestingUserId);
            var target = memberships
                .FirstOrDefault(wm => wm.UserId == targetUserId);

            if (requester == null)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");

            // Only the Owner can assign roles.
            if (requester.WorkspaceRole != "Owner")
                throw new ForbiddenException(
                    "Only the workspace Owner can assign roles.");

            if (target == null)
                throw new KeyNotFoundException(
                    "Target user is not a member of this workspace.");

            // Prevent modifying the Owner's own role.
            if (target.WorkspaceRole == "Owner")
                throw new InvalidOperationException(
                    "The Owner's role cannot be changed.");

            target.WorkspaceRole = newRole;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(
            Guid workspaceId, Guid targetUserId,
            Guid requestingUserId)
        {
            var memberships = await _context.WorkspaceMembers
                .Where(wm => wm.WorkspaceId == workspaceId
                          && (wm.UserId == requestingUserId
                           || wm.UserId == targetUserId))
                .ToListAsync();

            var requester = memberships
                .FirstOrDefault(wm => wm.UserId == requestingUserId);
            var target = memberships
                .FirstOrDefault(wm => wm.UserId == targetUserId);

            if (requester == null)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");

            // Only Owners and Leads can remove members.
            if (requester.WorkspaceRole == "Member")
                throw new ForbiddenException(
                    "You do not have permission to remove members.");

            if (target == null)
                throw new KeyNotFoundException(
                    "Target user is not a member of this workspace.");

            // Nobody can remove the Owner.
            if (target.WorkspaceRole == "Owner")
                throw new InvalidOperationException(
                    "The workspace Owner cannot be removed.");

            // Leads cannot remove other Leads, only Members.
            if (requester.WorkspaceRole == "Lead"
             && target.WorkspaceRole == "Lead")
                throw new ForbiddenException(
                    "Leads cannot remove other Leads.");

            _context.WorkspaceMembers.Remove(target);
            await _context.SaveChangesAsync();
        }

        // ---------------------------------------------------------------
        // PRIVATE HELPERS
        // ---------------------------------------------------------------

        // Generates a random uppercase 8-character invite code.
        // Uses cryptographically random bytes for unpredictability.
        private async Task<string> GenerateUniqueInviteCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string code;

            do
            {
                var bytes = System.Security.Cryptography.RandomNumberGenerator
                    .GetBytes(8);
                code = new string(bytes.Select(b => chars[b % chars.Length])
                    .ToArray());
            }
            while (await _context.Workspaces
                .AsNoTracking()
                .AnyAsync(w => w.InviteCode == code));

            return code;
        }

        // A static mapper method keeps the mapping logic in one place.
        // If you add a new field to the DTO, you only change this method.
        // This is simpler than AutoMapper for a focused DTO like this.
        private static WorkspaceResponseDto MapToResponseDto(
            Workspace workspace, string ownerUsername)
        {
            return new WorkspaceResponseDto
            {
                Id = workspace.Id,
                Name = workspace.Name,
                Description = workspace.Description,
                InviteCode = workspace.InviteCode,
                OwnerUsername = ownerUsername,
                CreatedAt = workspace.CreatedAt
            };
        }
    }
}
