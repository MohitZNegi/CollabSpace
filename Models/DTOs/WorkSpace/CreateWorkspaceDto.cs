namespace CollabSpace.Models.DTOs.WorkSpace
{
    public class CreateWorkspaceDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
