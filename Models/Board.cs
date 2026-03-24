using System;
using System.Collections.Generic;

namespace CollabSpace.Models
{
    public class Board
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }
        public required string Name { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; } = false;

        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}
