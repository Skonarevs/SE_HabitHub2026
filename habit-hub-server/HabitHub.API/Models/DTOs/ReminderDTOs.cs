namespace HabitHub.API.Models.DTOs
{
    public class SetReminderDto
    {
        public string ReminderTime { get; set; }
    }

    public class ChangeReminderDto
    {
        public bool Enabled { get; set; }
    }

    public class ReminderResponseDto
    {
        public Guid HabitId { get; set; }
        public string HabitName { get; set; }
        public bool Enabled { get; set; }
        public string ReminderTime { get; set; } 
    }
    public class UserReminderDto
    {
        public Guid HabitId { get; set; }
        public string HabitName { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }
        public TimeOnly? DefaultReminderTime { get; set; }  
        public bool Enabled { get; set; }  
    }
}
