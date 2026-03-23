namespace HabitHub.Models.Entities
{
    public class InviteCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } 
        public Guid TeamId { get; set; }
        public Team Team { get; set; }
        public DateTime ExpiryDate { get; set; }
        public InviteCodeState State { get; set; } = InviteCodeState.Active;
    }

    public enum InviteCodeState { 
        Active, 
        Expired, 
        Invalid 
    }
}
