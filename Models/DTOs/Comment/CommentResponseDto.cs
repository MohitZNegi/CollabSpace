namespace CollabSpace.Models.DTOs.Comment
{
    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public Guid UserId { get; set; }
        public required string Username { get; set; }
        public string? AvatarUrl { get; set; }
        public required string Content { get; set; }
        public Guid? ParentCommentId { get; set; }
        public bool IsEdited { get; set; }
        public DateTime CreatedAt { get; set; }

        // Replies are nested inside their parent comment.
        // This recursive structure mirrors the tree in the UI.
        public List<CommentResponseDto> Replies { get; set; } = new();
    }
}