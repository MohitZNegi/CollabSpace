using System;
using System.Collections.Generic;

namespace CollabSpace.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string GlobalRole { get; set; } = "Member";
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastSeenAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Workspace> OwnedWorkspaces { get; set; } = new List<Workspace>();
        public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; } = new List<WorkspaceMember>();
        public ICollection<Board> CreatedBoards { get; set; } = new List<Board>();
        public ICollection<Card> AssignedCards { get; set; } = new List<Card>();
        public ICollection<Card> CreatedCards { get; set; } = new List<Card>();

    }
}
