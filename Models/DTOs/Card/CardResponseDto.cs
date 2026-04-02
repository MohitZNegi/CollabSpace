namespace CollabSpace.Models.DTOs.Card
{
    public class CardResponseDto
    {
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public string? AssignedToUsername { get; set; }
        public required string Status { get; set; }
        public int Position { get; set; }
        public required string CreatedByUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CommentCount { get; set; }
    }
}