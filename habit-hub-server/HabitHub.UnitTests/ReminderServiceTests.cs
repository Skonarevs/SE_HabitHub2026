using HabitHub.API.Exceptions;
using HabitHub.API.Services;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HabitHub.UnitTests
{

    public class FakeEmailSenderForReminders : HabitHub.API.Services.IEmailSender
    {
        public List<(string Email, string Name, string Subject)> SentEmails { get; } = new();

        public Task SendEmailAsync(string toEmail, string toName, string subject, string message)
        {
            SentEmails.Add((toEmail, toName, subject));
            return Task.CompletedTask;
        }
    }

    public class ReminderServiceTests
    {
        //helpers

        private AppDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private (ReminderService service, FakeEmailSenderForReminders emailSender) CreateService(AppDbContext db)
        {
            var emailSender = new FakeEmailSenderForReminders();
            var service = new ReminderService(db, emailSender);
            return (service, emailSender);
        }

        /// <summary>
        /// Seeds a TeamCreator, Team, and an active Habit with a default reminder time.
        /// Also adds an active membership for the creator.
        /// </summary>
        private async Task<(Guid creatorId, Guid teamId, Guid habitId)> SeedTeamAndHabitAsync(
            AppDbContext db,
            TimeOnly? defaultReminderTime = null,
            HabitState state = HabitState.Active)
        {
            var creator = new TeamCreator
            {
                Id = Guid.NewGuid(),
                Name = "Creator",
                Email = $"{Guid.NewGuid()}@test.com",
                Timezone = "UTC",
                PasswordHash = "hash"
            };
            db.Users.Add(creator);

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                CreatorId = creator.Id
            };
            db.Teams.Add(team);

            db.Memberships.Add(new Membership
            {
                UserId = creator.Id,
                TeamId = team.Id,
                Status = MembershipStatus.Active
            });

            var habit = new Habit
            {
                Id = Guid.NewGuid(),
                Name = "Test Habit",
                Goal = "Stay consistent",
                Type = HabitType.Binary,
                Unit = "times",
                State = state,
                TeamId = team.Id,
                DefaultReminderTime = defaultReminderTime
            };
            db.Habits.Add(habit);

            await db.SaveChangesAsync();
            return (creator.Id, team.Id, habit.Id);
        }


        // Seeds a TeamMember with an active membership in the given team.
        private async Task<Guid> SeedMemberAsync(AppDbContext db, Guid teamId)
        {
            var member = new TeamMember
            {
                Id = Guid.NewGuid(),
                Name = "Member",
                Email = $"{Guid.NewGuid()}@test.com",
                Timezone = "UTC",
                PasswordHash = "hash"
            };
            db.Users.Add(member);

            db.Memberships.Add(new Membership
            {
                UserId = member.Id,
                TeamId = teamId,
                Status = MembershipStatus.Active
            });

            await db.SaveChangesAsync();
            return member.Id;
        }

        // SetReminderAsync -----------------------------------------------------------------------

        [Fact]
        public async Task SetReminder_ShouldThrowNotFound_WhenHabitDoesNotExist()
        {
            var db = CreateDb();
            var (service, _) = CreateService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.SetReminderAsync(Guid.NewGuid(), Guid.NewGuid(), "08:00:00"));
        }

        [Fact]
        public async Task SetReminder_ShouldThrowForbidden_WhenCallerIsNotCreator()
        {
            var db = CreateDb();
            var (creatorId, teamId, habitId) = await SeedTeamAndHabitAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);
            var (service, _) = CreateService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.SetReminderAsync(habitId, memberId, "08:00:00"));
        }

        [Fact]
        public async Task SetReminder_ShouldThrowValidation_WhenTimeFormatIsInvalid()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);
            var (service, _) = CreateService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.SetReminderAsync(habitId, creatorId, "not-a-time"));
        }

        [Fact]
        public async Task SetReminder_ShouldUpdateHabitDefaultReminderTime_WhenCreatorSets()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);
            var (service, _) = CreateService(db);

            await service.SetReminderAsync(habitId, creatorId, "08:30:00");

            var habit = await db.Habits.FindAsync(habitId);
            Assert.Equal(new TimeOnly(8, 30, 0), habit.DefaultReminderTime);
        }

        [Fact]
        public async Task SetReminder_ShouldUpdateExistingReminders_WhenReminderRecordsExist()
        {
            var db = CreateDb();
            var (creatorId, teamId, habitId) = await SeedTeamAndHabitAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);

            // Pre-seed a reminder record for the member
            db.Reminders.Add(new Reminder
            {
                HabitId = habitId,
                UserId = memberId,
                Enabled = true,
                Time = new TimeOnly(7, 0, 0) // old time
            });
            await db.SaveChangesAsync();

            var (service, _) = CreateService(db);
            await service.SetReminderAsync(habitId, creatorId, "09:00:00");

            var reminder = await db.Reminders.FirstAsync(r => r.HabitId == habitId && r.UserId == memberId);
            Assert.Equal(new TimeOnly(9, 0, 0), reminder.Time);
        }


        // ChangeReminderEnabledAsync -----------------------------------------------------------------------

        [Fact]
        public async Task ChangeReminderEnabled_ShouldThrowNotFound_WhenHabitDoesNotExist()
        {
            var db = CreateDb();
            var (service, _) = CreateService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.ChangeReminderEnabledAsync(Guid.NewGuid(), Guid.NewGuid(), true));
        }

        [Fact]
        public async Task ChangeReminderEnabled_ShouldThrowForbidden_WhenUserIsNotMemberOrCreator()
        {
            var db = CreateDb();
            var (_, _, habitId) = await SeedTeamAndHabitAsync(db);
            var (service, _) = CreateService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.ChangeReminderEnabledAsync(habitId, Guid.NewGuid(), true));
        }

        [Fact]
        public async Task ChangeReminderEnabled_ShouldCreateReminderRecord_WhenNoneExists()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);
            var (service, _) = CreateService(db);

            // No reminder record seeded — service should create one automatically
            await service.ChangeReminderEnabledAsync(habitId, creatorId, true);

            var reminder = await db.Reminders.FirstOrDefaultAsync(r =>
                r.HabitId == habitId && r.UserId == creatorId);
            Assert.NotNull(reminder);
        }

        [Fact]
        public async Task ChangeReminderEnabled_ShouldSetEnabledToTrue_WhenCalledWithTrue()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);

            db.Reminders.Add(new Reminder
            {
                HabitId = habitId,
                UserId = creatorId,
                Enabled = false,
                Time = new TimeOnly(8, 0, 0)
            });
            await db.SaveChangesAsync();

            var (service, _) = CreateService(db);
            await service.ChangeReminderEnabledAsync(habitId, creatorId, true);

            var reminder = await db.Reminders.FirstAsync(r =>
                r.HabitId == habitId && r.UserId == creatorId);
            Assert.True(reminder.Enabled);
        }

        [Fact]
        public async Task ChangeReminderEnabled_ShouldSetEnabledToFalse_WhenCalledWithFalse()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);

            db.Reminders.Add(new Reminder
            {
                HabitId = habitId,
                UserId = creatorId,
                Enabled = true,
                Time = new TimeOnly(8, 0, 0)
            });
            await db.SaveChangesAsync();

            var (service, _) = CreateService(db);
            await service.ChangeReminderEnabledAsync(habitId, creatorId, false);

            var reminder = await db.Reminders.FirstAsync(r =>
                r.HabitId == habitId && r.UserId == creatorId);
            Assert.False(reminder.Enabled);
        }

        [Fact]
        public async Task ChangeReminderEnabled_ShouldWork_WhenCallerIsActiveMember()
        {
            var db = CreateDb();
            var (_, teamId, habitId) = await SeedTeamAndHabitAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);
            var (service, _) = CreateService(db);

            // Should not throw — members are allowed to toggle their own reminders
            await service.ChangeReminderEnabledAsync(habitId, memberId, true);

            var reminder = await db.Reminders.FirstOrDefaultAsync(r =>
                r.HabitId == habitId && r.UserId == memberId);
            Assert.NotNull(reminder);
            Assert.True(reminder.Enabled);
        }



        // SendPendingRemindersAsync -----------------------------------------------------------------------


        [Fact]
        public async Task SendPendingReminders_ShouldNotSendEmail_WhenNoActiveHabitsWithReminders()
        {
            var db = CreateDb();
            var (service, emailSender) = CreateService(db);

            await service.SendPendingRemindersAsync();

            Assert.Empty(emailSender.SentEmails);
        }

        [Fact]
        public async Task SendPendingReminders_ShouldNotSendEmail_WhenHabitIsArchived()
        {
            var db = CreateDb();
            // Reminder time set to now so it would normally trigger
            var reminderTime = TimeOnly.FromDateTime(DateTime.UtcNow);
            var (_, _, _) = await SeedTeamAndHabitAsync(db,
                defaultReminderTime: reminderTime,
                state: HabitState.Archived);
            var (service, emailSender) = CreateService(db);

            await service.SendPendingRemindersAsync();

            Assert.Empty(emailSender.SentEmails);
        }

        [Fact]
        public async Task SendPendingReminders_ShouldNotSendEmail_WhenReminderIsDisabled()
        {
            var db = CreateDb();
            var reminderTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(-10));
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db,
                defaultReminderTime: reminderTime);

            // Reminder exists but is disabled
            db.Reminders.Add(new Reminder
            {
                HabitId = habitId,
                UserId = creatorId,
                Enabled = false,
                Time = reminderTime
            });
            await db.SaveChangesAsync();

            var (service, emailSender) = CreateService(db);
            await service.SendPendingRemindersAsync();

            Assert.Empty(emailSender.SentEmails);
        }

        [Fact]
        public async Task SendPendingReminders_ShouldNotSendEmail_WhenUserAlreadyLoggedToday()
        {
            var db = CreateDb();
            var reminderTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(-10));
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db,
                defaultReminderTime: reminderTime);

            // User already logged today
            db.HabitEntries.Add(new HabitEntry
            {
                Id = Guid.NewGuid(),
                HabitId = habitId,
                UserId = creatorId,
                Date = DateTime.UtcNow.Date,
                Status = EntryStatus.Logged,
                Notes = ""
            });
            await db.SaveChangesAsync();

            var (service, emailSender) = CreateService(db);
            await service.SendPendingRemindersAsync();

            Assert.Empty(emailSender.SentEmails);
        }

        [Fact]
        public async Task SendPendingReminders_ShouldNotSendEmail_WhenReminderTimeIsInTheFuture()
        {
            var db = CreateDb();
            // Reminder time is 1 hour from now — should not trigger yet
            var reminderTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1));
            var (_, _, _) = await SeedTeamAndHabitAsync(db, defaultReminderTime: reminderTime);
            var (service, emailSender) = CreateService(db);

            await service.SendPendingRemindersAsync();

            Assert.Empty(emailSender.SentEmails);
        }

        [Fact]
        public async Task SendPendingReminders_ShouldSendEmail_WhenReminderTimeIsWithinLastHour()
        {
            var db = CreateDb();
            // Reminder time was 10 minutes ago — within the 1-hour window
            var reminderTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(-10));
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db,
                defaultReminderTime: reminderTime);

            var (service, emailSender) = CreateService(db);
            await service.SendPendingRemindersAsync();

            Assert.Single(emailSender.SentEmails);
            Assert.Equal("Creator", emailSender.SentEmails[0].Name);
        }

        [Fact]
        public async Task SendPendingReminders_ShouldSendEmailToAllMembers_WhenMultipleMembersHaveRemindersEnabled()
        {
            var db = CreateDb();
            var reminderTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(-10));
            var (creatorId, teamId, habitId) = await SeedTeamAndHabitAsync(db,
                defaultReminderTime: reminderTime);
            var memberId = await SeedMemberAsync(db, teamId);
            var (service, emailSender) = CreateService(db);

            await service.SendPendingRemindersAsync();

            // Both creator and member should get emails
            Assert.Equal(2, emailSender.SentEmails.Count);
        }

        [Fact]
        public async Task SendPendingReminders_ShouldNotSendEmail_WhenReminderTimeIsMoreThanOneHourAgo()
        {
            var db = CreateDb();
            // Reminder was 2 hours ago — outside the window
            var reminderTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(-2));
            var (_, _, _) = await SeedTeamAndHabitAsync(db, defaultReminderTime: reminderTime);
            var (service, emailSender) = CreateService(db);

            await service.SendPendingRemindersAsync();

            Assert.Empty(emailSender.SentEmails);
        }

        [Fact]
        public async Task SendPendingReminders_ShouldNotSendEmail_WhenHabitHasExpired()
        {
            var db = CreateDb();
            var reminderTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(-10));
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db,
                defaultReminderTime: reminderTime);

            // Set expiry to yesterday
            var habit = await db.Habits.FindAsync(habitId);
            habit.ExpiryDate = DateTime.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();

            var (service, emailSender) = CreateService(db);
            await service.SendPendingRemindersAsync();

            Assert.Empty(emailSender.SentEmails);
        }
    }
}