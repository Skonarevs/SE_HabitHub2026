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
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenEmailIsUnique()
        {
            // Arrange
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

            // Act
            var result = await service.RegisterAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Email, result.Email);

            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            Assert.NotNull(userInDb);
        }
    
}
}