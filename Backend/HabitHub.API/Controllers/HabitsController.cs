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
    [Route("habits")]
    [Authorize]
    public class HabitsController : ControllerBase
    {
        private readonly IHabitService _habitService;

        public HabitsController(IHabitService habitService)
        {
            _habitService = habitService;
        }

        [HttpPatch("{habitId}")]
        public async Task<IActionResult> EditHabit(Guid habitId, [FromBody] UpdateHabitDto dto)
        {
            var userId = User.GetUserId();
            var updated = await _habitService.UpdateHabitAsync(habitId, dto, userId);
            return Ok(updated);
        }

        [HttpPost("{habitId}/archive")]
        public async Task<IActionResult> ArchiveHabit(Guid habitId)
        {
            var userId = User.GetUserId();
            await _habitService.ArchiveHabitAsync(habitId, userId);
            return Ok();
        }

        [HttpDelete("{habitId}")]
        public async Task<IActionResult> DeleteHabit(Guid habitId)
        {
            var userId = User.GetUserId();
            await _habitService.DeleteHabitAsync(habitId, userId);
            return NoContent();
        }

        [HttpPost("{habitId}/entries")]
        public async Task<IActionResult> LogProgress(Guid habitId, [FromBody] LogProgressDto dto)
        {
            var userId = User.GetUserId();
            var entry = await _habitService.LogProgressAsync(habitId, userId, dto);
            return CreatedAtAction(nameof(GetProgress), new { habitId, entryId = entry.Id }, entry); //Location constructor
        }

        [HttpDelete("{habitId}/entries/{entryId}")]
        public async Task<IActionResult> UndoLog(Guid habitId, Guid entryId)
        {
            var userId = User.GetUserId();
            await _habitService.UndoLogAsync(habitId, entryId, userId);
            return NoContent();
        }

        [HttpGet("{habitId}/entries")]
        public async Task<IActionResult> GetProgress(Guid habitId, [FromQuery] Guid? memberId)
        {
            var userId = User.GetUserId();
            var entries = await _habitService.GetProgressAsync(habitId, userId, memberId);
            return Ok(entries);
        }

        [HttpGet("{habitId}/leaderboard")]
        public async Task<IActionResult> GetLeaderboard(Guid habitId)
        {
            var userId = User.GetUserId();
            var leaderboard = await _habitService.GetLeaderboardAsync(habitId, userId);
            return Ok(leaderboard);
        }
    }
}