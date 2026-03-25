namespace CollabSpace.Models.DTOs.Auth
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string GlobalRole { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
