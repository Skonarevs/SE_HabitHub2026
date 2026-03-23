using HabitHub.API.Models.DTOs;
using HabitHub.API.Services.Interfaces;
using HabitHub.Data;
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

        public Task<TeamResponseDto> CreateTeamAsync(Guid creatorId, string teamName)
        {
            throw new NotImplementedException();
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