using HabitHub.API.Models.DTOs;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace HabitHub.ApiTests
{
    public class RemindersControllerApiTests : AuthTestBase
    {
        public RemindersControllerApiTests(WebApplicationFactory<Program> factory) : base(factory) { }


        // Seed helpers


        // Seeds a TeamCreator with session, team, and active habit in one DB scope.
        private async Task<(Guid userId, Guid sessionId, Guid teamId, Guid habitId)> SeedFullSetupAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = new TeamCreator
            {
                Id = Guid.NewGuid(),
                Name = "Creator",
                Email = $"{Guid.NewGuid()}@test.com",
                Timezone = "UTC",
                PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>()
                    .HashPassword(null, "Password123!")
            };
            db.Users.Add(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = SessionStatus.Active
            };
            db.Sessions.Add(session);

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                CreatorId = user.Id,
                Chat = new TeamChat()
            };
            db.Teams.Add(team);

            db.Memberships.Add(new Membership
            {
                UserId = user.Id,
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
                State = HabitState.Active,
                TeamId = team.Id
            };
            db.Habits.Add(habit);

            await db.SaveChangesAsync();
            return (user.Id, session.Id, team.Id, habit.Id);
        }

        // Seeds a TeamMember with session and active membership in the given team.
        private async Task<(Guid memberId, Guid memberSessionId)> SeedMemberWithSessionAsync(Guid teamId)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var member = new TeamMember
            {
                Id = Guid.NewGuid(),
                Name = "Member",
                Email = $"{Guid.NewGuid()}@test.com",
                Timezone = "UTC",
                PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>()
                    .HashPassword(null, "Password123!")
            };
            db.Users.Add(member);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = member.Id,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = SessionStatus.Active
            };
            db.Sessions.Add(session);

            db.Memberships.Add(new Membership
            {
                UserId = member.Id,
                TeamId = teamId,
                Status = MembershipStatus.Active
            });

            await db.SaveChangesAsync();
            return (member.Id, session.Id);
        }

        // -----------------------------------------------------------------------
        // SetReminder PATCH /habits/{habitId}/reminder
        // -----------------------------------------------------------------------

        [Fact]
        public async Task SetReminder_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, _, habitId) = await SeedFullSetupAsync();

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}/reminder",
                new SetReminderDto { ReminderTime = "08:00:00" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SetReminder_ShouldReturn200_WhenCreatorSetsValidTime()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}/reminder",
                new SetReminderDto { ReminderTime = "08:00:00" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SetReminder_ShouldReturn403_WhenMemberTriesToSetReminder()
        {
            var (_, _, teamId, habitId) = await SeedFullSetupAsync();
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}/reminder",
                new SetReminderDto { ReminderTime = "08:00:00" });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SetReminder_ShouldReturn404_WhenHabitDoesNotExist()
        {
            var (userId, sessionId, _, _) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{Guid.NewGuid()}/reminder",
                new SetReminderDto { ReminderTime = "08:00:00" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SetReminder_ShouldReturn400_WhenTimeFormatIsInvalid()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}/reminder",
                new SetReminderDto { ReminderTime = "not-a-time" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }





        // ChangeReminderEnabled PATCH /habits/{habitId}/reminder/enabled


        [Fact]
        public async Task ChangeReminderEnabled_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, _, habitId) = await SeedFullSetupAsync();

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}/reminder/enabled",
                new ChangeReminderDto { Enabled = true });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ChangeReminderEnabled_ShouldReturn200_WhenCreatorEnablesReminder()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}/reminder/enabled",
                new ChangeReminderDto { Enabled = true });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ChangeReminderEnabled_ShouldReturn200_WhenCreatorDisablesReminder()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}/reminder/enabled",
                new ChangeReminderDto { Enabled = false });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ChangeReminderEnabled_ShouldReturn200_WhenMemberTogglesOwnReminder()
        {
            var (_, _, teamId, habitId) = await SeedFullSetupAsync();
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}/reminder/enabled",
                new ChangeReminderDto { Enabled = true });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ChangeReminderEnabled_ShouldReturn403_WhenUserIsNotMember()
        {
            var (_, _, _, habitId) = await SeedFullSetupAsync();
            // Seed a user with no membership in this team
            var (outsiderId, outsiderSessionId) = await SeedMemberWithSessionAsync(Guid.NewGuid());
            var client = CreateAuthenticatedClient(outsiderId, "test@test.com", outsiderSessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}/reminder/enabled",
                new ChangeReminderDto { Enabled = true });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ChangeReminderEnabled_ShouldReturn404_WhenHabitDoesNotExist()
        {
            var (userId, sessionId, _, _) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{Guid.NewGuid()}/reminder/enabled",
                new ChangeReminderDto { Enabled = true });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}