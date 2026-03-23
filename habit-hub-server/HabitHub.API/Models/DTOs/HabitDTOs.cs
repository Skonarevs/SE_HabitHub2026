namespace HabitHub.API.Models.DTOs
{
    public class CreateHabitDto
    {
        public string Name { get; set; }
        public string Goal { get; set; }
        public string HabitType { get; set; } 
        public string Unit { get; set; } 
        public DateTime? ExpiryDate { get; set; } 
    }

    public class UpdateHabitDto
    {
        public string Name { get; set; }
        public string Goal { get; set; }
        public string HabitType { get; set; }
        public string Unit { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class HabitResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Goal { get; set; }
        public string HabitType { get; set; }
        public string Unit { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string State { get; set; } 
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }
    }

    public class LogProgressDto
    {
        public float? Value { get; set; } 
        public string Notes { get; set; } 
        public string Status { get; set; }
    }

    public class HabitEntryResponseDto
    {
        public Guid Id { get; set; }
        public Guid HabitId { get; set; }
        public string HabitName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public float? Value { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class LeaderboardEntryDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public float? Progress { get; set; }
    }

    public class ArchivedHabitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Goal { get; set; }
        public string HabitType { get; set; }
        public string Unit { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime ArchivedAt { get; set; }
    }
}
