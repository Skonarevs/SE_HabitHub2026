namespace HabitHub.Models.Entities
{
    public class TeamMember : User
    {
    }

    public class TeamCreator : User
    {
        public ICollection<Team> CreatedTeams { get; set; } = new List<Team>();
    }
}