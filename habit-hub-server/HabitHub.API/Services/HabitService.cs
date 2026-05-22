using HabitHub.API.Models.DTOs;
using HabitHub.API.Services.Interfaces;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HabitHub.API.Exceptions;

namespace HabitHub.API.Services
{
    public class HabitService : IHabitService
    {
        private readonly AppDbContext _context;

        public HabitService(AppDbContext context)
        {
            _context = context;
        }

        private async Task<Habit> GetHabitWithTeamAsync(Guid habitId, Guid userId, bool requireCreator = false)
        {
            var habit = await _context.Habits
                .Include(h => h.Team)
                .FirstOrDefaultAsync(h => h.Id == habitId);
            if (habit == null)
                throw new NotFoundException("Habit not found");

            var team = habit.Team;
            if (requireCreator)
            {
                if (team.CreatorId != userId)
                    throw new ForbiddenException("Only team creator can perform this action");
            }
            else
            {
                var isMember = await _context.Memberships
                    .AnyAsync(m => m.TeamId == team.Id && m.UserId == userId && m.Status == MembershipStatus.Active);
                if (team.CreatorId != userId && !isMember)
                    throw new ForbiddenException("You do not have access to this habit");
            }
            return habit;
        }

        public async Task<HabitResponseDto> UpdateHabitAsync(Guid habitId, UpdateHabitDto dto, Guid userId)
        {
            var habit = await GetHabitWithTeamAsync(habitId, userId, requireCreator: true);

            if (!string.IsNullOrWhiteSpace(dto.Name))
                habit.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Goal))
                habit.Goal = dto.Goal;
            if (!string.IsNullOrWhiteSpace(dto.HabitType))
            {
                if (!Enum.TryParse<HabitType>(dto.HabitType, true, out var type))
                    throw new ValidationException("Invalid habit type");
                habit.Type = type;
            }
            if (dto.Unit != null)
                habit.Unit = dto.Unit;
            if (dto.ExpiryDate.HasValue)
            {
                if (dto.ExpiryDate.Value <= DateTime.UtcNow)
                    throw new ValidationException("Expiry date must be in the future");
                habit.ExpiryDate = dto.ExpiryDate;
            }

            await _context.SaveChangesAsync();

