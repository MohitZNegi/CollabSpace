using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models;
using CollabSpace.Models.DTOs.Comment;
using CollabSpace.Services;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CollabSpace.Tests.Services
{
    public class CommentServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static INotificationService CreateMockNotifications()
        {
            var mock = new Mock<INotificationService>();
            mock.Setup(m => m.NotifyCommentAddedAsync(
                    It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<Guid>(),
                    It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.NotifyMentionsAsync(
                    It.IsAny<List<Guid>>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            return mock.Object;
        }

        // Seeds a full hierarchy: user, workspace, board, card
        private async Task<(Guid userId, Guid cardId, Guid workspaceId)>
            SeedCardAsync(AppDbContext context, string username = "author")
        {
            var userId = Guid.NewGuid();
            var workspaceId = Guid.NewGuid();
            var boardId = Guid.NewGuid();
            var cardId = Guid.NewGuid();

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
            context.Boards.Add(new Board
            {
                Id = boardId,
                WorkspaceId = workspaceId,
                Name = "Board",
                CreatedByUserId = userId
            });
            context.Cards.Add(new Card
            {
                Id = cardId,
                BoardId = boardId,
                Title = "Test card",
                Status = "Todo",
                Position = 0,
                CreatedByUserId = userId
            });

            await context.SaveChangesAsync();
            return (userId, cardId, workspaceId);
        }

        [Fact]
        public async Task CreateCommentAsync_CreatesTopLevelComment()
        {
            var context = CreateContext();
            var (userId, cardId, _) = await SeedCardAsync(context);
            var service = new CommentService(context,
                CreateMockNotifications());

            var result = await service.CreateCommentAsync(
                cardId,
                new CreateCommentDto { Content = "Looks good!" },
                userId);

            Assert.Equal("Looks good!", result.Content);
            Assert.Null(result.ParentCommentId);
            Assert.False(result.IsEdited);
        }

        [Fact]
        public async Task CreateCommentAsync_CreatesReply_UnderParent()
        {
            var context = CreateContext();
            var (userId, cardId, _) = await SeedCardAsync(context);
            var service = new CommentService(context,
                CreateMockNotifications());

            var parent = await service.CreateCommentAsync(
                cardId,
                new CreateCommentDto { Content = "Parent comment" },
                userId);

            var reply = await service.CreateCommentAsync(
                cardId,
                new CreateCommentDto
                {
                    Content = "Reply to parent",
                    ParentCommentId = parent.Id
                },
                userId);

            Assert.Equal(parent.Id, reply.ParentCommentId);
        }

        [Fact]
        public async Task GetCommentsAsync_ReturnsNestedTree()
        {
            var context = CreateContext();
            var (userId, cardId, _) = await SeedCardAsync(context);
            var service = new CommentService(context,
                CreateMockNotifications());

            var parent = await service.CreateCommentAsync(cardId,
                new CreateCommentDto { Content = "Parent" }, userId);

            await service.CreateCommentAsync(cardId,
                new CreateCommentDto
                {
                    Content = "Reply",
                    ParentCommentId = parent.Id
                }, userId);

            var comments = await service.GetCommentsAsync(cardId, userId);

            // Only one top-level comment
            Assert.Single(comments);
            // That comment has one reply nested inside it
            Assert.Single(comments[0].Replies);
            Assert.Equal("Reply", comments[0].Replies[0].Content);
        }

        [Fact]
        public async Task EditCommentAsync_Throws_WhenNotAuthor()
        {
            var context = CreateContext();
            var (authorId, cardId, workspaceId) =
                await SeedCardAsync(context, "author");

            // Add a second user
            var otherId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = otherId,
                Username = "other",
                Email = "other@test.com",
                PasswordHash = "hash"
            });
            context.WorkspaceMembers.Add(new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                UserId = otherId,
                WorkspaceId = workspaceId,
                WorkspaceRole = "Member"
            });
            await context.SaveChangesAsync();

            var service = new CommentService(context,
                CreateMockNotifications());

            var comment = await service.CreateCommentAsync(cardId,
                new CreateCommentDto { Content = "My comment" }, authorId);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.EditCommentAsync(
                    comment.Id,
                    new EditCommentDto { Content = "Hacked" },
                    otherId));
        }

        [Fact]
        public async Task DeleteCommentAsync_DeletesRepliesFirst()
        {
            var context = CreateContext();
            var (userId, cardId, _) = await SeedCardAsync(context);
            var service = new CommentService(context,
                CreateMockNotifications());

            var parent = await service.CreateCommentAsync(cardId,
                new CreateCommentDto { Content = "Parent" }, userId);

            await service.CreateCommentAsync(cardId,
                new CreateCommentDto
                {
                    Content = "Reply",
                    ParentCommentId = parent.Id
                }, userId);

            // Should not throw even though replies exist
            await service.DeleteCommentAsync(parent.Id, userId);

            var remaining = await context.Comments
                .Where(c => c.CardId == cardId)
                .CountAsync();

            // Both parent and reply are gone
            Assert.Equal(0, remaining);
        }
    }
}