namespace CollabSpace.Models.DTOs.Dashboard
{
    public class ActivityResponseDto
    {
        public Guid Id { get; set; }
        public required string ActorUsername { get; set; }
        public string? ActorAvatarUrl { get; set; }
        public required string ActionType { get; set; }
        public required string Description { get; set; }
        public Guid? EntityId { get; set; }
        public string? EntityType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}