            return MapToHabitResponse(habit);
        }

        public async Task ArchiveHabitAsync(Guid habitId, Guid userId)
        {
            var habit = await GetHabitWithTeamAsync(habitId, userId, requireCreator: true);
            if (habit.State == HabitState.Archived)
                return; 
            if (habit.State == HabitState.Closed)
                throw new ConflictException("Habit is already closed");

            habit.State = HabitState.Archived;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteHabitAsync(Guid habitId, Guid userId)
        {
            var habit = await GetHabitWithTeamAsync(habitId, userId, requireCreator: true);

            _context.HabitEntries.RemoveRange(habit.Entries);
            _context.Reminders.RemoveRange(habit.Reminders);
            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();
        }

        public async Task<HabitEntryResponseDto> LogProgressAsync(Guid habitId, Guid userId, LogProgressDto dto)
        {
            var habit = await GetHabitWithTeamAsync(habitId, userId, requireCreator: false);
            if (habit.State != HabitState.Active)
                throw new ConflictException("Habit is not active");


            var today = DateTime.UtcNow.Date;
            var existing = await _context.HabitEntries
                .FirstOrDefaultAsync(e => e.HabitId == habitId && e.UserId == userId && e.Date == today);
            if (existing != null)
                throw new ConflictException("Already logged for today");

            EntryStatus status;
            if (!Enum.TryParse<EntryStatus>(dto.Status, true, out status))
                throw new ValidationException("Invalid status");
            if (status != EntryStatus.Logged && status != EntryStatus.Skipped)
                throw new ValidationException("Status must be Logged or Skipped");

            float? value = null;
            if (habit.Type == HabitType.Quantitative)
            {
                if (dto.Value == null)
                    throw new ValidationException("Value is required for quantitative habit");
                value = dto.Value;
            }
            else 
            {
                if (dto.Value != null)
                    throw new ValidationException("Value should not be provided for binary habit");
            }

            var entry = new HabitEntry
            {
                HabitId = habitId,
                UserId = userId,
                Date = today,
                Value = value,
                Status = status,
                Notes = dto.Notes
            };
            _context.HabitEntries.Add(entry);
            await _context.SaveChangesAsync();

            return MapToEntryResponse(entry);
        }

        public async Task UndoLogAsync(Guid habitId, Guid entryId, Guid userId)
        {
            var habit = await GetHabitWithTeamAsync(habitId, userId, requireCreator: false);
            if (habit.State != HabitState.Active)
                throw new ConflictException("Cannot undo log for inactive habit");

            var entry = await _context.HabitEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.HabitId == habitId && e.UserId == userId);
            if (entry == null)
                throw new NotFoundException("Log entry not found");

            _context.HabitEntries.Remove(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<HabitEntryResponseDto>> GetProgressAsync(Guid habitId, Guid userId, Guid? memberId)
        {
            var habit = await GetHabitWithTeamAsync(habitId, userId, requireCreator: false);
            var isCreator = habit.Team.CreatorId == userId;


            if (isCreator && memberId == null)
            {
                var allEntries = await _context.HabitEntries
                    .Include(e => e.Habit)
                    .Include(e => e.User)
                    .Where(e => e.HabitId == habitId)
                    .OrderBy(e => e.Date)
                    .ToListAsync();
                return allEntries.Select(e => MapToEntryResponse(e)).ToList();
            }

            var targetUserId = memberId ?? userId;

            if (targetUserId != userId)
            {
                if (!isCreator)
                    throw new ForbiddenException("You can only view your own progress unless you are team creator");
            }

            var entries = await _context.HabitEntries
                .Include(e => e.Habit)
                .Include(e => e.User)
                .Where(e => e.HabitId == habitId && e.UserId == targetUserId)
                .OrderBy(e => e.Date)
                .ToListAsync();
            return entries.Select(e => MapToEntryResponse(e)).ToList();
        }

        public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(Guid habitId, Guid userId)
        {
            var habit = await GetHabitWithTeamAsync(habitId, userId, requireCreator: false);
            var teamId = habit.TeamId;

            var members = await _context.Memberships
                .Where(m => m.TeamId == teamId && m.Status == MembershipStatus.Active)
                .Select(m => m.UserId)
                .ToListAsync();

            var entries = await _context.HabitEntries
                .Where(e => e.HabitId == habitId && members.Contains(e.UserId) && e.Status == EntryStatus.Logged)
                .ToListAsync();

            var userNames = await _context.Users
                .Where(u => members.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            var leaderboard = members.Select(memberId =>
            {
                var memberEntries = entries.Where(e => e.UserId == memberId);
                double? progress = habit.Type == HabitType.Binary
                    ? memberEntries.Count()
                    : memberEntries.Sum(e => e.Value);
                return new LeaderboardEntryDto
                {
                    UserId = memberId,
                    UserName = userNames.GetValueOrDefault(memberId) ?? "Unknown",
                    Progress = (float?)progress
                };
            }).OrderByDescending(l => l.Progress).ToList();

            return leaderboard;
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
                TeamName = habit.Team?.Name,
                ReminderTime = habit.DefaultReminderTime?.ToString("HH:mm:ss")
            };
        }

        private HabitEntryResponseDto MapToEntryResponse(HabitEntry entry)
        {
            return new HabitEntryResponseDto
            {
                Id = entry.Id,
                HabitId = entry.HabitId,
                HabitName = entry.Habit?.Name,
                UserId = entry.UserId,
                UserName = entry.User?.Name,
                Date = entry.Date,
                Value = entry.Value,
                Status = entry.Status.ToString(),
                Notes = entry.Notes
            };
        }

        public async Task<List<HabitResponseDto>> GetUserHabitsAsync(Guid userId)
        {
            var teamIds = await _context.Memberships
                .Where(m => m.UserId == userId && m.Status == MembershipStatus.Active)
                .Select(m => m.TeamId)
                .Union(_context.Teams.Where(t => t.CreatorId == userId).Select(t => t.Id))
                .Distinct()
                .ToListAsync();

            var habits = await _context.Habits
                .Where(h => teamIds.Contains(h.TeamId) && h.State == HabitState.Active)
                .Select(h => MapToHabitResponse(h))
                .ToListAsync();

            return habits;
        }
    }
}