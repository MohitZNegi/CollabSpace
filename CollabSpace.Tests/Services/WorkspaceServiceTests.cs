using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models;
using CollabSpace.Models.DTOs.WorkSpace;
using CollabSpace.Services;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Tests.Services
{
    public class WorkspaceServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // Seeds a user into the in-memory database and returns their Id.
        private async Task<Guid> SeedUserAsync(AppDbContext context,
            string username = "testuser")
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = "hash",
                GlobalRole = "Member"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user.Id;
        }

        [Fact]
        public async Task CreateWorkspaceAsync_CreatesWorkspaceAndOwnerMembership()
        {
            var context = CreateContext();
            var ownerId = await SeedUserAsync(context);
            var service = new WorkspaceService(context);

            var result = await service.CreateWorkspaceAsync(
                new CreateWorkspaceDto { Name = "My Team" }, ownerId);

            // Workspace was created
            Assert.Equal("My Team", result.Name);
            Assert.NotNull(result.InviteCode);
            Assert.Equal(8, result.InviteCode.Length);

            // Owner membership was created automatically
            var membership = await context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.UserId == ownerId);
            Assert.NotNull(membership);
            Assert.Equal("Owner", membership.WorkspaceRole);
        }

        [Fact]
        public async Task JoinByCodeAsync_AddsMemberWithMemberRole()
        {
            var context = CreateContext();
            var ownerId = await SeedUserAsync(context, "owner");
            var joinerId = await SeedUserAsync(context, "joiner");
            var service = new WorkspaceService(context);

            var workspace = await service.CreateWorkspaceAsync(
                new CreateWorkspaceDto { Name = "Team" }, ownerId);

            await service.JoinByCodeAsync(workspace.InviteCode, joinerId);

            var membership = await context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.UserId == joinerId);
            Assert.NotNull(membership);
            Assert.Equal("Member", membership.WorkspaceRole);
        }

        [Fact]
        public async Task JoinByCodeAsync_IsIdempotent_WhenAlreadyMember()
        {
            var context = CreateContext();
            var ownerId = await SeedUserAsync(context, "owner");
            var service = new WorkspaceService(context);

            var workspace = await service.CreateWorkspaceAsync(
                new CreateWorkspaceDto { Name = "Team" }, ownerId);

            // Join twice — should not throw, should not create duplicate
            await service.JoinByCodeAsync(workspace.InviteCode, ownerId);
            await service.JoinByCodeAsync(workspace.InviteCode, ownerId);

            var count = await context.WorkspaceMembers
                .CountAsync(wm => wm.UserId == ownerId);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task JoinByCodeAsync_Throws_WhenCodeIsInvalid()
        {
            var context = CreateContext();
            var userId = await SeedUserAsync(context);
            var service = new WorkspaceService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.JoinByCodeAsync("BADCODE", userId));
        }

        [Fact]
        public async Task RemoveMemberAsync_Throws_WhenTargetIsOwner()
        {
            var context = CreateContext();
            var ownerId = await SeedUserAsync(context, "owner");
            var leadId = await SeedUserAsync(context, "lead");
            var service = new WorkspaceService(context);

            var workspace = await service.CreateWorkspaceAsync(
                new CreateWorkspaceDto { Name = "Team" }, ownerId);
            await service.JoinByCodeAsync(workspace.InviteCode, leadId);
            await service.AssignRoleAsync(
                workspace.Id, leadId, "Lead", ownerId);

            // A Lead trying to remove the Owner should be forbidden
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RemoveMemberAsync(workspace.Id, ownerId, leadId));
        }

        [Fact]
        public async Task AssignRoleAsync_Throws_WhenRequesterIsNotOwner()
        {
            var context = CreateContext();
            var ownerId = await SeedUserAsync(context, "owner");
            var memberId = await SeedUserAsync(context, "member");
            var service = new WorkspaceService(context);

            var workspace = await service.CreateWorkspaceAsync(
                new CreateWorkspaceDto { Name = "Team" }, ownerId);
            await service.JoinByCodeAsync(workspace.InviteCode, memberId);

            // A Member trying to assign roles should be forbidden
            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.AssignRoleAsync(
                    workspace.Id, memberId, "Lead", memberId));
        }
    }
}