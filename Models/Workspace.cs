using System;
using System.Collections.Generic;

namespace CollabSpace.Models
{
    public class Workspace
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Guid OwnerId { get; set; }
        public User? Owner { get; set; }
        public required string InviteCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; } = false;

        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
        public ICollection<Board> Boards { get; set; } = new List<Board>();
    }
}
