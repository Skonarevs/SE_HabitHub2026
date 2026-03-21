using System;

namespace HabitHub.Models.Entities
{
    public abstract class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Timezone { get; set; } // IANA 
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
        public ICollection<HabitEntry> HabitEntries { get; set; } = new List<HabitEntry>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }
}