using HabitHub.API.Models.DTOs;
using HabitHub.API.Services.Interfaces;
using HabitHub.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HabitHub.API.Services
{
    public class HabitService : IHabitService
    {
        private readonly AppDbContext _context;

        public HabitService(AppDbContext context)
        {
            _context = context;
        }

        public Task<HabitResponseDto> UpdateHabitAsync(Guid habitId, UpdateHabitDto dto, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task ArchiveHabitAsync(Guid habitId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteHabitAsync(Guid habitId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<HabitEntryResponseDto> LogProgressAsync(Guid habitId, Guid userId, LogProgressDto dto)
        {
            throw new NotImplementedException();
        }

        public Task UndoLogAsync(Guid habitId, Guid entryId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<HabitEntryResponseDto>> GetProgressAsync(Guid habitId, Guid userId, Guid? memberId)
        {
            throw new NotImplementedException();
        }

        public Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(Guid habitId, Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}