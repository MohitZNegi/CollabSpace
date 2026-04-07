using CollabSpace.Models.DTOs.Notification;

namespace CollabSpace.Services.Interfaces
{
    public interface INotificationEventService
    {
        // Delivers a notification to a specific user via SignalR.
        // If they are offline this does nothing — they get the
        // notification from the database when they next log in.
        Task PushNotificationAsync(
            string recipientUserId,
            NotificationResponseDto notification);
    }
}