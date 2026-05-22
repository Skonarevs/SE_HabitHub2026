using HabitHub.API.Exceptions;
using HabitHub.API.Models.DTOs;
using HabitHub.API.Services.Interfaces;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HabitHub.API.Services
{
    public class ReminderService : IReminderService
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;

        public ReminderService(AppDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task SetReminderAsync(Guid habitId, Guid userId, string reminderTime)
        {
            var habit = await _context.Habits
                .Include(h => h.Team)
                .FirstOrDefaultAsync(h => h.Id == habitId);
            if (habit == null)
            {
                throw new NotFoundException("Habit not found");
            }
            if (habit.Team.CreatorId != userId)
            {
                throw new ForbiddenException("Only team creator can set reminder time");
            }
                
            if (!TimeOnly.TryParse(reminderTime, out var time))
            {
                throw new ValidationException("Invalid reminder time format. Use HH:mm:ss");
            }
                
            habit.DefaultReminderTime = time;

            var reminders = await _context.Reminders
                .Where(r => r.HabitId == habitId)
                .ToListAsync();
            foreach (var reminder in reminders)
            {
                reminder.Time = time;
            }

            await _context.SaveChangesAsync();
        }

        private async Task EnsureReminderRecordExistsAsync(Guid habitId, Guid userId)
        {
            var exists = await _context.Reminders.AnyAsync(r => r.HabitId == habitId && r.UserId == userId);
            if (exists)
            {
                return;
            }

            var habit = await _context.Habits.FindAsync(habitId);
            if (habit == null)
            {
                throw new NotFoundException("Habit not found");
            }

            var reminder = new Reminder
            {
                HabitId = habitId,
                UserId = userId,
                Enabled = true,   
                Time = habit.DefaultReminderTime 
            };
            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeReminderEnabledAsync(Guid habitId, Guid userId, bool enabled)
        {
            var habit = await _context.Habits
                .Include(h => h.Team)
                .FirstOrDefaultAsync(h => h.Id == habitId);
            if (habit == null)
            {
                throw new NotFoundException("Habit not found");
            }
            var team = habit.Team;
            var isCreator = team.CreatorId == userId;
            var isActiveMember = await _context.Memberships
                .AnyAsync(m => m.TeamId == team.Id && m.UserId == userId && m.Status == MembershipStatus.Active);
            if (!isCreator && !isActiveMember)
            {
                throw new ForbiddenException("You are not a member of this team");
            }
            
            await EnsureReminderRecordExistsAsync(habitId, userId);

            var reminder = await _context.Reminders
                .FirstAsync(r => r.HabitId == habitId && r.UserId == userId);
            reminder.Enabled = enabled;
            await _context.SaveChangesAsync();
        }

        public async Task SendPendingRemindersAsync()
        {
            var now = DateTime.UtcNow;
            var currentTime = TimeOnly.FromDateTime(now);
            var habits = await _context.Habits
                .Include(h => h.Team)
                .Where(h => h.State == HabitState.Active
                            && h.DefaultReminderTime != null
                            && (!h.ExpiryDate.HasValue || h.ExpiryDate >= now.Date))
                .ToListAsync();

            foreach (var habit in habits)
            {

                if (habit.DefaultReminderTime > currentTime)
                {
                    continue;
                }
                if (habit.DefaultReminderTime < currentTime && (currentTime - habit.DefaultReminderTime.Value).TotalHours > 1)
                {
                    continue; // Only send within 1 hour after the reminder 
                }

                var members = await _context.Memberships
                    .Where(m => m.TeamId == habit.TeamId && m.Status == MembershipStatus.Active)
                    .Select(m => m.UserId)
                    .ToListAsync();

                foreach (var memberId in members)
                {
                    await EnsureReminderRecordExistsAsync(habit.Id, memberId);
                    var reminder = await _context.Reminders
                        .FirstAsync(r => r.HabitId == habit.Id && r.UserId == memberId);

                    if (!reminder.Enabled)
                    {
                        continue;
                    }

                    var alreadyLogged = await _context.HabitEntries
                        .AnyAsync(e => e.HabitId == habit.Id && e.UserId == memberId && e.Date == now.Date);
                    if (alreadyLogged)
                    {
                        continue;
                    }

                    var user = await _context.Users.FindAsync(memberId);
                    if (user != null)
                    {
                        await SendReminderEmailAsync(user.Email, user.Name, habit.Name);
                    }
                }
            }
        }


        private async Task SendReminderEmailAsync(string email, string userName, string habitName)
        {
            var subject = $"HabitHub Reminder: Time to log '{habitName}'!";
            var message = $"Hello {userName},\n\nThis is a friendly reminder to log your progress for the habit '{habitName}' today.\n\nKeep up the great work!\n\n- The HabitHub Team";

            await _emailSender.SendEmailAsync(email, userName, subject, message);
        }


        public async Task<List<UserReminderDto>> GetUserRemindersAsync(Guid userId)
        {
            // Left join: all active habits from teams where user is active member or creator, with optional reminder record for that user.
            var reminders = await (from habit in _context.Habits
                                   join team in _context.Teams on habit.TeamId equals team.Id
                                   join reminder in _context.Reminders
                                       on new { HabitId = habit.Id, UserId = userId }
                                       equals new { reminder.HabitId, reminder.UserId }
                                       into reminderJoin
                                   from reminder in reminderJoin.DefaultIfEmpty()
                                   where habit.State == HabitState.Active
                                         && (team.CreatorId == userId
                                             || _context.Memberships.Any(m => m.TeamId == team.Id && m.UserId == userId && m.Status == MembershipStatus.Active))
                                   select new UserReminderDto
                                   {
                                       HabitId = habit.Id,
                                       HabitName = habit.Name,
                                       TeamId = team.Id,
                                       TeamName = team.Name,
                                       DefaultReminderTime = habit.DefaultReminderTime,
                                       Enabled = reminder != null ? reminder.Enabled : true  
                                   }).ToListAsync();

            return reminders;
        }


    }
}