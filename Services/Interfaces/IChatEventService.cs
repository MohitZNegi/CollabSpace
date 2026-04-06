namespace CollabSpace.Services.Interfaces
{
    public interface IChatEventService
    {
        // Broadcast a new workspace message to all workspace members
        Task BroadcastWorkspaceMessageAsync(
            string workspaceId, object message);

        // Send a direct message to a specific user only
        Task SendDirectMessageAsync(
            string recipientUserId, object message);

        // Broadcast typing indicator to workspace group
        Task BroadcastTypingAsync(
            string workspaceId, object typingData);

        // Send typing indicator to a specific user in a DM
        Task SendTypingToUserAsync(
            string recipientUserId, object typingData);
    }
}