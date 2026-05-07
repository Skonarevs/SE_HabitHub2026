using HabitHub.API.Exceptions;
using HabitHub.API.Models.DTOs;
using HabitHub.API.Services.Interfaces;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HabitHub.API.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        // Helper: Get team and check user access (active member or creator)
        private async Task<Team> GetTeamWithAccessAsync(Guid teamId, Guid userId)
        {
            var team = await _context.Teams
                .Include(t => t.Chat)
                .FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null)
            {
                throw new NotFoundException("Team not found");
            }
                
            var isCreator = team.CreatorId == userId;
            var isActiveMember = await _context.Memberships
                .AnyAsync(m => m.TeamId == teamId && m.UserId == userId && m.Status == MembershipStatus.Active);
            if (!isCreator && !isActiveMember)
            {
                throw new ForbiddenException("You are not a member of this team");
            }
                

            return team;
        }

        public async Task<List<MessageResponseDto>> GetMessagesAsync(Guid teamId, Guid userId)
        {
            var team = await GetTeamWithAccessAsync(teamId, userId);
            var chatId = team.Chat.Id;

            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageResponseDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.Sender != null ? m.Sender.Name : "Unknown",
                    Content = m.Content,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            return messages;
        }

        public async Task<MessageResponseDto> SendMessageAsync(Guid teamId, Guid userId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ValidationException("Message content cannot be empty");
            }
                
            var team = await GetTeamWithAccessAsync(teamId, userId);
            var chatId = team.Chat.Id;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
                
            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                SenderId = userId,
                Content = content,
                SentAt = DateTime.UtcNow
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return new MessageResponseDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = user.Name,
                Content = message.Content,
                SentAt = message.SentAt
            };
        }

        public async Task DeleteMessageAsync(Guid teamId, Guid messageId, Guid userId)
        {
            var team = await GetTeamWithAccessAsync(teamId, userId);
            var chatId = team.Chat.Id;

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ChatId == chatId);
            if (message == null)
            {
                throw new NotFoundException("Message not found");
            }

            var isSender = message.SenderId == userId;
            var isCreator = team.CreatorId == userId;
            if (!isSender && !isCreator)
            {
                throw new ForbiddenException("You do not have permission to delete this message");
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }
    }
}