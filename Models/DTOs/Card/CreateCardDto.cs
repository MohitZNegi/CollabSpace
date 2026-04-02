namespace CollabSpace.Models.DTOs.Card
{
    public class CreateCardDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
    }
}