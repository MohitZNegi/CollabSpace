namespace CollabSpace.Models.DTOs.Card
{
    public class UpdateCardDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public required string Status { get; set; }
    }
}