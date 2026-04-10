using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models.DTOs.Dashboard;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IActivityService _activityService;

        public DashboardService(
            AppDbContext context,
            IActivityService activityService)
        {
            _context = context;
            _activityService = activityService;
        }

        public async Task<DashboardResponseDto> GetDashboardAsync(
            Guid workspaceId, Guid requestingUserId)
        {
            var isMember = await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == requestingUserId);

            if (!isMember)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");

            // Load all three datasets in parallel for performance
            var activityTask = _activityService.GetRecentActivityAsync(
                workspaceId, requestingUserId, 20);

            var taskStatsTask = GetTaskStatsAsync(workspaceId);

            var activeMembersTask = GetActiveMembersAsync(workspaceId);

            await Task.WhenAll(activityTask, taskStatsTask,
                activeMembersTask);

            return new DashboardResponseDto
            {
                RecentActivity = await activityTask,
                TaskStats = await taskStatsTask,
                ActiveMembers = await activeMembersTask
            };
        }

        private async Task<TaskStatsDto> GetTaskStatsAsync(
            Guid workspaceId)
        {
            // Count cards across all boards in this workspace
            // grouped by status in a single query
            var stats = await _context.Cards
                .Where(c => c.Board!.WorkspaceId == workspaceId)
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var todo = stats.FirstOrDefault(
                s => s.Status == "Todo")?.Count ?? 0;
            var inProgress = stats.FirstOrDefault(
                s => s.Status == "InProgress")?.Count ?? 0;
            var done = stats.FirstOrDefault(
                s => s.Status == "Done")?.Count ?? 0;

            return new TaskStatsDto
            {
                Todo = todo,
                InProgress = inProgress,
                Done = done,
                Total = todo + inProgress + done
            };
        }

        private async Task<List<ActiveMemberDto>> GetActiveMembersAsync(
            Guid workspaceId)
        {
            // Active = seen in the last 15 minutes
            var cutoff = DateTime.UtcNow.AddMinutes(-15);

            return await _context.WorkspaceMembers
                .Where(wm => wm.WorkspaceId == workspaceId)
                .Include(wm => wm.User)
                .OrderByDescending(wm => wm.User!.LastSeenAt)
                .Take(10)
                .Select(wm => new ActiveMemberDto
                {
                    UserId = wm.UserId,
                    Username = wm.User!.Username,
                    AvatarUrl = wm.User.AvatarUrl,
                    LastSeenAt = wm.User.LastSeenAt
                })
                .ToListAsync();
        }
    }
}