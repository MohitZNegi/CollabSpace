namespace CollabSpace.Models.DTOs.Card
{
    // Separate DTO for move operations.
    // Moving a card is a distinct action from editing its content.
    // This maps to the PATCH /cards/{id}/move endpoint which is
    // optimised for drag and drop — lightweight, frequent, fast.
    public class MoveCardDto
    {
        public required string Status { get; set; }
        public int Position { get; set; }
    }
}