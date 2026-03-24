using System;

namespace CollabSpace.Models
{
    public class DirectMessage
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public User? Sender { get; set; }
        public Guid RecipientId { get; set; }
        public User? Recipient { get; set; }
        public required string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
