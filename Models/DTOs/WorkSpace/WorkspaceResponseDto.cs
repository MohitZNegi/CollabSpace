namespace CollabSpace.Models.DTOs.WorkSpace
{
    public class WorkspaceResponseDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string InviteCode { get; set; }
        public required string OwnerUsername { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
