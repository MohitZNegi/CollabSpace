using System;

namespace CollabSpace.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }
        public Guid SenderId { get; set; }
        public User? Sender { get; set; }
        public required string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsEdited { get; set; } = false;
    }
}
