namespace CollabSpace.Models
{
    // Records every significant event in a workspace.
    // This is an append-only log — entries are never updated,
    // only created. This makes it fast to write and safe to query.
    public class ActivityLog
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        // Who performed the action
        public Guid ActorId { get; set; }
        public User? Actor { get; set; }

        // What type of action occurred
        // Examples: "CardCreated", "CardMoved", "MemberJoined",
        //           "MessageSent", "BoardCreated", "CommentAdded"
        public required string ActionType { get; set; }

        // Human-readable description built at record time.
        // Example: "Alice moved 'Fix login bug' to Done"
        public required string Description { get; set; }

        // Optional link to the entity involved
        public Guid? EntityId { get; set; }
        public string? EntityType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}