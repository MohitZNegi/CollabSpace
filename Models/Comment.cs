using System;
using System.Collections.Generic;

namespace CollabSpace.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public Card? Card { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public required string Content { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEdited { get; set; } = false;
    }
}
