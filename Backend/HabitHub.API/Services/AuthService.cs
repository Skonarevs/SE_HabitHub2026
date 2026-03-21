using HabitHub.API.Models.DTOs;
using HabitHub.API.Services.Interfaces;
using HabitHub.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HabitHub.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<List<SessionInfoDto>> GetActiveSessionsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task InvalidateSessionAsync(Guid userId, Guid sessionId)
        {
            throw new NotImplementedException();
        }

        public Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            throw new NotImplementedException();
        }

        public Task ChangeEmailAsync(Guid userId, ChangeEmailDto dto)
        {
            throw new NotImplementedException();
        }
    }
}