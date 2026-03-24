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

    }
}
