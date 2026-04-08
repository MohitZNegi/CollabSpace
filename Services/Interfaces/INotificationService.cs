using CollabSpace.Models.DTOs.Notification;

namespace CollabSpace.Services.Interfaces
{
    public interface INotificationService
    {
        // Create, save, and immediately push a notification.
        // Called from other services after their primary operation succeeds.
        Task NotifyCardUpdatedAsync(
            Guid cardOwnerId, string cardTitle,
            string updaterUsername, Guid cardId);

        Task NotifyCardAssignedAsync(
            Guid assigneeId, string cardTitle,
            string assignerUsername, Guid cardId);

        Task NotifyCommentAddedAsync(
            Guid cardOwnerId, string cardTitle,
            string commenterUsername, Guid commentId,
            Guid commentAuthorId);

        Task NotifyMentionsAsync(
            List<Guid> mentionedUserIds, string mentionerUsername,
            string cardTitle, Guid commentId);

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