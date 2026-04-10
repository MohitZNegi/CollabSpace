namespace CollabSpace.Models.DTOs.Dashboard
{
    public class DashboardResponseDto
    {
        public List<ActivityResponseDto> RecentActivity { get; set; }
            = new();

        public TaskStatsDto TaskStats { get; set; } = new();

        public List<ActiveMemberDto> ActiveMembers { get; set; }
            = new();
    }

    public class TaskStatsDto
    {
        public int Todo { get; set; }
        public int InProgress { get; set; }
        public int Done { get; set; }
        public int Total { get; set; }
    }

    public class ActiveMemberDto
    {
        public Guid UserId { get; set; }
        public required string Username { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? LastSeenAt { get; set; }
    }
}