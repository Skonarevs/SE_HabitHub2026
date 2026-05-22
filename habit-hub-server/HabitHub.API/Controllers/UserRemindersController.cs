using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitHub.API.Services.Interfaces;
using HabitHub.API.Extensions;
using System.Threading.Tasks;

namespace HabitHub.API.Controllers
{
    [ApiController]
    [Route("reminders")]
    [Authorize]
    public class UserRemindersController : ControllerBase
    {
        private readonly IReminderService _reminderService;

        public UserRemindersController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyReminders()
        {
            var userId = User.GetUserId();
            var reminders = await _reminderService.GetUserRemindersAsync(userId);
            return Ok(reminders);
        }
    }
}