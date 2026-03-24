using System;

namespace CollabSpace.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid RecipientUserId { get; set; }
        public User? Recipient { get; set; }
        public required string Type { get; set; }
        public required string Message { get; set; }
        public Guid? ReferenceId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
