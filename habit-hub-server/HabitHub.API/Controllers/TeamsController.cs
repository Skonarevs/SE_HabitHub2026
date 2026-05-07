using Microsoft.AspNetCore.Mvc;
using HabitHub.API.Models.DTOs;
using System.Threading.Tasks;
using HabitHub.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using HabitHub.API.Services;
using HabitHub.API.Services.Interfaces;
using HabitHub.API.Controllers;

namespace HabitHub.API.Controllers
{
    [ApiController]
    [Route("teams")]
    [Authorize]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpPost]
public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto dto)
{
    try
    {
        var userId = User.GetUserId();
        var team = await _teamService.CreateTeamAsync(userId, dto.Name);
        return CreatedAtAction(nameof(GetTeam), new { teamId = team.Id }, team);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex); 
        return StatusCode(500, ex.Message);
    }
}
    //    [HttpGet]
    //    public async Task<IActionResult> GetUserTeams()
    //   {
    //      var userId = User.GetUserId();
    //      var teams = await _teamService.GetUserTeamsAsync(userId);
    //      return Ok(teams);
    //    }
        [HttpGet("{teamId}")]
        public async Task<IActionResult> GetTeam(Guid teamId)
        {
            var userId = User.GetUserId();
            var team = await _teamService.GetTeamAsync(teamId, userId);
            return Ok(team);
        }

        [HttpPost("{teamId}/invite-codes")]
        public async Task<IActionResult> GenerateInviteCode(Guid teamId)
        {
            var userId = User.GetUserId();
            var code = await _teamService.GenerateInviteCodeAsync(teamId, userId);
            return Created("", code); // location empty for now
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinTeam([FromBody] JoinTeamDto dto)
        {
            var userId = User.GetUserId();
            var membership = await _teamService.JoinTeamAsync(userId, dto.Code);
            return Ok(membership);
        }

        [HttpPost("{teamId}/members/{memberId}/kick")]
        public async Task<IActionResult> KickUser(Guid teamId, Guid memberId)
        {
            var userId = User.GetUserId();
            await _teamService.KickMemberAsync(teamId, memberId, userId);
            return Ok();
        }

        [HttpPost("{teamId}/leave")]
        public async Task<IActionResult> LeaveTeam(Guid teamId)
        {
            var userId = User.GetUserId();
            await _teamService.LeaveTeamAsync(teamId, userId);
            return Ok();
        }

        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(Guid teamId)
        {
            var userId = User.GetUserId();
            await _teamService.DeleteTeamAsync(teamId, userId);
            return NoContent();
        }

        [HttpPost("{teamId}/habits")]
        public async Task<IActionResult> CreateHabit(Guid teamId, [FromBody] CreateHabitDto dto)
        {
            var userId = User.GetUserId();
            var habit = await _teamService.CreateHabitAsync(teamId, dto, userId);
            return CreatedAtAction(nameof(HabitsController.GetProgress), "Habits", new { habitId = habit.Id }, habit);
        }

        [HttpGet("{teamId}/habits")]
        public async Task<IActionResult> GetArchivedHabits(Guid teamId, [FromQuery] string state)
        {
            var userId = User.GetUserId();
            if (state?.ToLower() == "active")
            {
                var active = await _teamService.GetActiveHabitsAsync(teamId, userId);
                return Ok(active);
            }
            if (state?.ToLower() == "archived")
            {
                var archived = await _teamService.GetArchivedHabitsAsync(teamId, userId);
                return Ok(archived);
            }
            return BadRequest("Invalid state parameter.");
        }

        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            var userId = User.GetUserId();
            var teams = await _teamService.GetTeamsForUserAsync(userId);
            return Ok(teams);
        }

        [HttpGet("{teamId}/members")]
        [Authorize]
        public async Task<IActionResult> GetTeamMembers(Guid teamId)
        {
            var userId = User.GetUserId();
            var members = await _teamService.GetTeamMembersAsync(teamId, userId);
            return Ok(members);
        }


        [HttpGet("{teamId}/habits")]
        public async Task<IActionResult> GetTeamHabits(Guid teamId, [FromQuery] string? state = null)
        {
            var userId = User.GetUserId();
            var habits = await _teamService.GetTeamHabitsAsync(teamId, userId, state);
            return Ok(habits);
        }




    }
}