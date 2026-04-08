using CollabSpace.Hubs;
using CollabSpace.Models.DTOs.Notification;
using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CollabSpace.Services
{
    public class NotificationEventService : INotificationEventService
    {
        private readonly IHubContext<CollabHub> _hubContext;

        public NotificationEventService(IHubContext<CollabHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PushNotificationAsync(
            string recipientUserId,
            NotificationResponseDto notification)
        {
            // Clients.User() sends to a specific authenticated user.
            // SignalR maps the user's ID claim to their connection(s).
            // If they have multiple tabs open, all receive the event.
            // If they are offline, nothing is sent — no error thrown.
            await _hubContext.Clients
                .User(recipientUserId)
                .SendAsync("ReceiveNotification", notification);
        }
    }
}