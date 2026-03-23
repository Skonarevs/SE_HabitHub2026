namespace HabitHub.Models.Entities
{
    public class Reminder
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid HabitId { get; set; }
        public Habit Habit { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public bool Enabled { get; set; } = true; 
        public TimeOnly? Time { get; set; } 
    }
}