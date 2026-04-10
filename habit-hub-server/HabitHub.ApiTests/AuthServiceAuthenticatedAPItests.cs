using HabitHub.API.Models.DTOs;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace HabitHub.ApiTests
{
    // Each test class gets its own isolated in-memory DB via a unique DB name.

    public abstract class AuthTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly WebApplicationFactory<Program> Factory;
        protected readonly string DbName;

        protected AuthTestBase(WebApplicationFactory<Program> factory)
        {

            DbName = $"TestDb_{Guid.NewGuid()}";

            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {

                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(DbName));
                });
            });
        }


        protected HttpClient CreateAuthenticatedClient(Guid userId, string email, Guid sessionId)
        {
            var authedFactory = Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(new TestAuthHandlerOptions
                    {
                        UserId = userId.ToString(),
                        Email = email,
                        SessionId = sessionId.ToString()
                    });

                    services.AddAuthentication(defaultScheme: "Test")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                                "Test", _ => { });
                });
            });

            var client = authedFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Session-Id", sessionId.ToString());
            return client;
        }


        protected async Task<(Guid UserId, Guid SessionId)> SeedUserWithSessionAsync(
            string email, string password)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = new TeamCreator
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = email,
                Timezone = "UTC",
                PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>()
                    .HashPassword(null, password)
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
    }




    public class AuthServiceApiTests : AuthTestBase
    {
        public AuthServiceApiTests(WebApplicationFactory<Program> factory) : base(factory) { }


        /// ////////////////////Register 


        [Fact]
        public async Task Register_ShouldReturn201_WhenValidCreatorDto()
        {
            var client = Factory.CreateClient();
            var dto = new RegisterDto
            {
                Name = "Alice",
                Email = "alice@test.com",
                Password = "Password123!",
                Timezone = "UTC",
                UserType = "Creator"
            };

            var response = await client.PostAsJsonAsync("/auth/register", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_ShouldReturn201_WhenValidMemberDto()
        {
            var client = Factory.CreateClient();
            var dto = new RegisterDto
            {
                Name = "Bob",
                Email = "bob@test.com",
                Password = "Password123!",
                Timezone = "UTC",
                UserType = "Member"
            };

            var response = await client.PostAsJsonAsync("/auth/register", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_ShouldReturn409_WhenEmailAlreadyExists()
        {
            var client = Factory.CreateClient();
            var dto = new RegisterDto
            {
                Name = "Carol",
                Email = "carol@test.com",
                Password = "Password123!",
                Timezone = "UTC",
                UserType = "Creator"
            };

            await client.PostAsJsonAsync("/auth/register", dto);      
            var response = await client.PostAsJsonAsync("/auth/register", dto);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Register_ShouldReturn400_WhenUserTypeIsInvalid()
        {
            var client = Factory.CreateClient();
            var dto = new RegisterDto
            {
                Name = "Dave",
                Email = "dave@test.com",
                Password = "Password123!",
                Timezone = "UTC",
                UserType = "Admin" // invalid
            };

            var response = await client.PostAsJsonAsync("/auth/register", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------------------- Login

        [Fact]
        public async Task Login_ShouldReturn200_WhenCredentialsAreCorrect()
        {

            var client = Factory.CreateClient();
            await client.PostAsJsonAsync("/auth/register", new RegisterDto
            {
                Name = "Eve",
                Email = "eve@test.com",
                Password = "Pass123!",
                Timezone = "UTC",
                UserType = "Creator"
            });

            var response = await client.PostAsJsonAsync("/auth/login", new LoginDto
            {
                Email = "eve@test.com",
                Password = "Pass123!",
                UserType = "Creator"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_ShouldReturn401_WhenPasswordIsWrong()
        {
            var client = Factory.CreateClient();
            await client.PostAsJsonAsync("/auth/register", new RegisterDto
            {
                Name = "Frank",
                Email = "frank@test.com",
                Password = "Pass123!",
                Timezone = "UTC",
                UserType = "Creator"
            });

            var response = await client.PostAsJsonAsync("/auth/login", new LoginDto
            {
                Email = "frank@test.com",
                Password = "WrongPass!",
                UserType = "Creator"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_ShouldReturn401_WhenUserTypeMismatch()
        {
            var client = Factory.CreateClient();
            await client.PostAsJsonAsync("/auth/register", new RegisterDto
            {
                Name = "Grace",
                Email = "grace@test.com",
                Password = "Pass123!",
                Timezone = "UTC",
                UserType = "Creator"
            });

            // Registered as Creator but trying to login as Member
            var response = await client.PostAsJsonAsync("/auth/login", new LoginDto
            {
                Email = "grace@test.com",
                Password = "Pass123!",
                UserType = "Member"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_ShouldReturn401_WhenUserDoesNotExist()
        {
            var client = Factory.CreateClient();

            var response = await client.PostAsJsonAsync("/auth/login", new LoginDto
            {
                Email = "nobody@test.com",
                Password = "Pass123!",
                UserType = "Creator"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // /////////////////////////////--- ChangePassword 

        [Fact]
        public async Task ChangePassword_ShouldReturn200_WhenCurrentPasswordCorrect()
        {
            var (userId, sessionId) = await SeedUserWithSessionAsync("cp1@test.com", "OldPass123!");
            var client = CreateAuthenticatedClient(userId, "cp1@test.com", sessionId);

            var response = await client.PostAsJsonAsync("/auth/change-password", new ChangePasswordDto
            {
                CurrentPassword = "OldPass123!",
                NewPassword = "NewPass123!"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_ShouldReturn401_WhenCurrentPasswordIsWrong()
        {
            var (userId, sessionId) = await SeedUserWithSessionAsync("cp2@test.com", "CorrectPass123!");
            var client = CreateAuthenticatedClient(userId, "cp2@test.com", sessionId);

            var response = await client.PostAsJsonAsync("/auth/change-password", new ChangePasswordDto
            {
                CurrentPassword = "WrongPass!",
                NewPassword = "NewPass123!"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        //ChangeEmail --------------------------------------------------------

        [Fact]
        public async Task ChangeEmail_ShouldReturn200_WhenPasswordCorrectAndEmailAvailable()
        {
            var (userId, sessionId) = await SeedUserWithSessionAsync("ce1@test.com", "Password123!");
            var client = CreateAuthenticatedClient(userId, "ce1@test.com", sessionId);

            var response = await client.PostAsJsonAsync("/auth/change-email", new ChangeEmailDto
            {
                Password = "Password123!",
                NewEmail = "ce1new@test.com"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ChangeEmail_ShouldReturn409_WhenNewEmailAlreadyExists()
        {
            var (userId1, sessionId1) = await SeedUserWithSessionAsync("ce2a@test.com", "Password123!");
            await SeedUserWithSessionAsync("ce2b@test.com", "Password123!");

            var client = CreateAuthenticatedClient(userId1, "ce2a@test.com", sessionId1);

            var response = await client.PostAsJsonAsync("/auth/change-email", new ChangeEmailDto
            {
                Password = "Password123!",
                NewEmail = "ce2b@test.com" 
            });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task ChangeEmail_ShouldReturn401_WhenPasswordIsWrong()
        {
            var (userId, sessionId) = await SeedUserWithSessionAsync("ce3@test.com", "Password123!");
            var client = CreateAuthenticatedClient(userId, "ce3@test.com", sessionId);

            var response = await client.PostAsJsonAsync("/auth/change-email", new ChangeEmailDto
            {
                Password = "WrongPassword!",
                NewEmail = "ce3new@test.com"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        //Sessions -----------------------------------------------------------

        [Fact]
        public async Task GetSessions_ShouldReturn200_WithActiveSession()
        {
            var (userId, sessionId) = await SeedUserWithSessionAsync("sess1@test.com", "Password123!");
            var client = CreateAuthenticatedClient(userId, "sess1@test.com", sessionId);

            var response = await client.GetAsync("/auth/sessions");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var sessions = await response.Content.ReadFromJsonAsync<List<SessionInfoDto>>();
            Assert.NotEmpty(sessions);
        }

        [Fact]
        public async Task InvalidateSession_ShouldReturn204_WhenSessionBelongsToUser()
        {
            var (userId, sessionId) = await SeedUserWithSessionAsync("sess2@test.com", "Password123!");
            var client = CreateAuthenticatedClient(userId, "sess2@test.com", sessionId);

            var response = await client.DeleteAsync($"/auth/sessions/{sessionId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task InvalidateSession_ShouldReturn404_WhenSessionDoesNotExist()
        {
            var (userId, sessionId) = await SeedUserWithSessionAsync("sess3@test.com", "Password123!");
            var client = CreateAuthenticatedClient(userId, "sess3@test.com", sessionId);

            var fakeSessionId = Guid.NewGuid();
            var response = await client.DeleteAsync($"/auth/sessions/{fakeSessionId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}