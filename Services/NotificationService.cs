using CollabSpace.Data;
using CollabSpace.Factories;
using CollabSpace.Models;
using CollabSpace.Models.DTOs.Notification;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Services
{
    public class NotificationService : BaseService, INotificationService
    {
        private readonly AppDbContext _context;
        private readonly INotificationEventService _notificationEvents;

        public NotificationService(
            AppDbContext context,
            INotificationEventService notificationEvents)
        {
            _context = context;
            _notificationEvents = notificationEvents;
        }

        public async Task NotifyCardUpdatedAsync(
            Guid cardOwnerId, string cardTitle,
            string updaterUsername, Guid cardId)
        {
            // Do not notify the person who made the change.
            // Receiving a notification for your own action is noise.
            if (cardOwnerId == Guid.Empty) return;

            await SaveAndPushAsync(
                NotificationFactory.CardUpdated(
                    cardOwnerId, cardTitle, updaterUsername, cardId));
        }

        public async Task NotifyCardAssignedAsync(
            Guid assigneeId, string cardTitle,
            string assignerUsername, Guid cardId)
        {
            await SaveAndPushAsync(
                NotificationFactory.CardAssigned(
                    assigneeId, cardTitle, assignerUsername, cardId));
        }

        public async Task NotifyCommentAddedAsync(
            Guid cardOwnerId, string cardTitle,
            string commenterUsername, Guid commentId,
            Guid commentAuthorId)
        {
            // Do not notify the card owner if they wrote the comment themselves
            if (cardOwnerId == commentAuthorId) return;

            await SaveAndPushAsync(
                NotificationFactory.CommentAdded(
                    cardOwnerId, cardTitle, commenterUsername, commentId));
        }

        public async Task NotifyMentionsAsync(
            List<Guid> mentionedUserIds, string mentionerUsername,
            string cardTitle, Guid commentId)
        {
            // Create one notification per mentioned user.
            // SaveChangesAsync called once for all of them together.
            var notifications = mentionedUserIds
                .Select(userId => NotificationFactory.Mention(
                    userId, mentionerUsername, cardTitle, commentId))
                .ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Push each notification to its recipient via SignalR
            var pushOperations = notifications.Select(n =>
                new Func<Task>(() => _notificationEvents.PushNotificationAsync(
                    n.RecipientUserId.ToString(), MapToDto(n))));

            await ExecuteSequentiallyAsync(pushOperations);
        }

        public async Task NotifyMemberJoinedAsync(
            List<Guid> existingMemberIds, string newMemberUsername,
            string workspaceName, Guid workspaceId)
        {
            var notifications = existingMemberIds
                .Select(memberId => NotificationFactory.MemberJoined(
                    memberId, newMemberUsername, workspaceName, workspaceId))
                .ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            var pushOperations = notifications.Select(n =>
                new Func<Task>(() => _notificationEvents.PushNotificationAsync(
                    n.RecipientUserId.ToString(), MapToDto(n))));

            await ExecuteSequentiallyAsync(pushOperations);
        }

        public async Task NotifyMemberRemovedAsync(
            Guid removedUserId, string workspaceName, Guid workspaceId)
        {
            await SaveAndPushAsync(
                NotificationFactory.MemberRemoved(
                    removedUserId, workspaceName, workspaceId));
        }

        public async Task<List<NotificationResponseDto>> GetUserNotificationsAsync(
            Guid userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.RecipientUserId == userId)
                // Unread first, then most recent within each group
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => MapToDto(n))
                .ToListAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread) n.IsRead = true;

            if (unread.Any())
                await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .CountAsync(n => n.RecipientUserId == userId && !n.IsRead);
        }

        // ---------------------------------------------------------------
        // PRIVATE HELPERS
        // ---------------------------------------------------------------

        // Saves a single notification and pushes it via SignalR.
        // The database write always happens first. If SignalR fails,
        // the notification still exists in the database for later retrieval.
        private async Task SaveAndPushAsync(
            Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _notificationEvents.PushNotificationAsync(
                notification.RecipientUserId.ToString(),
                MapToDto(notification));
        }

        private static NotificationResponseDto MapToDto(
            Notification n) => new()
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                ReferenceId = n.ReferenceId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            };
    }
}
