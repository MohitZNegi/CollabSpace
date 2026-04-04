namespace CollabSpace.Services.Interfaces
{
    // This service is the bridge between your business logic layer
    // and SignalR. When a card changes, CardService calls this.
    // This service broadcasts the change to all connected clients.
    // Keeping it as an interface lets you mock it in tests.
    public interface IBoardEventService
    {
        // Broadcast that a card was created on a board
        Task BroadcastCardCreatedAsync(string boardId, object card);

        // Broadcast that a card was updated
        Task BroadcastCardUpdatedAsync(string boardId, object card);

        // Broadcast that a card was moved (lightweight — just id, status, position)
        Task BroadcastCardMovedAsync(string boardId, object moveData);

        // Broadcast that a card was deleted
        Task BroadcastCardDeletedAsync(string boardId, Guid cardId);
    }
}