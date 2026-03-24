using System;

namespace CollabSpace.Models
{
    public class WorkspaceMember
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public string WorkspaceRole { get; set; } = "Member";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
