namespace HabitHub.Models.Entities
{
    public class TeamChat
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TeamId { get; set; }
        public Team Team { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }

    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ChatId { get; set; }
        public TeamChat Chat { get; set; }
        public Guid SenderId { get; set; }
        public User Sender { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
