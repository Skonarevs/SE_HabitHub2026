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


            if (dto.ExpiryDate.HasValue && dto.ExpiryDate.Value <= DateTime.UtcNow)
                throw new ValidationException("Expiry date must be in the future");

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

            return MapToHabitResponse(habit);
        }

        public async Task<TeamResponseDto> CreateTeamAsync(Guid creatorId, string teamName)
        {
            var creator = await _context.Users.OfType<TeamCreator>().FirstOrDefaultAsync(u => u.Id == creatorId);
            if (creator == null)
                throw new ForbiddenException("Only team creators can create teams");

            if (string.IsNullOrWhiteSpace(teamName))
                throw new ValidationException("Team name is required");

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = teamName,
                CreatorId = creatorId,
                Chat = new TeamChat()
            };
            _context.Teams.Add(team);
            var membership = new Membership
            {
                UserId = creatorId,
                TeamId = team.Id,
                Status = MembershipStatus.Active,
                JoinedAt = DateTime.UtcNow
            };
            _context.Memberships.Add(membership);

            await _context.SaveChangesAsync();
            var code = GenerateRandomCode(8);
            var inviteCode = new InviteCode
            {
                Id = Guid.NewGuid(),
                Code = code,
                TeamId = team.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(10),
                State = InviteCodeState.Active
            };
            _context.InviteCodes.Add(inviteCode);
            await _context.SaveChangesAsync();

            return MapToTeamResponse(team, code);
        }

        public async Task<TeamResponseDto> GetTeamAsync(Guid teamId, Guid userId)
        {
            var team = await _context.Teams
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null)
            {
                throw new NotFoundException("Team not found");
            }
                
            bool isCreator = (team.CreatorId == userId);
            bool isActiveMember = await _context.Memberships
                .AnyAsync(m => m.TeamId == teamId && m.UserId == userId && m.Status == MembershipStatus.Active);
            if (!isCreator && !isActiveMember)
            {
                throw new ForbiddenException("You do not have access to this team");
            }
                
            var inviteCode = await _context.InviteCodes
                .Where(ic => ic.TeamId == teamId && ic.State == InviteCodeState.Active)
                .FirstOrDefaultAsync();

            string codeValue = inviteCode?.Code; 

            return MapToTeamResponse(team, codeValue);
        }

        public async Task<InviteCodeResponseDto> GenerateInviteCodeAsync(Guid teamId, Guid creatorId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                throw new NotFoundException("Team not found");
            }

            if (team.CreatorId != creatorId)
            {
                throw new ForbiddenException("Only team creator can generate invite codes");
            }

            var code = GenerateRandomCode(8);

            var inviteCode = new InviteCode
            {
                Id = Guid.NewGuid(),
                Code = code,
                TeamId = teamId,
                ExpiryDate = DateTime.UtcNow.AddDays(10),
                State = InviteCodeState.Active
            };
            _context.InviteCodes.Add(inviteCode);
            await _context.SaveChangesAsync();

            return new InviteCodeResponseDto
            {
                Code = inviteCode.Code,
                ExpiryDate = inviteCode.ExpiryDate
            };
        }

        public async Task<MembershipResponseDto> JoinTeamAsync(Guid userId, string inviteCode)
        {
            var code = await _context.InviteCodes
                .Include(ic => ic.Team)
                .FirstOrDefaultAsync(ic => ic.Code == inviteCode && ic.State == InviteCodeState.Active);
            if (code == null || code.ExpiryDate < DateTime.UtcNow)
            {
                throw new NotFoundException("Invite code not found or invalid");
            }
               
            var team = code.Team;

            var existing = await _context.Memberships
                .FirstOrDefaultAsync(m => m.TeamId == team.Id && m.UserId == userId);
            if (existing != null)
            {
                if (existing.Status == MembershipStatus.Active)
                {
                    throw new ConflictException("Already a member of this team");
                }
            }

            var membership = new Membership
            {
                UserId = userId,
                TeamId = team.Id,
                Status = MembershipStatus.Active,
                JoinedAt = DateTime.UtcNow
            };
            _context.Memberships.Add(membership);
            await _context.SaveChangesAsync();

            return new MembershipResponseDto
            {
                TeamId = team.Id,
                TeamName = team.Name,
                Status = membership.Status.ToString()
            };
        }

        public async Task KickMemberAsync(Guid teamId, Guid memberId, Guid requesterId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                throw new NotFoundException("Team not found");
            }
            if (team.CreatorId != requesterId)
            {
                throw new ForbiddenException("Only team creator can kick members");
            }
                
            if (memberId == requesterId)
            {
                throw new ConflictException("Cannot kick yourself");
            }
                

            var membership = await _context.Memberships
                .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == memberId && m.Status == MembershipStatus.Active);
            if (membership == null)
            {
                throw new NotFoundException("User is not an active member of this team");
            }
                

            membership.Status = MembershipStatus.Kicked;
            membership.LeftAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task LeaveTeamAsync(Guid teamId, Guid userId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                throw new NotFoundException("Team not found");
            }
                
            if (team.CreatorId == userId)
            {
                throw new ConflictException("Team creator cannot leave; delete team instead");
            }
                

            var membership = await _context.Memberships
                .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId && m.Status == MembershipStatus.Active);
            if (membership == null)
            {
                throw new NotFoundException("You are not an active member of this team");
            }
               

            membership.Status = MembershipStatus.Left;
            membership.LeftAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTeamAsync(Guid teamId, Guid creatorId)
        {
            var team = await _context.Teams
                .Include(t => t.Habits)
                .Include(t => t.Memberships)
                .Include(t => t.Chat)
                .FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null)
            {
                throw new NotFoundException("Team not found");
            }
               
            if (team.CreatorId != creatorId)
            {
                throw new ForbiddenException("Only team creator can delete the team");
            }
               
            // Remove related data (manual deletion, nt cascade just in case)
            _context.HabitEntries.RemoveRange(_context.HabitEntries.Where(e => team.Habits.Select(h => h.Id).Contains(e.HabitId)));
            _context.Habits.RemoveRange(team.Habits);
            _context.Messages.RemoveRange(_context.Messages.Where(m => m.ChatId == team.Chat.Id));
            _context.TeamChats.Remove(team.Chat);
            _context.Memberships.RemoveRange(team.Memberships);
            _context.InviteCodes.RemoveRange(_context.InviteCodes.Where(ic => ic.TeamId == teamId));
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }

        public async Task<List<HabitResponseDto>> GetActiveHabitsAsync(Guid teamId, Guid userId)
        {
            var team = await _context.Teams.FindAsync(teamId);
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

            return await _context.Habits
                .Where(h => h.TeamId == teamId && h.State == HabitState.Active)
                .Select(h => new HabitResponseDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Goal = h.Goal,
                    HabitType = h.Type.ToString(),
                    Unit = h.Unit,
                    ExpiryDate = h.ExpiryDate,
                    State = h.State.ToString(),
                    TeamId = h.TeamId,
                    TeamName = h.Team != null ? h.Team.Name : null
                })
                .ToListAsync();
        }

        public async Task<List<ArchivedHabitDto>> GetArchivedHabitsAsync(Guid teamId, Guid userId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                throw new NotFoundException("Team not found");
            }

            bool isMember = await _context.Memberships
                .AnyAsync(m => m.TeamId == teamId && m.UserId == userId && m.Status == MembershipStatus.Active);
            if (team.CreatorId != userId || !isMember)
            {
                throw new ForbiddenException("You are not a member of this team");
            }
                

            var habits = await _context.Habits
                .Where(h => h.TeamId == teamId && h.State == HabitState.Archived)
                .Select(h => new ArchivedHabitDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Goal = h.Goal,
                    HabitType = h.Type.ToString(),
                    Unit = h.Unit,
                    ExpiryDate = h.ExpiryDate,
                    ArchivedAt = DateTime.UtcNow
                })
                .ToListAsync();
            return habits;
        }

        private HabitResponseDto MapToHabitResponse(Habit habit)
        {
            return new HabitResponseDto
            {
                Id = habit.Id,
                Name = habit.Name,
                Goal = habit.Goal,
                HabitType = habit.Type.ToString(),
                Unit = habit.Unit,
                ExpiryDate = habit.ExpiryDate,
                State = habit.State.ToString(),
                TeamId = habit.TeamId,
                TeamName = habit.Team?.Name
            };
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var buffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = chars[random.Next(chars.Length)];
            }
            return new string(buffer);
        }


        private TeamResponseDto MapToTeamResponse(Team team, string code)
        {
            return new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                CreatorId = team.CreatorId,
                InviteCode = code
            };
        }


        public async Task<List<TeamResponseDto>> GetTeamsForUserAsync(Guid userId)
        {
            var memberships = await _context.Memberships
                .Where(m => m.UserId == userId && m.Status == MembershipStatus.Active)
                .Include(m => m.Team)
                .ToListAsync();

            var teamIds = memberships.Select(m => m.Team.Id).ToList();
            var activeCodes = await _context.InviteCodes
                .Where(ic => teamIds.Contains(ic.TeamId) && ic.State == InviteCodeState.Active && ic.ExpiryDate > DateTime.UtcNow)
                .GroupBy(ic => ic.TeamId)
                .Select(g => new { TeamId = g.Key, Code = g.First().Code })
                .ToDictionaryAsync(k => k.TeamId, v => v.Code);

            return memberships.Select(m => new TeamResponseDto
            {
                Id = m.Team.Id,
                Name = m.Team.Name,
                CreatorId = m.Team.CreatorId,
                InviteCode = activeCodes.ContainsKey(m.Team.Id) ? activeCodes[m.Team.Id] : null
            }).ToList();
        }

        public async Task<List<TeamMemberResponseDto>> GetTeamMembersAsync(Guid teamId, Guid requesterId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                throw new NotFoundException("Team not found");
            }
                

            if (team.CreatorId != requesterId)
            {
                throw new ForbiddenException("Only the team creator can view team members");
            }
                

            var members = await _context.Memberships
                .Where(m => m.TeamId == teamId && m.Status == MembershipStatus.Active)
                .Include(m => m.User)
                .Select(m => new TeamMemberResponseDto
                {
                    UserId = m.User.Id,
                    Name = m.User.Name,
                    Email = m.User.Email,
                    JoinedAt = m.JoinedAt,
                    Status = m.Status.ToString()
                })
                .ToListAsync();

            return members;
        }

        public async Task<List<HabitResponseDto>> GetTeamHabitsAsync(Guid teamId, Guid userId, string? state)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                throw new NotFoundException("Team not found");
            }
            bool isMember = await _context.Memberships.AnyAsync(m => m.TeamId == teamId && m.UserId == userId && m.Status == MembershipStatus.Active);
            if (team.CreatorId != userId && !isMember)
            {
                throw new ForbiddenException("Access denied");
            }

            var query = _context.Habits.Where(h => h.TeamId == teamId);
            if (!string.IsNullOrEmpty(state) && Enum.TryParse<HabitState>(state, true, out var habitState))
            {
                query = query.Where(h => h.State == habitState);
            }
                

            var habits = await query.Select(h => MapToHabitResponse(h)).ToListAsync();
            return habits;
        }


    }
}