using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models;
using CollabSpace.Models.DTOs.Chat;
using CollabSpace.Services;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CollabSpace.Tests.Services
{
    public class ChatServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static IChatEventService CreateMockChatEvents()
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

        private static Mock<INotificationService> CreateMockNotifications()
        {
            var mock = new Mock<INotificationService>();
            mock.Setup(m => m.NotifyMentionsAsync(
                    It.IsAny<List<Guid>>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<Guid>(),
                    It.IsAny<string?>()))
                .Returns(Task.CompletedTask);
            return mock;
        }

        private async Task<(Guid userId, Guid workspaceId)>
            SeedMemberAsync(AppDbContext context, string username = "user")
        {
            var userId = Guid.NewGuid();
            var workspaceId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = userId,
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = "hash"
            });
            context.WorkspaceMembers.Add(new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WorkspaceId = workspaceId,
                WorkspaceRole = "Member"
            });
            await context.SaveChangesAsync();
            return (userId, workspaceId);
        }

        [Fact]
        public async Task SendWorkspaceMessageAsync_SavesAndReturnsMessage()
        {
            var context = CreateContext();
            var (userId, workspaceId) = await SeedMemberAsync(context);
            var notifications = CreateMockNotifications();
            var service = new ChatService(
                context,
                CreateMockChatEvents(),
                notifications.Object);

            var result = await service.SendWorkspaceMessageAsync(
                workspaceId,
                new SendMessageDto { Content = "Hello team!" },
                userId);

            Assert.Equal("Hello team!", result.Content);
            Assert.Equal(userId, result.SenderId);
            Assert.False(result.IsEdited);
        }

        [Fact]
        public async Task SendWorkspaceMessageAsync_NotifiesMentionedUsers()
        {
            var context = CreateContext();
            var notifications = CreateMockNotifications();
            var service = new ChatService(
                context,
                CreateMockChatEvents(),
                notifications.Object);

            var (senderId, workspaceId) = await SeedMemberAsync(context, "sender");
            var (mentionedUserId, mentionedWorkspaceId) = await SeedMemberAsync(context, "alice");

            var mentionedMembership = await context.WorkspaceMembers
                .FirstAsync(wm => wm.UserId == mentionedUserId
                               && wm.WorkspaceId == mentionedWorkspaceId);
            mentionedMembership.WorkspaceId = workspaceId;
            await context.SaveChangesAsync();

            var result = await service.SendWorkspaceMessageAsync(
                workspaceId,
                new SendMessageDto { Content = "Hello @alice" },
                senderId);

            notifications.Verify(m => m.NotifyMentionsAsync(
                It.Is<List<Guid>>(ids => ids.Count == 1 && ids.Contains(mentionedUserId)),
                "sender",
                "chat in workspace",
                result.Id,
                It.Is<string>(url => url.Contains($"/workspaces/{workspaceId}/boards/")
                    && url.Contains($"chatMessage={result.Id}"))),
                Times.Once);
        }

        [Fact]
        public async Task SendWorkspaceMessageAsync_Throws_WhenNotMember()
        {
            var context = CreateContext();
            var service = new ChatService(
                context,
                CreateMockChatEvents(),
                CreateMockNotifications().Object);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.SendWorkspaceMessageAsync(
                    Guid.NewGuid(),
                    new SendMessageDto { Content = "Hello!" },
                    Guid.NewGuid()));
        }

        [Fact]
        public async Task GetWorkspaceMessagesAsync_ReturnsOrderedByTime()
        {
            var context = CreateContext();
            var (userId, workspaceId) = await SeedMemberAsync(context);
            var service = new ChatService(
                context,
                CreateMockChatEvents(),
                CreateMockNotifications().Object);

            await service.SendWorkspaceMessageAsync(workspaceId,
                new SendMessageDto { Content = "First" }, userId);

            await Task.Delay(10); // ensure different timestamps

            await service.SendWorkspaceMessageAsync(workspaceId,
                new SendMessageDto { Content = "Second" }, userId);

            var messages = await service.GetWorkspaceMessagesAsync(
                workspaceId, userId);

            Assert.Equal(2, messages.Count);
            Assert.Equal("First", messages[0].Content);
            Assert.Equal("Second", messages[1].Content);
        }

        [Fact]
        public async Task SendDirectMessageAsync_Throws_WhenSendingToSelf()
        {
            var context = CreateContext();
            var (userId, _) = await SeedMemberAsync(context);
            var service = new ChatService(
                context,
                CreateMockChatEvents(),
                CreateMockNotifications().Object);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SendDirectMessageAsync(
                    userId,
                    new SendMessageDto { Content = "Hi me" },
                    userId));
        }

        [Fact]
        public async Task MarkDirectMessagesAsReadAsync_SetsIsReadToTrue()
        {
            var context = CreateContext();
            var (senderId, _) = await SeedMemberAsync(context, "sender");
            var (recipientId, _) = await SeedMemberAsync(context, "recipient");
            var service = new ChatService(
                context,
                CreateMockChatEvents(),
                CreateMockNotifications().Object);

            context.DirectMessages.Add(new DirectMessage
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                RecipientId = recipientId,
                Content = "Hey",
                SentAt = DateTime.UtcNow,
                IsRead = false
            });
            await context.SaveChangesAsync();

            await service.MarkDirectMessagesAsReadAsync(senderId, recipientId);

            var dm = await context.DirectMessages
                .FirstAsync(d => d.SenderId == senderId);
            Assert.True(dm.IsRead);
        }
    }
}
