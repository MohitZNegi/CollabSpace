using CollabSpace.Data;
using CollabSpace.Models;
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
            // Fire and forget pattern: we do not await this in
            // the calling service so it does not slow down the
            // primary operation. Activity logging is important
            // but not critical to the user's immediate action.
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
            Guid workspaceId, Guid requestingUserId, int limit = 20)
        {
            // Verify membership before showing activity
            var isMember = await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == requestingUserId);

            if (!isMember)
                throw new Exceptions.ForbiddenException(
                    "You are not a member of this workspace.");

            return await _context.ActivityLogs
                .Where(a => a.WorkspaceId == workspaceId)
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