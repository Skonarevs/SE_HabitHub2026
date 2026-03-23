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
    [Route("teams/{teamId}/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages(Guid teamId)
        {
            var userId = User.GetUserId();
            var messages = await _chatService.GetMessagesAsync(teamId, userId);
            return Ok(messages);
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage(Guid teamId, [FromBody] SendMessageDto dto)
        {
            var userId = User.GetUserId();
            var message = await _chatService.SendMessageAsync(teamId, userId, dto.Content);
            return CreatedAtAction(nameof(GetMessages), new { teamId }, message);
        }

        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid teamId, Guid messageId)
        {
            var userId = User.GetUserId();
            await _chatService.DeleteMessageAsync(teamId, messageId, userId);
            return NoContent();
        }
    }
}