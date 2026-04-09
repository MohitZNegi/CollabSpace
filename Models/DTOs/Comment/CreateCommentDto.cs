namespace CollabSpace.Models.DTOs.Comment
{
    public class CreateCommentDto
    {
        public required string Content { get; set; }

        // Null for top-level comments.
        // Set to the parent comment's ID for replies.
        public Guid? ParentCommentId { get; set; }
    }
}