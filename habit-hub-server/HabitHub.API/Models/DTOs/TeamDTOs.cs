namespace HabitHub.API.Models.DTOs
{
    public class CreateTeamDto
    {
        public string Name { get; set; }
    }

    public class TeamResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class InviteCodeResponseDto
    {
        public string Code { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class JoinTeamDto
    {
        public string Code { get; set; }
    }

    //public class KickUserDto
    //{
    //    
    //}

    public class MembershipResponseDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }
        public string Status { get; set; } 
    }
}
