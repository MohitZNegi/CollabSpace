using CollabSpace.Data;
using CollabSpace.Models;
using CollabSpace.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CollabSpace.Tests.Services
{
    public class WorkspaceAuthServiceTests
    {
        // Creates a fresh in-memory database for each test.
        // Tests never touch your real SQL Server database.
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task IsMemberAsync_ReturnsFalse_WhenUserNotInWorkspace()
        {
            var context = CreateContext();
            var service = new WorkspaceAuthService(context);

            var result = await service.IsMemberAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task IsMemberAsync_ReturnsTrue_WhenUserIsMember()
        {
            var context = CreateContext();

            var userId = Guid.NewGuid();
            var workspaceId = Guid.NewGuid();

            context.WorkspaceMembers.Add(new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WorkspaceId = workspaceId,
                WorkspaceRole = "Member"
            });
            await context.SaveChangesAsync();

            var service = new WorkspaceAuthService(context);
            var result = await service.IsMemberAsync(workspaceId, userId);

            Assert.True(result);
        }

        [Fact]
        public async Task RequireMemberAsync_Throws_WhenUserIsNotMember()
        {
            var context = CreateContext();
            var service = new WorkspaceAuthService(context);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.RequireMemberAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task IsLeadOrAboveAsync_ReturnsFalse_ForRegularMember()
        {
            var context = CreateContext();

            var userId = Guid.NewGuid();
            var workspaceId = Guid.NewGuid();

            context.WorkspaceMembers.Add(new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WorkspaceId = workspaceId,
                WorkspaceRole = "Member"
            });
            await context.SaveChangesAsync();

            var service = new WorkspaceAuthService(context);
            var result = await service.IsLeadOrAboveAsync(workspaceId, userId);

            Assert.False(result);
        }
    }
}