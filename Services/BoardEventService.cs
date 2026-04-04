using CollabSpace.Hubs;
using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CollabSpace.Services
{
    public class BoardEventService : IBoardEventService
    {
        // IHubContext gives you access to the hub from outside it.
        // You can call Clients.Group() exactly as you would inside the hub.
        private readonly IHubContext<CollabHub> _hubContext;

        public BoardEventService(IHubContext<CollabHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Each method broadcasts a named event to all clients
        // currently in the specified board group.
        // The event name (e.g. "CardCreated") must exactly match
        // the listener name registered on the JavaScript client.

        public async Task BroadcastCardCreatedAsync(
            string boardId, object card)
        {
            await _hubContext.Clients
                .Group($"board:{boardId}")
                .SendAsync("CardCreated", card);
        }

        public async Task BroadcastCardUpdatedAsync(
            string boardId, object card)
        {
            await _hubContext.Clients
                .Group($"board:{boardId}")
                .SendAsync("CardUpdated", card);
        }

        public async Task BroadcastCardMovedAsync(
            string boardId, object moveData)
        {
            await _hubContext.Clients
                .Group($"board:{boardId}")
                .SendAsync("CardMoved", moveData);
        }

        public async Task BroadcastCardDeletedAsync(
            string boardId, Guid cardId)
        {
            await _hubContext.Clients
                .Group($"board:{boardId}")
                .SendAsync("CardDeleted", new { cardId });
        }
    }
}