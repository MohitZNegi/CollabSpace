using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models;
using CollabSpace.Models.DTOs.Card;
using CollabSpace.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CollabSpace.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace CollabSpace.Tests.Services
{
    public class CardServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // Mock IBoardEventService
        private static IBoardEventService CreateMockBoardEvents()
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

        private async Task<(Guid userId, Guid workspaceId, Guid boardId)>
            SeedBoardAsync(AppDbContext context)
        {
            var userId = Guid.NewGuid();
            var workspaceId = Guid.NewGuid();
            var boardId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@test.com",
                PasswordHash = "hash"
            });

            context.WorkspaceMembers.Add(new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WorkspaceId = workspaceId,
                WorkspaceRole = "Owner"
            });

            context.Boards.Add(new Board
            {
                Id = boardId,
                WorkspaceId = workspaceId,
                Name = "Test Board",
                CreatedByUserId = userId
            });

            await context.SaveChangesAsync();
            return (userId, workspaceId, boardId);
        }

        [Fact]
        public async Task CreateCardAsync_SetsPositionToZero_WhenFirstCard()
        {
            var context = CreateContext();
            var (userId, _, boardId) = await SeedBoardAsync(context);
            var service = new CardService(context, CreateMockBoardEvents());

            var result = await service.CreateCardAsync(
                boardId,
                new CreateCardDto { Title = "First card" },
                userId);

            Assert.Equal(0, result.Position);
            Assert.Equal("Todo", result.Status);
        }

        [Fact]
        public async Task CreateCardAsync_IncrementsPosition_ForEachNewCard()
        {
            var context = CreateContext();
            var (userId, _, boardId) = await SeedBoardAsync(context);
            var service = new CardService(context, CreateMockBoardEvents());

            await service.CreateCardAsync(boardId,
                new CreateCardDto { Title = "Card 1" }, userId);

            await service.CreateCardAsync(boardId,
                new CreateCardDto { Title = "Card 2" }, userId);

            var third = await service.CreateCardAsync(boardId,
                new CreateCardDto { Title = "Card 3" }, userId);

            Assert.Equal(2, third.Position);
        }

        [Fact]
        public async Task MoveCardAsync_UpdatesStatusAndPosition()
        {
            var context = CreateContext();
            var (userId, _, boardId) = await SeedBoardAsync(context);
            var service = new CardService(context, CreateMockBoardEvents());

            var card = await service.CreateCardAsync(boardId,
                new CreateCardDto { Title = "Task" }, userId);

            await service.MoveCardAsync(
                card.Id,
                new MoveCardDto { Status = "InProgress", Position = 0 },
                userId);

            var updated = await context.Cards.FindAsync(card.Id);

            Assert.Equal("InProgress", updated!.Status);
            Assert.Equal(0, updated.Position);
        }

        [Fact]
        public async Task DeleteCardAsync_Throws_WhenUserIsMember()
        {
            var context = CreateContext();
            var (ownerId, workspaceId, boardId) = await SeedBoardAsync(context);

            var memberId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = memberId,
                Username = "member",
                Email = "member@test.com",
                PasswordHash = "hash"
            });

            context.WorkspaceMembers.Add(new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                UserId = memberId,
                WorkspaceId = workspaceId,
                WorkspaceRole = "Member"
            });

            await context.SaveChangesAsync();

            var service = new CardService(context, CreateMockBoardEvents());

            var card = await service.CreateCardAsync(boardId,
                new CreateCardDto { Title = "Task" }, ownerId);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.DeleteCardAsync(card.Id, memberId));
        }
    }
}