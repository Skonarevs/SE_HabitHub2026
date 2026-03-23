namespace HabitHub.Models.Entities
{
    public class HabitEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid HabitId { get; set; }
        public Habit Habit { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public DateTime Date { get; set; } 
        public float? Value { get; set; } 
        public EntryStatus Status { get; set; } = EntryStatus.Pending;
        public string Notes { get; set; }
    }

    public enum EntryStatus { 
        Pending, 
        Logged, 
        Skipped 
    }
}