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
    public class AuthServiceTests
    {
        private AuthService GetService(AppDbContext context)
        {
            var passwordHasher = new PasswordHasher<User>();
            return new AuthService(context, passwordHasher);
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

    }
}