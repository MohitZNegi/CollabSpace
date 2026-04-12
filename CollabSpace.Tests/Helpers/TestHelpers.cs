using CollabSpace.Data;
using CollabSpace.Models;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CollabSpace.Tests.Helpers
{
    // Central place for shared test infrastructure.
    // Every test class uses these helpers to avoid duplication.
    public static class TestHelpers
    {
        // Creates a fresh isolated in-memory database for each test.
        // Guid.NewGuid() ensures no two tests share the same database
        // even if they run in parallel.
        public static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // Seeds a complete user into the database and returns their ID.
        public static async Task<Guid> SeedUserAsync(
            AppDbContext context,
            string username = "testuser",
            string role = "Member")
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = "hashedpassword",
                GlobalRole = role
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user.Id;
        }

        // Seeds a workspace with an owner membership.
        public static async Task<(Guid workspaceId, Guid inviteCode)>
            SeedWorkspaceAsync(
                AppDbContext context,
                Guid ownerId,
                string name = "Test Workspace")
        {
            var workspaceId = Guid.NewGuid();
            var inviteCode = "TESTCODE";

            context.Workspaces.Add(new Workspace
            {
                Id = workspaceId,
                Name = name,
                OwnerId = ownerId,
                InviteCode = inviteCode,
                CreatedAt = DateTime.UtcNow
            });
            context.WorkspaceMembers.Add(new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                UserId = ownerId,
                WorkspaceRole = "Owner"
            });
            await context.SaveChangesAsync();
            return (workspaceId, Guid.Empty);
        }

        // Seeds a board belonging to a workspace.
        public static async Task<Guid> SeedBoardAsync(
            AppDbContext context,
            Guid workspaceId,
            Guid createdByUserId,
            string name = "Test Board")
        {
            var boardId = Guid.NewGuid();
            context.Boards.Add(new Board
            {
                Id = boardId,
                WorkspaceId = workspaceId,
                Name = name,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            return boardId;
        }

        // Seeds a card on a board.
        public static async Task<Guid> SeedCardAsync(
            AppDbContext context,
            Guid boardId,
            Guid createdByUserId,
            string title = "Test Card",
            string status = "Todo",
            int position = 0)
        {
            var cardId = Guid.NewGuid();
            context.Cards.Add(new Card
            {
                Id = cardId,
                BoardId = boardId,
                Title = title,
                Status = status,
                Position = position,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            return cardId;
        }

        // Creates a mock IBoardEventService that succeeds silently.
        public static IBoardEventService MockBoardEvents()
        {
            var mock = new Mock<IBoardEventService>();
            mock.Setup(m => m.BroadcastCardCreatedAsync(
                    It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.BroadcastCardUpdatedAsync(
                    It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.BroadcastCardMovedAsync(
                    It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.BroadcastCardDeletedAsync(
                    It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            return mock.Object;
        }

        // Creates a mock INotificationService that succeeds silently.
        public static INotificationService MockNotifications()
        {
            var mock = new Mock<INotificationService>();
            mock.Setup(m => m.NotifyCardUpdatedAsync(
                    It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.NotifyCardAssignedAsync(
                    It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.NotifyCommentAddedAsync(
                    It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<Guid>(),
                    It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.NotifyMentionsAsync(
                    It.IsAny<List<Guid>>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.NotifyMemberJoinedAsync(
                    It.IsAny<List<Guid>>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.NotifyMemberRemovedAsync(
                    It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            return mock.Object;
        }

        // Creates a mock IActivityService that succeeds silently.
        public static IActivityService MockActivity()
        {
            var mock = new Mock<IActivityService>();
            mock.Setup(m => m.RecordAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(),
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Guid?>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);
            return mock.Object;
        }

        // Creates a mock IChatEventService that succeeds silently.
        public static IChatEventService MockChatEvents()
        {
            var mock = new Mock<IChatEventService>();
            mock.Setup(m => m.BroadcastWorkspaceMessageAsync(
                    It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.SendDirectMessageAsync(
                    It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);
            return mock.Object;
        }
    }
}