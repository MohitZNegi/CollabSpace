using System;
using System.Collections.Generic;

namespace CollabSpace.Models
{
    public class Card
    {
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
        public Board? Board { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public User? AssignedTo { get; set; }
        public string Status { get; set; } = "Todo";
        public int Position { get; set; } = 0;
        public Guid CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
