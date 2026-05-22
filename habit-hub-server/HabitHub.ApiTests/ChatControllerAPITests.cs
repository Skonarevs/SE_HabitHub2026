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
    public class ChatControllerApiTests : AuthTestBase
    {
        public ChatControllerApiTests(WebApplicationFactory<Program> factory) : base(factory) { }

        //helpers

        /// <summary>
        /// Seeds a TeamCreator with a session, a Team with a chat,
        /// and an active membership for the creator — all in one DB scope.
        /// </summary>
        private async Task<(Guid userId, Guid sessionId, Guid teamId)> SeedCreatorWithTeamAsync()
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
                Chat = new TeamChat(),
                //CreatedAt = DateTime.UtcNow
            };
            db.Teams.Add(team);

            db.Memberships.Add(new Membership
            {
                UserId = user.Id,
                TeamId = team.Id,
                Status = MembershipStatus.Active
            });

            await db.SaveChangesAsync();
            return (user.Id, session.Id, team.Id);
        }

        /// <summary>
        /// Seeds a TeamMember with a session and an active membership
        /// in the given team — all in one DB scope.
        /// </summary>
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

        /// <summary>
        /// Seeds a message in the team's chat sent by the given user.
        /// </summary>
        private async Task<Guid> SeedMessageAsync(Guid teamId, Guid senderId)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var team = await db.Teams
                .Include(t => t.Chat)
                .FirstAsync(t => t.Id == teamId);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatId = team.Chat.Id,
                SenderId = senderId,
                Content = "Hello!",
                SentAt = DateTime.UtcNow
            };
            db.Messages.Add(message);
            await db.SaveChangesAsync();

            return message.Id;
        }


        // GetMessages GET /teams/{teamId}/chat/messages -----------------------------------------------------------------------


        [Fact]
        public async Task GetMessages_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (userId, _, teamId) = await SeedCreatorWithTeamAsync();

            var response = await client.GetAsync($"/teams/{teamId}/chat/messages");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMessages_ShouldReturn200_WithEmptyList_WhenNoMessages()
        {
            var (userId, sessionId, teamId) = await SeedCreatorWithTeamAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.GetAsync($"/teams/{teamId}/chat/messages");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<List<MessageResponseDto>>();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMessages_ShouldReturn200_WithMessages_WhenCreatorRequests()
        {
            var (userId, sessionId, teamId) = await SeedCreatorWithTeamAsync();
            await SeedMessageAsync(teamId, userId);
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.GetAsync($"/teams/{teamId}/chat/messages");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<List<MessageResponseDto>>();
            Assert.Single(result);
            Assert.Equal("Hello!", result[0].Content);
        }

        [Fact]
        public async Task GetMessages_ShouldReturn200_WhenMemberRequests()
        {
            var (creatorId, _, teamId) = await SeedCreatorWithTeamAsync();
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            await SeedMessageAsync(teamId, creatorId);
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.GetAsync($"/teams/{teamId}/chat/messages");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<List<MessageResponseDto>>();
            Assert.Single(result);
        }

        [Fact]
        public async Task GetMessages_ShouldReturn403_WhenUserIsNotMember()
        {
            var (_, _, teamId) = await SeedCreatorWithTeamAsync();
            // Seed an outsider with no membership
            var (outsiderId, outsiderSessionId) = await SeedMemberWithSessionAsync(Guid.NewGuid()); // different team
            var client = CreateAuthenticatedClient(outsiderId, "test@test.com", outsiderSessionId);

            var response = await client.GetAsync($"/teams/{teamId}/chat/messages");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetMessages_ShouldReturn404_WhenTeamDoesNotExist()
        {
            var (userId, sessionId, _) = await SeedCreatorWithTeamAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.GetAsync($"/teams/{Guid.NewGuid()}/chat/messages");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        // SendMessage POST /teams/{teamId}/chat/messages -----------------------------------------------------------------------

        [Fact]
        public async Task SendMessage_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, teamId) = await SeedCreatorWithTeamAsync();

            var response = await client.PostAsJsonAsync($"/teams/{teamId}/chat/messages",
                new SendMessageDto { Content = "Hello!" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SendMessage_ShouldReturn201_WhenCreatorSendsMessage()
        {
            var (userId, sessionId, teamId) = await SeedCreatorWithTeamAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync($"/teams/{teamId}/chat/messages",
                new SendMessageDto { Content = "Hello team!" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MessageResponseDto>();
            Assert.Equal("Hello team!", result.Content);
            Assert.Equal(userId, result.SenderId);
        }

        [Fact]
        public async Task SendMessage_ShouldReturn201_WhenMemberSendsMessage()
        {
            var (_, _, teamId) = await SeedCreatorWithTeamAsync();
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.PostAsJsonAsync($"/teams/{teamId}/chat/messages",
                new SendMessageDto { Content = "Hi from member!" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MessageResponseDto>();
            Assert.Equal(memberId, result.SenderId);
        }

        [Fact]
        public async Task SendMessage_ShouldReturn400_WhenContentIsEmpty()
        {
            var (userId, sessionId, teamId) = await SeedCreatorWithTeamAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync($"/teams/{teamId}/chat/messages",
                new SendMessageDto { Content = "" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendMessage_ShouldReturn403_WhenUserIsNotMember()
        {
            var (_, _, teamId) = await SeedCreatorWithTeamAsync();
            var (outsiderId, outsiderSessionId) = await SeedMemberWithSessionAsync(Guid.NewGuid());
            var client = CreateAuthenticatedClient(outsiderId, "test@test.com", outsiderSessionId);

            var response = await client.PostAsJsonAsync($"/teams/{teamId}/chat/messages",
                new SendMessageDto { Content = "Sneaky message" });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SendMessage_ShouldReturn404_WhenTeamDoesNotExist()
        {
            var (userId, sessionId, _) = await SeedCreatorWithTeamAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.PostAsJsonAsync($"/teams/{Guid.NewGuid()}/chat/messages",
                new SendMessageDto { Content = "Hello!" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        // DeleteMessage DELETE /teams/{teamId}/chat/messages/{messageId} -----------------------------------------------------------------------


        [Fact]
        public async Task DeleteMessage_ShouldReturn401_WhenNotAuthenticated()
        {
            var client = Factory.CreateClient();
            var (_, _, teamId) = await SeedCreatorWithTeamAsync();

            var response = await client.DeleteAsync(
                $"/teams/{teamId}/chat/messages/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturn204_WhenSenderDeletesOwnMessage()
        {
            var (_, _, teamId) = await SeedCreatorWithTeamAsync();
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            var messageId = await SeedMessageAsync(teamId, memberId);
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.DeleteAsync(
                $"/teams/{teamId}/chat/messages/{messageId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturn204_WhenCreatorDeletesAnyMessage()
        {
            var (creatorId, creatorSessionId, teamId) = await SeedCreatorWithTeamAsync();
            var (memberId, _) = await SeedMemberWithSessionAsync(teamId);
            var messageId = await SeedMessageAsync(teamId, memberId); // member's message
            var client = CreateAuthenticatedClient(creatorId, "test@test.com", creatorSessionId);

            var response = await client.DeleteAsync(
                $"/teams/{teamId}/chat/messages/{messageId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturn403_WhenMemberDeletesAnotherMembersMessage()
        {
            var (_, _, teamId) = await SeedCreatorWithTeamAsync();
            var (memberId, memberSessionId) = await SeedMemberWithSessionAsync(teamId);
            var (otherMemberId, _) = await SeedMemberWithSessionAsync(teamId);
            var messageId = await SeedMessageAsync(teamId, otherMemberId); // other member's message
            var client = CreateAuthenticatedClient(memberId, "test@test.com", memberSessionId);

            var response = await client.DeleteAsync(
                $"/teams/{teamId}/chat/messages/{messageId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturn404_WhenMessageDoesNotExist()
        {
            var (userId, sessionId, teamId) = await SeedCreatorWithTeamAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.DeleteAsync(
                $"/teams/{teamId}/chat/messages/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturn404_WhenTeamDoesNotExist()
        {
            var (userId, sessionId, _) = await SeedCreatorWithTeamAsync();
            var client = CreateAuthenticatedClient(userId, "test@test.com", sessionId);

            var response = await client.DeleteAsync(
                $"/teams/{Guid.NewGuid()}/chat/messages/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturn403_WhenUserIsNotMember()
        {
            var (creatorId, _, teamId) = await SeedCreatorWithTeamAsync();
            var messageId = await SeedMessageAsync(teamId, creatorId);
            var (outsiderId, outsiderSessionId) = await SeedMemberWithSessionAsync(Guid.NewGuid());
            var client = CreateAuthenticatedClient(outsiderId, "test@test.com", outsiderSessionId);

            var response = await client.DeleteAsync(
                $"/teams/{teamId}/chat/messages/{messageId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}