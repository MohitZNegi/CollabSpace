namespace CollabSpace.Models.DTOs.Notification
{
    public class NotificationResponseDto
    {
        public Guid Id { get; set; }
        public required string Type { get; set; }
        public required string Message { get; set; }
        public Guid? ReferenceId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}