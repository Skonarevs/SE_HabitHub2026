using HabitHub.API.Exceptions;
using HabitHub.API.Models.DTOs;
using HabitHub.API.Services;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HabitHub.UnitTests
{
    public class FakeEmailSender : HabitHub.API.Services.IEmailSender
    {
        public Task SendEmailAsync(string toEmail, string toName, string subject, string message)
        {
            return Task.CompletedTask;
        }
    }

    public class AuthServiceTests
    {
        private AuthService GetService(AppDbContext context)
        {
            var passwordHasher = new PasswordHasher<User>();
            var emailSender = new FakeEmailSender();
            return new AuthService(context, passwordHasher, emailSender);
        }


        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }


        private TeamCreator CreateTestCreator(string email, string password)
        {
            var hasher = new PasswordHasher<User>();

            return new TeamCreator
            {
                Name = "Test",
                Email = email,
                Timezone = "UTC",
                PasswordHash = hasher.HashPassword(null, password)
            };
        }



        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenEmailIsUnique()
        {

            var context = GetDbContext();
            var service = GetService(context);

            var dto = new RegisterDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                UserType = "Creator",
                Timezone = "UTC"
            };


            var result = await service.RegisterAsync(dto);


            Assert.NotNull(result);
            Assert.Equal(dto.Email, result.Email);

            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            Assert.NotNull(userInDb);
        }



        [Fact]
        public async Task RegisterAsync_ShouldThrowConflictException_WhenEmailExists()
        {

            var context = GetDbContext();

            context.Users.Add(new TeamMember
            {
                Name = "Existing",
                Email = "test@example.com",
                PasswordHash = "hash",
                Timezone = "UTC",
            });
            await context.SaveChangesAsync();

            var service = GetService(context);

            var dto = new RegisterDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                UserType = "Creator",
                Timezone = "UTC"
            };


            await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(dto));
        }



        [Fact]
        public async Task RegisterAsync_ShouldThrowValidationException_WhenUserTypeInvalid()
        {

            var context = GetDbContext();
            var service = GetService(context);

            var dto = new RegisterDto
            {
                Name = "Test User",
                Email = "test2@example.com",
                Password = "Password123!",
                UserType = "InvalidType",
                Timezone = "UTC"
            };


            await Assert.ThrowsAsync<ValidationException>(() => service.RegisterAsync(dto));
        }


        [Fact]
        public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsValid()
        {

            var context = GetDbContext();
            var service = GetService(context);

            var passwordHasher = new PasswordHasher<User>();

            var user = new TeamCreator
            {
                Name = "Test",
                Email = "login@test.com",
                PasswordHash = passwordHasher.HashPassword(null, "Password123!"),
                Timezone = "UTC"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new LoginDto
            {
                Email = "login@test.com",
                Password = "Password123!",
                UserType = "Creator"
            };


            var result = await service.LoginAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
        }



        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
        {

            var context = GetDbContext();
            var service = GetService(context);

            var dto = new LoginDto
            {
                Email = "notfound@test.com",
                Password = "Password123!",
                UserType = "Creator"
            };

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.LoginAsync(dto));
        }


        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenPasswordIncorrect()
        {

            var context = GetDbContext();
            var service = GetService(context);

            var passwordHasher = new PasswordHasher<User>();

            var user = new TeamCreator
            {
                Name = "Test",
                Email = "wrongpass@test.com",
                PasswordHash = passwordHasher.HashPassword(null, "CorrectPassword"),
                Timezone = "UTC",
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new LoginDto
            {
                Email = "wrongpass@test.com",
                Password = "WrongPassword",
                UserType = "Creator"
            };


            await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.LoginAsync(dto));
        }


        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenUserTypeIncorrect()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var passwordHasher = new PasswordHasher<User>();

            var user = new TeamCreator
            {
                Name = "Test",
                Email = "type@test.com",
                PasswordHash = passwordHasher.HashPassword(null, "Password123!"),
                Timezone = "UTC",
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new LoginDto
            {
                Email = "type@test.com",
                Password = "Password123!",
                UserType = "Member" // WRONG
            };


            await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.LoginAsync(dto));
        }



        [Fact]
        public async Task GetActiveSessionsAsync_ShouldReturnOnlyActiveSessions()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var userId = Guid.NewGuid();

            context.Sessions.Add(new Session
            {
                UserId = userId,
                Status = SessionStatus.Active
            });

            context.Sessions.Add(new Session
            {
                UserId = userId,
                Status = SessionStatus.Invalidated
            });

            await context.SaveChangesAsync();

            var result = await service.GetActiveSessionsAsync(userId);

            Assert.Single(result);
        }


        [Fact]
        public async Task InvalidateSessionAsync_ShouldThrow_WhenSessionNotFound()
        {
            var context = GetDbContext();
            var service = GetService(context);


            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.InvalidateSessionAsync(Guid.NewGuid(), Guid.NewGuid()));
        }



        [Fact]
        public async Task ChangePasswordAsync_ShouldChangePassword_AndInvalidateOtherSessions()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var user = CreateTestCreator("test@test.com", "OldPassword");
            context.Users.Add(user);

            var currentSession = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Status = SessionStatus.Active
            };

            var otherSession = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Status = SessionStatus.Active
            };

            context.Sessions.AddRange(currentSession, otherSession);
            await context.SaveChangesAsync();

            var dto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword",
                NewPassword = "NewPassword123!"
            };

            await service.ChangePasswordAsync(user.Id, dto, currentSession.Id);

            var updatedUser = await context.Users.FindAsync(user.Id);
            var hasher = new PasswordHasher<User>();

            var verify = hasher.VerifyHashedPassword(null, updatedUser.PasswordHash, "NewPassword123!");
            Assert.Equal(PasswordVerificationResult.Success, verify);

            var sessions = await context.Sessions.ToListAsync();

            Assert.Contains(sessions, s => s.Id == otherSession.Id && s.Status == SessionStatus.Invalidated);
            Assert.Contains(sessions, s => s.Id == currentSession.Id && s.Status == SessionStatus.Active);
        }


        [Fact]
        public async Task ChangePasswordAsync_ShouldThrow_WhenCurrentPasswordIncorrect()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var user = CreateTestCreator("test@test.com", "CorrectPassword");
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new ChangePasswordDto
            {
                CurrentPassword = "WrongPassword",
                NewPassword = "NewPassword123!"
            };

            await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
                service.ChangePasswordAsync(user.Id, dto, Guid.NewGuid()));
        }


        [Fact]
        public async Task ChangeEmailAsync_ShouldChangeEmail_AndInvalidateOtherSessions()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var user = CreateTestCreator("old@test.com", "Password123!");
            context.Users.Add(user);

            var currentSession = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Status = SessionStatus.Active
            };

            var otherSession = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Status = SessionStatus.Active
            };

            context.Sessions.AddRange(currentSession, otherSession);
            await context.SaveChangesAsync();

            var dto = new ChangeEmailDto
            {
                Password = "Password123!",
                NewEmail = "new@test.com"
            };

            await service.ChangeEmailAsync(user.Id, dto, currentSession.Id);

            var updatedUser = await context.Users.FindAsync(user.Id);
            Assert.Equal("new@test.com", updatedUser.Email);


            var sessions = await context.Sessions.ToListAsync();

            Assert.Contains(sessions, s => s.Id == otherSession.Id && s.Status == SessionStatus.Invalidated);
            Assert.Contains(sessions, s => s.Id == currentSession.Id && s.Status == SessionStatus.Active);
        }

        [Fact]
        public async Task ChangeEmailAsync_ShouldThrow_WhenPasswordIncorrect()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var user = CreateTestCreator("test@test.com", "CorrectPassword");
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new ChangeEmailDto
            {
                Password = "WrongPassword",
                NewEmail = "new@test.com"
            };

            await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
                service.ChangeEmailAsync(user.Id, dto, Guid.NewGuid()));
        }



        [Fact]
        public async Task ChangeEmailAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var user1 = CreateTestCreator("user1@test.com", "Password123!");
            var user2 = CreateTestCreator("user2@test.com", "Password123!");

            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            var dto = new ChangeEmailDto
            {
                Password = "Password123!",
                NewEmail = "user2@test.com"
            };

            await Assert.ThrowsAsync<ConflictException>(() =>
                service.ChangeEmailAsync(user1.Id, dto, Guid.NewGuid()));
        }




        [Fact]
        public async Task ChangeEmailAsync_ShouldThrow_WhenUserNotFound()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ChangeEmailDto
            {
                Password = "pass",
                NewEmail = "new@test.com"
            };

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.ChangeEmailAsync(Guid.NewGuid(), dto, Guid.NewGuid()));
        }
    }
}