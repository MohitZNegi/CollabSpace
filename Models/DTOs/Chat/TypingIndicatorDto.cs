namespace CollabSpace.Models.DTOs.Chat
{
    // Typing indicators are never persisted to the database.
    // They are ephemeral SignalR-only events. This DTO is only
    // used for the real-time broadcast payload.
    public class TypingIndicatorDto
    {
        public Guid UserId { get; set; }
        public required string Username { get; set; }
        public required string Context { get; set; } // "workspace" or "direct"
        public required string ContextId { get; set; }
    }
}