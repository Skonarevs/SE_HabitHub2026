namespace HabitHub.Models.Entities
{
    public class Habit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Goal { get; set; }
        public HabitType Type { get; set; }
        public string Unit { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public HabitState State { get; set; } = HabitState.Active;
        public Guid TeamId { get; set; }
        public Team Team { get; set; }
        public ICollection<HabitEntry> Entries { get; set; } = new List<HabitEntry>();
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }

    public enum HabitType { 
        Binary,
        Quantitative 
    }
    public enum HabitState { 
        Active, 
        Archived, 
        Closed 
    }
}