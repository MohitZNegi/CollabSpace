namespace CollabSpace.Models.DTOs.Chat
{
    public class DirectMessageResponseDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public required string SenderUsername { get; set; }
        public string? SenderAvatarUrl { get; set; }
        public Guid RecipientId { get; set; }
        public required string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}