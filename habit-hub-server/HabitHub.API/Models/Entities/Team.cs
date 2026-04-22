namespace HabitHub.Models.Entities
{
    public class Team
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public Guid CreatorId { get; set; }
        public TeamCreator Creator { get; set; }
        public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
        public ICollection<Habit> Habits { get; set; } = new List<Habit>();
        public TeamChat Chat { get; set; }
        public ICollection<InviteCode> InviteCodes { get; set; } = new List<InviteCode>();
        public DateTime CreatedAt { get; set; }
    }
}