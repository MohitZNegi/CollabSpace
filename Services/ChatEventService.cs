using CollabSpace.Hubs;
using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CollabSpace.Services
{
    public class ChatEventService : IChatEventService
    {
        private readonly IHubContext<CollabHub> _hubContext;

        public ChatEventService(IHubContext<CollabHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastWorkspaceMessageAsync(
            string workspaceId, object message)
        {
            // Sends to all users in the workspace group.
            // Including the sender so their message appears
            // confirmed in their own chat window.
            await _hubContext.Clients
                .Group($"workspace:{workspaceId}")
                .SendAsync("ReceiveWorkspaceMessage", message);
        }

        public async Task SendDirectMessageAsync(
            string recipientUserId, object message)
        {
            // Clients.User() sends to a specific user by their
            // user ID claim. SignalR handles the connection mapping.
            // If the user has multiple tabs open, all receive it.
            // If they are offline, nothing is sent — which is fine
            // because they will load history via REST on reconnect.
            await _hubContext.Clients
                .User(recipientUserId)
                .SendAsync("ReceiveDirectMessage", message);
        }

        public async Task BroadcastTypingAsync(
            string workspaceId, object typingData)
        {
            await _hubContext.Clients
                .Group($"workspace:{workspaceId}")
                .SendAsync("UserTyping", typingData);
        }

        public async Task SendTypingToUserAsync(
            string recipientUserId, object typingData)
        {
            await _hubContext.Clients
                .User(recipientUserId)
                .SendAsync("UserTyping", typingData);
        }
    }
}