using CollabSpace.Models;
using CollabSpace.Services;
using CollabSpace.Tests.Helpers;

namespace CollabSpace.Tests.Services
{
    public class DashboardServiceTests
    {
        [Fact]
        public async Task GetDashboardAsync_ReturnsCorrectTaskStats()
        {
            var context = TestHelpers.CreateContext();
            var ownerId = await TestHelpers.SeedUserAsync(context);
            var (wsId, _) = await TestHelpers.SeedWorkspaceAsync(
                context, ownerId);
            var boardId = await TestHelpers.SeedBoardAsync(
                context, wsId, ownerId);

            // Seed cards with different statuses
            await TestHelpers.SeedCardAsync(
                context, boardId, ownerId, "Card 1", "Todo");
            await TestHelpers.SeedCardAsync(
                context, boardId, ownerId, "Card 2", "Todo");
            await TestHelpers.SeedCardAsync(
                context, boardId, ownerId, "Card 3", "InProgress");
            await TestHelpers.SeedCardAsync(
                context, boardId, ownerId, "Card 4", "Done");

            var activityService = new ActivityService(context);
            var service = new DashboardService(context, activityService);

            var result = await service.GetDashboardAsync(wsId, ownerId);

            Assert.Equal(2, result.TaskStats.Todo);
            Assert.Equal(1, result.TaskStats.InProgress);
            Assert.Equal(1, result.TaskStats.Done);
            Assert.Equal(4, result.TaskStats.Total);
        }

        [Fact]
        public async Task GetDashboardAsync_ReturnsRecentActivity()
        {
            var context = TestHelpers.CreateContext();
            var ownerId = await TestHelpers.SeedUserAsync(context);
            var (wsId, _) = await TestHelpers.SeedWorkspaceAsync(
                context, ownerId);

            // Seed some activity directly
            context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                WorkspaceId = wsId,
                ActorId = ownerId,
                ActionType = "CardCreated",
                Description = "Test created a card",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var activityService = new ActivityService(context);
            var service = new DashboardService(context, activityService);

            var result = await service.GetDashboardAsync(wsId, ownerId);

            Assert.Single(result.RecentActivity);
            Assert.Equal("CardCreated",
                result.RecentActivity[0].ActionType);
        }

        [Fact]
        public async Task GetDashboardAsync_Throws_WhenNotMember()
        {
            var context = TestHelpers.CreateContext();
            var ownerId = await TestHelpers.SeedUserAsync(context, "owner");
            var otherId = await TestHelpers.SeedUserAsync(context, "other");
            var (wsId, _) = await TestHelpers.SeedWorkspaceAsync(
                context, ownerId);

            var activityService = new ActivityService(context);
            var service = new DashboardService(context, activityService);

            await Assert.ThrowsAsync
                <CollabSpace.Exceptions.ForbiddenException>(() =>
                    service.GetDashboardAsync(wsId, otherId));
        }
    }
}