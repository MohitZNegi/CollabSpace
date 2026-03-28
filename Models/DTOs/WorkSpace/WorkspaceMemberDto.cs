namespace CollabSpace.Models.DTOs.WorkSpace
{
    public class WorkspaceMemberDto
    {
        public Guid UserId { get; set; }
        public required string Username { get; set; }
        public string? AvatarUrl { get; set; }
        public required string WorkspaceRole { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
    }
}
