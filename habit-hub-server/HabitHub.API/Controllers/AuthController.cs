using Microsoft.AspNetCore.Mvc;
using HabitHub.API.Models.DTOs;
using System.Threading.Tasks;
using HabitHub.API.Extensions;
using HabitHub.API.Services;

using HabitHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HabitHub.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return CreatedAtAction(nameof(Login), new { }, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        [HttpGet("sessions")]
        [Authorize] 
        public async Task<IActionResult> GetSessions()
        {
            var userId = User.GetUserId();
            var sessions = await _authService.GetActiveSessionsAsync(userId);
            return Ok(sessions);
        }

        [HttpDelete("sessions/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> InvalidateSession(Guid sessionId)
        {
            var userId = User.GetUserId();
            await _authService.InvalidateSessionAsync(userId, sessionId);
            return NoContent();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.GetUserId();
            var sessionId = Guid.Parse(User.FindFirst("SessionId")?.Value ?? throw new UnauthorizedAccessException());
            await _authService.ChangePasswordAsync(userId, dto, sessionId);
            return Ok();
        }

        [HttpPost("change-email")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto dto)
        {
            var userId = User.GetUserId();
            var sessionId = Guid.Parse(User.FindFirst("SessionId")?.Value ?? throw new UnauthorizedAccessException());
            await _authService.ChangeEmailAsync(userId, dto, sessionId);
            return Ok();
        }
    }
}