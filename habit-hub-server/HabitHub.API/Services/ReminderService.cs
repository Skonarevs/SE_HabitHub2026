using HabitHub.API.Models.DTOs;
using HabitHub.API.Services.Interfaces;
using HabitHub.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HabitHub.API.Services
{
    public class ReminderService : IReminderService
    {
        private readonly AppDbContext _context;

        public ReminderService(AppDbContext context)
        {
            _context = context;
        }

        public Task SetReminderAsync(Guid habitId, Guid userId, string reminderTime)
        {
            throw new NotImplementedException();
        }

        public Task ChangeReminderEnabledAsync(Guid habitId, Guid userId, bool enabled)
        {
            throw new NotImplementedException();
        }
    }
}