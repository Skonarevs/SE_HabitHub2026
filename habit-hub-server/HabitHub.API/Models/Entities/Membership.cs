namespace HabitHub.Models.Entities
{
    public class Membership
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid TeamId { get; set; }
        public Team Team { get; set; }
        public MembershipStatus Status { get; set; } = MembershipStatus.Active;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
    }

    public enum MembershipStatus
    {
        Active,
        Left,
        Kicked
    }
}