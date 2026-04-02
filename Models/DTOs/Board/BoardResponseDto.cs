namespace CollabSpace.Models.DTOs.Board
{
    public class BoardResponseDto
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public required string Name { get; set; }
        public required string CreatedByUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsArchived { get; set; }
    }
}