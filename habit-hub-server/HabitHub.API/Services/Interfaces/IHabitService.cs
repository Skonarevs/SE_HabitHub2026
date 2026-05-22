using HabitHub.API.Models.DTOs;
using System.Threading.Tasks;

namespace HabitHub.API.Services.Interfaces
{
    public interface IHabitService
    {
        Task<HabitResponseDto> UpdateHabitAsync(Guid habitId, UpdateHabitDto dto, Guid userId);
        Task ArchiveHabitAsync(Guid habitId, Guid userId);
        Task DeleteHabitAsync(Guid habitId, Guid userId);
        Task<HabitEntryResponseDto> LogProgressAsync(Guid habitId, Guid userId, LogProgressDto dto);
        Task UndoLogAsync(Guid habitId, Guid entryId, Guid userId);
        Task<List<HabitEntryResponseDto>> GetProgressAsync(Guid habitId, Guid userId, Guid? memberId);
        Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(Guid habitId, Guid userId);
        Task<List<HabitResponseDto>> GetUserHabitsAsync(Guid userId);
    }
}