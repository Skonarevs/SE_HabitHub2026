using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HabitHub.API.Extensions;
using HabitHub.API.Models.DTOs;
using HabitHub.API.Services;
using HabitHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HabitHub.API.Controllers
{
    [ApiController]
    [Route("habits/{habitId}/reminder")]
    [Authorize]
    public class RemindersController : ControllerBase
    {
        private readonly IReminderService _reminderService;

        public RemindersController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        [HttpPatch]
        public async Task<IActionResult> SetReminder(Guid habitId, [FromBody] SetReminderDto dto)
        {
            var userId = User.GetUserId();
            await _reminderService.SetReminderAsync(habitId, userId, dto.ReminderTime);
            return Ok();
        }

        [HttpPatch("enabled")]
        public async Task<IActionResult> ChangeReminderEnabled(Guid habitId, [FromBody] ChangeReminderDto dto)
        {
            var userId = User.GetUserId();
            await _reminderService.ChangeReminderEnabledAsync(habitId, userId, dto.Enabled);
            return Ok();
        }
    }
}