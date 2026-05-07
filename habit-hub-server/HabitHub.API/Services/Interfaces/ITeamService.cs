using HabitHub.API.Models.DTOs;
using System.Threading.Tasks;

namespace HabitHub.API.Services.Interfaces
{
    public interface ITeamService
    {
        Task<TeamResponseDto> CreateTeamAsync(Guid creatorId, string teamName);
        Task<TeamResponseDto> GetTeamAsync(Guid teamId, Guid userId);
        Task<InviteCodeResponseDto> GenerateInviteCodeAsync(Guid teamId, Guid creatorId);
        Task<MembershipResponseDto> JoinTeamAsync(Guid userId, string inviteCode);
        Task KickMemberAsync(Guid teamId, Guid memberId, Guid requesterId);
        Task LeaveTeamAsync(Guid teamId, Guid userId);
        Task DeleteTeamAsync(Guid teamId, Guid creatorId);
        Task<HabitResponseDto> CreateHabitAsync(Guid teamId, CreateHabitDto dto, Guid creatorId);
        Task<List<HabitResponseDto>> GetActiveHabitsAsync(Guid teamId, Guid userId);
        Task<List<ArchivedHabitDto>> GetArchivedHabitsAsync(Guid teamId, Guid userId);

        Task<List<TeamResponseDto>> GetTeamsForUserAsync(Guid userId);
        Task<List<TeamMemberResponseDto>> GetTeamMembersAsync(Guid teamId, Guid requesterId);

        Task<List<HabitResponseDto>> GetTeamHabitsAsync(Guid teamId, Guid userId, string? state);
    }
}