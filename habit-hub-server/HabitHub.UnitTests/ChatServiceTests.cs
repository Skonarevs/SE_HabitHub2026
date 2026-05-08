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
    public class ChatServiceTests
    {
        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private AppDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        /// <summary>
        /// Seeds a TeamCreator, a Team with a chat, and an active membership
        /// for the creator — all in one SaveChangesAsync.
        /// Returns (creatorId, teamId).
        /// </summary>
        private async Task<(Guid creatorId, Guid teamId)> SeedTeamAsync(AppDbContext db)
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
                CreatorId = creator.Id,
                Chat = new TeamChat()
            };
            db.Teams.Add(team);

            db.Memberships.Add(new Membership
            {
                UserId = creator.Id,
                TeamId = team.Id,
                Status = MembershipStatus.Active
            });

            await db.SaveChangesAsync();
            return (creator.Id, team.Id);
        }

        /// <summary>
        /// Seeds a TeamMember with an active membership in the given team.
        /// </summary>
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

        /// <summary>
        /// Seeds a message in the team's chat sent by the given user.
        /// </summary>
        private async Task<Guid> SeedMessageAsync(AppDbContext db, Guid teamId, Guid senderId)
        {
            var team = await db.Teams.Include(t => t.Chat).FirstAsync(t => t.Id == teamId);

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

        // -----------------------------------------------------------------------
        // GetMessagesAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task GetMessages_ShouldThrowNotFound_WhenTeamDoesNotExist()
        {
            var db = CreateDb();
            var service = new ChatService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetMessagesAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetMessages_ShouldThrowForbidden_WhenUserIsNotMemberOrCreator()
        {
            var db = CreateDb();
            var (_, teamId) = await SeedTeamAsync(db);
            var service = new ChatService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.GetMessagesAsync(teamId, Guid.NewGuid()));
        }

        [Fact]
        public async Task GetMessages_ShouldReturnEmptyList_WhenNoMessagesExist()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var service = new ChatService(db);

            var result = await service.GetMessagesAsync(teamId, creatorId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMessages_ShouldReturnMessages_WhenCreatorRequests()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            await SeedMessageAsync(db, teamId, creatorId);
            var service = new ChatService(db);

            var result = await service.GetMessagesAsync(teamId, creatorId);

            Assert.Single(result);
            Assert.Equal(creatorId, result[0].SenderId);
            Assert.Equal("Hello!", result[0].Content);
        }

        [Fact]
        public async Task GetMessages_ShouldReturnMessages_WhenMemberRequests()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);
            await SeedMessageAsync(db, teamId, creatorId);
            var service = new ChatService(db);

            var result = await service.GetMessagesAsync(teamId, memberId);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetMessages_ShouldReturnMessagesInChronologicalOrder()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var team = await db.Teams.Include(t => t.Chat).FirstAsync(t => t.Id == teamId);

            // Seed two messages with different timestamps
            db.Messages.Add(new Message
            {
                Id = Guid.NewGuid(),
                ChatId = team.Chat.Id,
                SenderId = creatorId,
                Content = "First",
                SentAt = DateTime.UtcNow.AddMinutes(-5)
            });
            db.Messages.Add(new Message
            {
                Id = Guid.NewGuid(),
                ChatId = team.Chat.Id,
                SenderId = creatorId,
                Content = "Second",
                SentAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var service = new ChatService(db);
            var result = await service.GetMessagesAsync(teamId, creatorId);

            Assert.Equal(2, result.Count);
            Assert.Equal("First", result[0].Content);
            Assert.Equal("Second", result[1].Content);
        }

        // -----------------------------------------------------------------------
        // SendMessageAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task SendMessage_ShouldThrowNotFound_WhenTeamDoesNotExist()
        {
            var db = CreateDb();
            var service = new ChatService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.SendMessageAsync(Guid.NewGuid(), Guid.NewGuid(), "Hello!"));
        }

        [Fact]
        public async Task SendMessage_ShouldThrowForbidden_WhenUserIsNotMemberOrCreator()
        {
            var db = CreateDb();
            var (_, teamId) = await SeedTeamAsync(db);
            var service = new ChatService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.SendMessageAsync(teamId, Guid.NewGuid(), "Hello!"));
        }

        [Fact]
        public async Task SendMessage_ShouldThrowValidation_WhenContentIsEmpty()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var service = new ChatService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.SendMessageAsync(teamId, creatorId, ""));
        }

        [Fact]
        public async Task SendMessage_ShouldThrowValidation_WhenContentIsWhitespace()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var service = new ChatService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.SendMessageAsync(teamId, creatorId, "   "));
        }

        [Fact]
        public async Task SendMessage_ShouldReturnMessage_WhenCreatorSendsMessage()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var service = new ChatService(db);

            var result = await service.SendMessageAsync(teamId, creatorId, "Hello team!");

            Assert.Equal(creatorId, result.SenderId);
            Assert.Equal("Hello team!", result.Content);
            Assert.Equal("Creator", result.SenderName);
        }

        [Fact]
        public async Task SendMessage_ShouldReturnMessage_WhenMemberSendsMessage()
        {
            var db = CreateDb();
            var (_, teamId) = await SeedTeamAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);
            var service = new ChatService(db);

            var result = await service.SendMessageAsync(teamId, memberId, "Hi!");

            Assert.Equal(memberId, result.SenderId);
            Assert.Equal("Hi!", result.Content);
        }

        [Fact]
        public async Task SendMessage_ShouldPersistMessage_ToDatabase()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var service = new ChatService(db);

            var result = await service.SendMessageAsync(teamId, creatorId, "Persisted!");

            var saved = await db.Messages.FindAsync(result.Id);
            Assert.NotNull(saved);
            Assert.Equal("Persisted!", saved.Content);
        }

        // -----------------------------------------------------------------------
        // DeleteMessageAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task DeleteMessage_ShouldThrowNotFound_WhenTeamDoesNotExist()
        {
            var db = CreateDb();
            var service = new ChatService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.DeleteMessageAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteMessage_ShouldThrowForbidden_WhenUserIsNotMemberOrCreator()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            await SeedMessageAsync(db, teamId, creatorId);
            var service = new ChatService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.DeleteMessageAsync(teamId, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteMessage_ShouldThrowNotFound_WhenMessageDoesNotExist()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var service = new ChatService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.DeleteMessageAsync(teamId, Guid.NewGuid(), creatorId));
        }

        [Fact]
        public async Task DeleteMessage_ShouldThrowForbidden_WhenMemberTriesToDeleteAnotherMembersMessage()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);
            var anotherMemberId = await SeedMemberAsync(db, teamId);

            // anotherMember sends a message
            var messageId = await SeedMessageAsync(db, teamId, anotherMemberId);
            var service = new ChatService(db);

            // memberId tries to delete anotherMember's message — not allowed
            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.DeleteMessageAsync(teamId, messageId, memberId));
        }

        [Fact]
        public async Task DeleteMessage_ShouldSucceed_WhenSenderDeletesOwnMessage()
        {
            var db = CreateDb();
            var (_, teamId) = await SeedTeamAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);
            var messageId = await SeedMessageAsync(db, teamId, memberId);
            var service = new ChatService(db);

            await service.DeleteMessageAsync(teamId, messageId, memberId);

            Assert.Null(await db.Messages.FindAsync(messageId));
        }

        [Fact]
        public async Task DeleteMessage_ShouldSucceed_WhenCreatorDeletesAnyMessage()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);

            // Member sends a message
            var messageId = await SeedMessageAsync(db, teamId, memberId);
            var service = new ChatService(db);

            // Creator can delete anyone's message
            await service.DeleteMessageAsync(teamId, messageId, creatorId);

            Assert.Null(await db.Messages.FindAsync(messageId));
        }

        [Fact]
        public async Task DeleteMessage_ShouldRemoveMessage_FromDatabase()
        {
            var db = CreateDb();
            var (creatorId, teamId) = await SeedTeamAsync(db);
            var messageId = await SeedMessageAsync(db, teamId, creatorId);
            var service = new ChatService(db);

            await service.DeleteMessageAsync(teamId, messageId, creatorId);

            var deleted = await db.Messages.FindAsync(messageId);
            Assert.Null(deleted);
        }
    }
}