using HabitHub.API.Models.DTOs;
using System.Threading.Tasks;

namespace HabitHub.API.Services.Interfaces
{
    public interface IChatService
    {
        Task<List<MessageResponseDto>> GetMessagesAsync(Guid teamId, Guid userId);
        Task<MessageResponseDto> SendMessageAsync(Guid teamId, Guid userId, string content);
        Task DeleteMessageAsync(Guid teamId, Guid messageId, Guid userId);
    }
}