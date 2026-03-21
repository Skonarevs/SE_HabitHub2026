using System.Threading.Tasks;

namespace HabitHub.API.Services.Interfaces
{
    public interface IReminderService
    {
        Task SetReminderAsync(Guid habitId, Guid userId, string reminderTime);
        Task ChangeReminderEnabledAsync(Guid habitId, Guid userId, bool enabled);
    }
}