using HabitHub.API.Models.DTOs;
using HabitHub.API.Services.Interfaces;
using HabitHub.Data;
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

        public Task<List<MessageResponseDto>> GetMessagesAsync(Guid teamId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<MessageResponseDto> SendMessageAsync(Guid teamId, Guid userId, string content)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMessageAsync(Guid teamId, Guid messageId, Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}