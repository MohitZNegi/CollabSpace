using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models;
using CollabSpace.Models.Constants;
using CollabSpace.Models.DTOs.Dashboard;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Services
{
    public class ActivityService : IActivityService
    {
        private readonly AppDbContext _context;

        public ActivityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task RecordAsync(
            Guid workspaceId, Guid actorId,
            string actionType, string description,
            Guid? entityId = null, string? entityType = null)
        {
            var activity = new ActivityLog
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                ActorId = actorId,
                ActionType = actionType,
                Description = description,
                EntityId = entityId,
                EntityType = entityType,
                CreatedAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ActivityResponseDto>> GetRecentActivityAsync(
         Guid workspaceId, Guid requestingUserId, int limit = 10)
            {
                var isMember = await _context.WorkspaceMembers
                    .AnyAsync(wm => wm.WorkspaceId == workspaceId
                                 && wm.UserId == requestingUserId);

                if (!isMember)
                    throw new ForbiddenException(
                        "You are not a member of this workspace.");

                // Only show meaningful high-level actions.
                // Filter out noisy events like every single card move.
                var meaningfulActions = new[]
                {
            ActivityTypes.CardCreated,
            ActivityTypes.BoardCreated,
            ActivityTypes.MemberJoined,
            ActivityTypes.MemberRemoved,
            ActivityTypes.CommentAdded,
        };

                return await _context.ActivityLogs
                    .AsNoTracking()
                    .Where(a => a.WorkspaceId == workspaceId
                             && meaningfulActions.Contains(a.ActionType))
                    .Include(a => a.Actor)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(limit)
                    .Select(a => new ActivityResponseDto
                    {
                        Id = a.Id,
                        ActorUsername = a.Actor!.Username,
                        ActorAvatarUrl = a.Actor.AvatarUrl,
                        ActionType = a.ActionType,
                        Description = a.Description,
                        EntityId = a.EntityId,
                        EntityType = a.EntityType,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();
        }
    }
}
