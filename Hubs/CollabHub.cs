using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CollabSpace.Hubs
{
    // [Authorize] means the client must send a valid JWT when connecting.
    // SignalR reads it from the query string: ?access_token={jwt}
    // This is because WebSocket headers cannot be set by browsers,
    // so the token is passed as a query parameter instead.
    [Authorize]
    public class CollabHub : Hub
    {
        private readonly ICurrentUserService _currentUser;

        public CollabHub(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        // Called automatically by SignalR when a client connects.
        // Context.ConnectionId is a unique ID for this connection.
        // Context.UserIdentifier is the user's ID from the JWT claim.
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        // Called automatically when a client disconnects.
        // exception is null for clean disconnections (tab closed),
        // or contains the error for unexpected disconnections.
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        // Client calls this when they open a board view.
        // Adds this connection to the board's broadcast group.
        // Group name format: "board:{boardId}"
        public async Task JoinBoard(string boardId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"board:{boardId}");

            // Notify other users on this board that someone joined.
            // Clients.OthersInGroup excludes the caller from receiving this.
            await Clients.OthersInGroup($"board:{boardId}")
                .SendAsync("UserJoinedBoard", new
                {
                    UserId = Context.UserIdentifier,
                    BoardId = boardId,
                    JoinedAt = DateTime.UtcNow
                });
        }

        // Client calls this when they leave the board view.
        // Removes them from the group so they stop receiving updates.
        public async Task LeaveBoard(string boardId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, $"board:{boardId}");

            await Clients.OthersInGroup($"board:{boardId}")
                .SendAsync("UserLeftBoard", new
                {
                    UserId = Context.UserIdentifier,
                    BoardId = boardId
                });
        }

        // Client calls this when they open a workspace.
        // Used for workspace-level events: chat messages, notifications.
        public async Task JoinWorkspace(string workspaceId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"workspace:{workspaceId}");
        }

        public async Task LeaveWorkspace(string workspaceId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, $"workspace:{workspaceId}");
        }
    }
}