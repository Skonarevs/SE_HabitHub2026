using HabitHub.API.Models.DTOs;
using System.Threading.Tasks;

namespace HabitHub.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<List<SessionInfoDto>> GetActiveSessionsAsync(Guid userId);
        Task InvalidateSessionAsync(Guid userId, Guid sessionId);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task ChangeEmailAsync(Guid userId, ChangeEmailDto dto);
    }
}