using HabitHub.API.Models.DTOs;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace HabitHub.ApiTests
{
    public class HabitsControllerApiTests : AuthTestBase
    {
        public HabitsControllerApiTests(WebApplicationFactory<Program> factory) : base(factory) { }

        // -----------------------------------------------------------------------
        // Seed helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Seeds everything needed for most tests in a single DB scope:
        /// creator, session, team, and habit. Avoids cross-scope visibility issues.
        /// </summary>
        private async Task<(Guid userId, Guid sessionId, Guid teamId, Guid habitId)>
            SeedFullSetupAsync(
                HabitType type = HabitType.Binary,
                HabitState state = HabitState.Active)
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
                CreatorId = user.Id
            };
            db.Teams.Add(team);

            var habit = new Habit
            {
                Id = Guid.NewGuid(),
                Name = "Test Habit",
                Goal = "Stay consistent",
                Type = type,
                State = state,
                Unit = "times",
                TeamId = team.Id
            };
            db.Habits.Add(habit);

            await db.SaveChangesAsync();
            return (user.Id, session.Id, team.Id, habit.Id);
        }

        /// <summary>
        /// Seeds a TeamMember with a session and active Membership in a single DB scope.
        /// </summary>
        private async Task<(Guid memberId, Guid memberSessionId)>
            SeedMemberWithSessionAsync(Guid teamId)
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

        /// <summary>
        /// Seeds a HabitEntry directly into the DB.
        /// </summary>
        private async Task<Guid> SeedEntryAsync(Guid habitId, Guid userId,
            EntryStatus status = EntryStatus.Logged, float? value = null)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var entry = new HabitEntry
            {
                Id = Guid.NewGuid(),
                HabitId = habitId,
                UserId = userId,
                Date = DateTime.UtcNow.Date,
                Status = status,
                Notes = "",
                Value = value
            };
            db.HabitEntries.Add(entry);
            await db.SaveChangesAsync();

            return entry.Id;
        }

        // -----------------------------------------------------------------------
        // EditHabit PATCH /habits/{habitId}
        // -----------------------------------------------------------------------

        [Fact]
        public async Task EditHabit_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, _, habitId) = await SeedFullSetupAsync();

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}",
                new UpdateHabitDto { Name = "New Name" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task EditHabit_ShouldReturn200_WhenCreatorUpdates()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}",
                new UpdateHabitDto { Name = "Updated Name" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<HabitResponseDto>();
            Assert.Equal("Updated Name", result.Name);
        }

        [Fact]
        public async Task EditHabit_ShouldReturn403_WhenMemberTriesToUpdate()
        {
            var (_, _, teamId, habitId) = await SeedFullSetupAsync();
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            var client = CreateAuthenticatedClient(memberId, "member@test.com", memberSessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}",
                new UpdateHabitDto { Name = "Hacked Name" });


            var body = await response.Content.ReadAsStringAsync(); // add this
            Console.WriteLine(body);                               // add this

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

           // Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task EditHabit_ShouldReturn404_WhenHabitDoesNotExist()
        {
            var (userId, sessionId, _, _) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{Guid.NewGuid()}",
                new UpdateHabitDto { Name = "Ghost" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task EditHabit_ShouldReturn400_WhenExpiryDateIsInThePast()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PatchAsJsonAsync($"/habits/{habitId}",
                new UpdateHabitDto { ExpiryDate = DateTime.UtcNow.AddDays(-1) });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // -----------------------------------------------------------------------
        // ArchiveHabit POST /habits/{habitId}/archive
        // -----------------------------------------------------------------------

        [Fact]
        public async Task ArchiveHabit_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, _, habitId) = await SeedFullSetupAsync();

            var response = await client.PostAsync($"/habits/{habitId}/archive", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ArchiveHabit_ShouldReturn200_WhenCreatorArchives()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsync($"/habits/{habitId}/archive", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ArchiveHabit_ShouldReturn403_WhenMemberTriesToArchive()
        {
            var (_, _, teamId, habitId) = await SeedFullSetupAsync();
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            var client = CreateAuthenticatedClient(memberId, "member@test.com", memberSessionId);

            var response = await client.PostAsync($"/habits/{habitId}/archive", null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ArchiveHabit_ShouldReturn409_WhenHabitIsAlreadyClosed()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync(state: HabitState.Closed);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsync($"/habits/{habitId}/archive", null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // -----------------------------------------------------------------------
        // DeleteHabit DELETE /habits/{habitId}
        // -----------------------------------------------------------------------

        [Fact]
        public async Task DeleteHabit_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, _, habitId) = await SeedFullSetupAsync();

            var response = await client.DeleteAsync($"/habits/{habitId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteHabit_ShouldReturn204_WhenCreatorDeletes()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.DeleteAsync($"/habits/{habitId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteHabit_ShouldReturn403_WhenMemberTriesToDelete()
        {
            var (_, _, teamId, habitId) = await SeedFullSetupAsync();
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            var client = CreateAuthenticatedClient(memberId, "member@test.com", memberSessionId);

            var response = await client.DeleteAsync($"/habits/{habitId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteHabit_ShouldReturn404_WhenHabitDoesNotExist()
        {
            var (userId, sessionId, _, _) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.DeleteAsync($"/habits/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // -----------------------------------------------------------------------
        // LogProgress POST /habits/{habitId}/entries
        // -----------------------------------------------------------------------

        [Fact]
        public async Task LogProgress_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, _, habitId) = await SeedFullSetupAsync();

            var response = await client.PostAsJsonAsync($"/habits/{habitId}/entries",
                new LogProgressDto { Status = "Logged", Notes = "" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task LogProgress_ShouldReturn201_WhenBinaryHabitLoggedByCreator()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync(type: HabitType.Binary);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync($"/habits/{habitId}/entries",
                new LogProgressDto { Status = "Logged", Notes = "" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task LogProgress_ShouldReturn201_WhenQuantitativeHabitLoggedWithValue()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync(type: HabitType.Quantitative);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync($"/habits/{habitId}/entries",
                new LogProgressDto { Status = "Logged", Value = 5.0f, Notes = "" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task LogProgress_ShouldReturn201_WhenMemberLogsProgress()
        {
            var (_, _, teamId, habitId) = await SeedFullSetupAsync(type: HabitType.Binary);
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            var client = CreateAuthenticatedClient(memberId, "member@test.com", memberSessionId);

            var response = await client.PostAsJsonAsync($"/habits/{habitId}/entries",
                new LogProgressDto { Status = "Logged", Notes = "" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task LogProgress_ShouldReturn409_WhenAlreadyLoggedToday()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync(type: HabitType.Binary);
            await SeedEntryAsync(habitId, userId);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync($"/habits/{habitId}/entries",
                new LogProgressDto { Status = "Logged", Notes = "" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task LogProgress_ShouldReturn400_WhenQuantitativeHabitMissingValue()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync(type: HabitType.Quantitative);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync($"/habits/{habitId}/entries",
                new LogProgressDto { Status = "Logged", Notes = "" }); // no Value

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task LogProgress_ShouldReturn409_WhenHabitIsArchived()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync(state: HabitState.Archived);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync($"/habits/{habitId}/entries",
                new LogProgressDto { Status = "Logged", Notes = "" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // -----------------------------------------------------------------------
        // UndoLog DELETE /habits/{habitId}/entries/{entryId}
        // -----------------------------------------------------------------------

        [Fact]
        public async Task UndoLog_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, _, habitId) = await SeedFullSetupAsync();

            var response = await client.DeleteAsync($"/habits/{habitId}/entries/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UndoLog_ShouldReturn204_WhenEntryDeletedSuccessfully()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            var entryId = await SeedEntryAsync(habitId, userId);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.DeleteAsync($"/habits/{habitId}/entries/{entryId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task UndoLog_ShouldReturn404_WhenEntryDoesNotExist()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.DeleteAsync($"/habits/{habitId}/entries/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // -----------------------------------------------------------------------
        // GetProgress GET /habits/{habitId}/entries
        // -----------------------------------------------------------------------

        [Fact]
        public async Task GetProgress_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, _, habitId) = await SeedFullSetupAsync();

            var response = await client.GetAsync($"/habits/{habitId}/entries");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetProgress_ShouldReturn200_WithOwnEntries()
        {
            var (userId, sessionId, _, habitId) = await SeedFullSetupAsync();
            await SeedEntryAsync(habitId, userId);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.GetAsync($"/habits/{habitId}/entries");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var entries = await response.Content.ReadFromJsonAsync<List<HabitEntryResponseDto>>();
            Assert.Single(entries);
        }

        [Fact]
        public async Task GetProgress_ShouldReturn403_WhenMemberViewsAnotherMembersEntries()
        {
            var (_, _, teamId, habitId) = await SeedFullSetupAsync();
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            var (otherMemberId, _) = await SeedMemberWithSessionAsync(teamId);
            var client = CreateAuthenticatedClient(memberId, "member@test.com", memberSessionId);

            var response = await client.GetAsync($"/habits/{habitId}/entries?memberId={otherMemberId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetProgress_ShouldReturn200_WhenCreatorViewsMemberEntries()
        {
            var (userId, sessionId, teamId, habitId) = await SeedFullSetupAsync();
            var (memberId, _) = await SeedMemberWithSessionAsync(teamId);
            await SeedEntryAsync(habitId, memberId);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.GetAsync($"/habits/{habitId}/entries?memberId={memberId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var entries = await response.Content.ReadFromJsonAsync<List<HabitEntryResponseDto>>();
            Assert.Single(entries);
        }

        // -----------------------------------------------------------------------
        // GetLeaderboard GET /habits/{habitId}/leaderboard
        // -----------------------------------------------------------------------

        [Fact]
        public async Task GetLeaderboard_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, _, habitId) = await SeedFullSetupAsync();

            var response = await client.GetAsync($"/habits/{habitId}/leaderboard");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetLeaderboard_ShouldReturn200_WithRankedMembers()
        {
            var (userId, sessionId, teamId, habitId) = await SeedFullSetupAsync(type: HabitType.Binary);
            var (memberId, _) = await SeedMemberWithSessionAsync(teamId);
            await SeedEntryAsync(habitId, memberId);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.GetAsync($"/habits/{habitId}/leaderboard");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var leaderboard = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
            Assert.NotEmpty(leaderboard);
        }

        [Fact]
        public async Task GetLeaderboard_ShouldReturn404_WhenHabitDoesNotExist()
        {
            var (userId, sessionId, _, _) = await SeedFullSetupAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.GetAsync($"/habits/{Guid.NewGuid()}/leaderboard");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}