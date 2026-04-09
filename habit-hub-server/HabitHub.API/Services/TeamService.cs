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
    public class TeamService : ITeamService
    {
        private readonly AppDbContext _context;

        public TeamService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HabitResponseDto> CreateHabitAsync(Guid teamId, CreateHabitDto dto, Guid creatorId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                throw new NotFoundException("Team not found");
            }
                
            if (team.CreatorId != creatorId)
            {
                throw new ForbiddenException("Only team creator can create habits");
            }
               
            if (!Enum.TryParse<HabitType>(dto.HabitType, true, out var habitType))
            {
                throw new ValidationException("Invalid habit type. Must be 'Binary' or 'Quantitative'");
            }
                
            if (habitType == HabitType.Quantitative && string.IsNullOrWhiteSpace(dto.Unit))
            {
                throw new ValidationException("Unit is required for quantitative habits");
            }
                

            // 4. Validate expiry date if provided
            if (dto.ExpiryDate.HasValue && dto.ExpiryDate.Value <= DateTime.UtcNow)
                throw new ValidationException("Expiry date must be in the future");

            // 5. Create habit entity
            var habit = new Habit
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Goal = dto.Goal,
                Type = habitType,
                Unit = dto.Unit,
                ExpiryDate = dto.ExpiryDate,
                State = HabitState.Active,
                TeamId = teamId
            };

            _context.Habits.Add(habit);
            await _context.SaveChangesAsync();

            // 6. Return DTO
            return MapToHabitResponse(habit);
        }

        public Task<TeamResponseDto> GetTeamAsync(Guid teamId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<InviteCodeResponseDto> GenerateInviteCodeAsync(Guid teamId, Guid creatorId)
        {
            throw new NotImplementedException();
        }

        public Task<MembershipResponseDto> JoinTeamAsync(Guid userId, string inviteCode)
        {
            throw new NotImplementedException();
        }

        public Task KickMemberAsync(Guid teamId, Guid memberId, Guid requesterId)
        {
            throw new NotImplementedException();
        }

        public Task LeaveTeamAsync(Guid teamId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteTeamAsync(Guid teamId, Guid creatorId)
        {
            throw new NotImplementedException();
        }

        public Task<HabitResponseDto> CreateHabitAsync(Guid teamId, CreateHabitDto dto, Guid creatorId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ArchivedHabitDto>> GetArchivedHabitsAsync(Guid teamId, Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}