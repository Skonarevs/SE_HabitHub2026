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
    public class TeamsControllerApiTests : AuthTestBase
    {
        public TeamsControllerApiTests(WebApplicationFactory<Program> factory) : base(factory) { }



        // Seeds a TeamCreator with a session in a single DB scope.

        private async Task<(Guid userId, Guid sessionId)> SeedCreatorWithSessionAsync()
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

            await db.SaveChangesAsync();
            return (user.Id, session.Id);
        }

        /// <summary>
        /// Seeds a TeamMember with a session in a single DB scope.
        /// </summary>
        private async Task<(Guid userId, Guid sessionId)> SeedMemberWithSessionAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = new TeamMember
            {
                Id = Guid.NewGuid(),
                Name = "Member",
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

            await db.SaveChangesAsync();
            return (user.Id, session.Id);
        }

        /// <summary>
        /// Seeds a Team with its chat and an active membership for the creator
        /// in a single DB scope.
        /// </summary>
        private async Task<Guid> SeedTeamAsync(Guid creatorId)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                CreatorId = creatorId,
                Chat = new TeamChat(),
                //CreatedAt = DateTime.UtcNow
            };
            db.Teams.Add(team);

            db.Memberships.Add(new Membership
            {
                UserId = creatorId,
                TeamId = team.Id,
                Status = MembershipStatus.Active
            });

            await db.SaveChangesAsync();
            return team.Id;
        }

        /// <summary>
        /// Seeds an active membership for an existing user in an existing team.
        /// </summary>
        private async Task SeedMembershipAsync(Guid teamId, Guid userId)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Memberships.Add(new Membership
            {
                UserId = userId,
                TeamId = teamId,
                Status = MembershipStatus.Active
            });

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds an active InviteCode for a team.
        /// </summary>
        private async Task<string> SeedInviteCodeAsync(Guid teamId)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var code = new InviteCode
            {
                Id = Guid.NewGuid(),
                Code = $"CODE{Guid.NewGuid().ToString("N")[..4].ToUpper()}",
                TeamId = teamId,
                ExpiryDate = DateTime.UtcNow.AddDays(10),
                State = InviteCodeState.Active
            };
            db.InviteCodes.Add(code);
            await db.SaveChangesAsync();

            return code.Code;
        }

        /// <summary>
        /// Seeds a Habit in the given team.
        /// </summary>
        private async Task<Guid> SeedHabitAsync(Guid teamId, HabitState state = HabitState.Active)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var habit = new Habit
            {
                Id = Guid.NewGuid(),
                Name = "Test Habit",
                Goal = "Stay consistent",
                Type = HabitType.Binary,
                Unit = "times",
                State = state,
                TeamId = teamId
            };
            db.Habits.Add(habit);
            await db.SaveChangesAsync();

            return habit.Id;
        }


        // CreateTeam POST /teams -----------------------------------------------------------------------


        [Fact]
        public async Task CreateTeam_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();

            var response = await client.PostAsJsonAsync("/teams",
                new CreateTeamDto { Name = "My Team" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateTeam_ShouldReturn201_WhenCreatorCreatesTeam()
        {
            var (userId, sessionId) = await SeedCreatorWithSessionAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync("/teams",
                new CreateTeamDto { Name = "My Team" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<TeamResponseDto>();
            Assert.Equal("My Team", result.Name);
            Assert.Equal(userId, result.CreatorId);
        }

        [Fact]
        public async Task CreateTeam_ShouldReturn403_WhenMemberTriesToCreateTeam()
        {
            var (userId, sessionId) = await SeedMemberWithSessionAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync("/teams",
                new CreateTeamDto { Name = "My Team" });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateTeam_ShouldReturn400_WhenTeamNameIsEmpty()
        {
            var (userId, sessionId) = await SeedCreatorWithSessionAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync("/teams",
                new CreateTeamDto { Name = "" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        // GetTeam GET /teams/{teamId} -----------------------------------------------------------------------


        [Fact]
        public async Task GetTeam_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (creatorId, _) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);

            var response = await client.GetAsync($"/teams/{teamId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetTeam_ShouldReturn200_WhenCreatorRequests()
        {
            var (userId, sessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(userId);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.GetAsync($"/teams/{teamId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<TeamResponseDto>();
            Assert.Equal(teamId, result.Id);
        }

        [Fact]
        public async Task GetTeam_ShouldReturn403_WhenUserIsNotMember()
        {
            var (creatorId, _) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var (outsiderId, outsiderSessionId) = await SeedMemberWithSessionAsync();
            var client = CreateAuthenticatedClient(outsiderId, "test@test.com", outsiderSessionId);

            var response = await client.GetAsync($"/teams/{teamId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetTeam_ShouldReturn404_WhenTeamDoesNotExist()
        {
            var (userId, sessionId) = await SeedCreatorWithSessionAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.GetAsync($"/teams/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }



 
        // GenerateInviteCode POST /teams/{teamId}/invite-codes -----------------------------------------------------------------------


        [Fact]
        public async Task GenerateInviteCode_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (creatorId, _) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);

            var response = await client.PostAsync($"/teams/{teamId}/invite-codes", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GenerateInviteCode_ShouldReturn201_WhenCreatorGenerates()
        {
            var (userId, sessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(userId);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsync($"/teams/{teamId}/invite-codes", null);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<InviteCodeResponseDto>();
            Assert.NotNull(result.Code);
            Assert.Equal(8, result.Code.Length);
        }

        [Fact]
        public async Task GenerateInviteCode_ShouldReturn403_WhenMemberTriesToGenerate()
        {
            var (creatorId, _) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync();
            await SeedMembershipAsync(teamId, memberId);
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PostAsync($"/teams/{teamId}/invite-codes", null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GenerateInviteCode_ShouldReturn404_WhenTeamDoesNotExist()
        {
            var (userId, sessionId) = await SeedCreatorWithSessionAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsync($"/teams/{Guid.NewGuid()}/invite-codes", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        // JoinTeam POST /teams/join -----------------------------------------------------------------------


        [Fact]
        public async Task JoinTeam_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();

            var response = await client.PostAsJsonAsync("/teams/join",
                new JoinTeamDto { Code = "TESTCODE" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task JoinTeam_ShouldReturn200_WhenValidCodeUsed()
        {
            var (creatorId, _) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var code = await SeedInviteCodeAsync(teamId);
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync();
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PostAsJsonAsync("/teams/join",
                new JoinTeamDto { Code = code });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MembershipResponseDto>();
            Assert.Equal(teamId, result.TeamId);
            Assert.Equal("Active", result.Status);
        }

        [Fact]
        public async Task JoinTeam_ShouldReturn404_WhenCodeIsInvalid()
        {
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync();
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PostAsJsonAsync("/teams/join",
                new JoinTeamDto { Code = "BADCODE" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task JoinTeam_ShouldReturn409_WhenUserIsAlreadyMember()
        {
            var (creatorId, _) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var code = await SeedInviteCodeAsync(teamId);
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync();
            await SeedMembershipAsync(teamId, memberId); // already a member
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PostAsJsonAsync("/teams/join",
                new JoinTeamDto { Code = code });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }




        // KickUser POST /teams/{teamId}/members/{memberId}/kick -----------------------------------------------------------------------


        [Fact]
        public async Task KickUser_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();

            var response = await client.PostAsync(
                $"/teams/{Guid.NewGuid()}/members/{Guid.NewGuid()}/kick", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task KickUser_ShouldReturn200_WhenCreatorKicksMember()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var (memberId, _) = await SeedMemberWithSessionAsync();
            await SeedMembershipAsync(teamId, memberId);
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.PostAsync(
                $"/teams/{teamId}/members/{memberId}/kick", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task KickUser_ShouldReturn403_WhenMemberTriesToKick()
        {
            var (creatorId, _) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync();
            await SeedMembershipAsync(teamId, memberId);
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PostAsync(
                $"/teams/{teamId}/members/{memberId}/kick", null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task KickUser_ShouldReturn404_WhenTeamDoesNotExist()
        {
            var (userId, sessionId) = await SeedCreatorWithSessionAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsync(
                $"/teams/{Guid.NewGuid()}/members/{Guid.NewGuid()}/kick", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task KickUser_ShouldReturn409_WhenCreatorTriesToKickThemself()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.PostAsync(
                $"/teams/{teamId}/members/{creatorId}/kick", null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }



        // LeaveTeam POST /teams/{teamId}/leave -----------------------------------------------------------------------



        [Fact]
        public async Task LeaveTeam_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();

            var response = await client.PostAsync($"/teams/{Guid.NewGuid()}/leave", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task LeaveTeam_ShouldReturn200_WhenMemberLeaves()
        {
            var (creatorId, _) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync();
            await SeedMembershipAsync(teamId, memberId);
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PostAsync($"/teams/{teamId}/leave", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task LeaveTeam_ShouldReturn409_WhenCreatorTriesToLeave()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.PostAsync($"/teams/{teamId}/leave", null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task LeaveTeam_ShouldReturn404_WhenTeamDoesNotExist()
        {
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync();
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PostAsync($"/teams/{Guid.NewGuid()}/leave", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        // DeleteTeam DELETE /teams/{teamId} -----------------------------------------------------------------------


        [Fact]
        public async Task DeleteTeam_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();

            var response = await client.DeleteAsync($"/teams/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTeam_ShouldReturn204_WhenCreatorDeletes()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.DeleteAsync($"/teams/{teamId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTeam_ShouldReturn403_WhenMemberTriesToDelete()
        {
            var (creatorId, _) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync();
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.DeleteAsync($"/teams/{teamId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTeam_ShouldReturn404_WhenTeamDoesNotExist()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.DeleteAsync($"/teams/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }



        // CreateHabit POST /teams/{teamId}/habits -----------------------------------------------------------------------

        [Fact]
        public async Task CreateHabit_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();

            var response = await client.PostAsJsonAsync($"/teams/{Guid.NewGuid()}/habits",
                new CreateHabitDto { Name = "Habit", Goal = "Goal", HabitType = "Binary", Unit = "times" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateHabit_ShouldReturn201_WhenCreatorCreatesBinaryHabit()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.PostAsJsonAsync($"/teams/{teamId}/habits",
                new CreateHabitDto { Name = "Run", Goal = "Daily run", HabitType = "Binary", Unit = "times" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<HabitResponseDto>();
            Assert.Equal("Run", result.Name);
            Assert.Equal("Binary", result.HabitType);
        }

        [Fact]
        public async Task CreateHabit_ShouldReturn201_WhenCreatorCreatesQuantitativeHabit()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.PostAsJsonAsync($"/teams/{teamId}/habits",
                new CreateHabitDto { Name = "Run", Goal = "Daily run", HabitType = "Quantitative", Unit = "km" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<HabitResponseDto>();
            Assert.Equal("km", result.Unit);
        }

        [Fact]
        public async Task CreateHabit_ShouldReturn403_WhenMemberTriesToCreate()
        {
            var (creatorId, _) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync();
            await SeedMembershipAsync(teamId, memberId);
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PostAsJsonAsync($"/teams/{teamId}/habits",
                new CreateHabitDto { Name = "Habit", Goal = "Goal", HabitType = "Binary", Unit = "times" });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateHabit_ShouldReturn400_WhenHabitTypeIsInvalid()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.PostAsJsonAsync($"/teams/{teamId}/habits",
                new CreateHabitDto { Name = "Habit", Goal = "Goal", HabitType = "Invalid", Unit = "times" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateHabit_ShouldReturn404_WhenTeamDoesNotExist()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.PostAsJsonAsync($"/teams/{Guid.NewGuid()}/habits",
                new CreateHabitDto { Name = "Habit", Goal = "Goal", HabitType = "Binary", Unit = "times" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        // GetArchivedHabits GET /teams/{teamId}/habits?state=archived -----------------------------------------------------------------------


        [Fact]
        public async Task GetArchivedHabits_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();

            var response = await client.GetAsync($"/teams/{Guid.NewGuid()}/habits?state=archived");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetArchivedHabits_ShouldReturn200_WithArchivedHabits()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            await SeedHabitAsync(teamId, HabitState.Archived);
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.GetAsync($"/teams/{teamId}/habits?state=archived");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<List<ArchivedHabitDto>>();
            Assert.Single(result);
        }

        [Fact]
        public async Task GetArchivedHabits_ShouldReturn200_WithEmptyList_WhenNoHabitsArchived()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            await SeedHabitAsync(teamId, HabitState.Active); // active, not archived
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.GetAsync($"/teams/{teamId}/habits?state=archived");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<List<ArchivedHabitDto>>();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetArchivedHabits_ShouldReturn400_WhenStateParameterIsInvalid()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var teamId = await SeedTeamAsync(creatorId);
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            // The controller only accepts "archived" — anything else returns 400
            var response = await client.GetAsync($"/teams/{teamId}/habits?state=active");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetArchivedHabits_ShouldReturn404_WhenTeamDoesNotExist()
        {
            var (creatorId, creatorSessionId) = await SeedCreatorWithSessionAsync();
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.GetAsync($"/teams/{Guid.NewGuid()}/habits?state=archived");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}