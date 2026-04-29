using CollabSpace.Models.DTOs.Notification;

namespace CollabSpace.Services.Interfaces
{
    public interface INotificationService
    {
        // Create, save, and immediately push a notification.
        // Called from other services after their primary operation succeeds.
        Task NotifyCardUpdatedAsync(
            Guid cardOwnerId, string cardTitle,
            string updaterUsername, Guid cardId,
            Guid boardId, Guid workspaceId);

        Task NotifyCardAssignedAsync(
            Guid assigneeId, string cardTitle,
            string assignerUsername, Guid cardId,
            Guid boardId, Guid workspaceId);

        Task NotifyCommentAddedAsync(
            Guid cardOwnerId, string cardTitle,
            string commenterUsername, Guid commentId,
            Guid cardId, Guid commentAuthorId,
            Guid boardId, Guid workspaceId);

        Task NotifyMentionsAsync(
            List<Guid> mentionedUserIds, string mentionerUsername,
            string context, Guid referenceId,
            string? navigationUrl = null);

        Task NotifyMemberJoinedAsync(
            List<Guid> existingMemberIds, string newMemberUsername,
            string workspaceName, Guid workspaceId);

        Task NotifyMemberRemovedAsync(
            Guid removedUserId, string workspaceName, Guid workspaceId);

        // REST endpoints for the notification bell UI
        Task<List<NotificationResponseDto>> GetUserNotificationsAsync(
            Guid userId);

        Task MarkAllAsReadAsync(Guid userId);

        Task<int> GetUnreadCountAsync(Guid userId);
    }
}
