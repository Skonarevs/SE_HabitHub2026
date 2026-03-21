namespace HabitHub.Models.Entities
{
    public class Session
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Active;
    }

    public enum SessionStatus { 
        Active, 
        Expired, 
        Invalidated 
    }
}