
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;


namespace HabitHub.ApiTests
{
    public class AuthServiseAPITests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public AuthServiseAPITests()
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                        if (descriptor != null)
                            services.Remove(descriptor);

                        services.AddDbContext<AppDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("TestDb");
                        });
                    });
                });

            _client = factory.CreateClient();
        }



        [Fact]
        public async Task Register_ShouldReturn201_WhenValidData()
        {
            var request = new
            {
                Name = "API User",
                Email = "api@test.com",
                Password = "Password123!",
                UserType = "Creator",
                Timezone = "UTC"
            };

            var response = await _client.PostAsJsonAsync("/auth/register", request);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);
        }



        [Fact]
        public async Task Register_ShouldReturn400_WhenValidationError()
        {
            var request = new
            {
                Name = "User",
                Email = "duplicate@test.com",
                Password = "Passwo",
                UserType = "Creator",
                Timezone = "UTC"
            };


            await _client.PostAsJsonAsync("/auth/register", request);


            var response = await _client.PostAsJsonAsync("/auth/register", request);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }




        [Fact]
        public async Task Register_ShouldReturn409_WhenEmailExists()
        {
            var request = new
            {
                Name = "User",
                Email = "duplicate@test.com",
                Password = "Password123!",
                UserType = "Creator",
                Timezone = "UTC"
            };


            await _client.PostAsJsonAsync("/auth/register", request);


            var response = await _client.PostAsJsonAsync("/auth/register", request);

            Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
        }



        [Fact]
        public async Task Login_ShouldReturn200Success_WhenCredentialsValid()
        {
            var register = new
            {
                Name = "Login User",
                Email = "loginapi@test.com",
                Password = "Password123!",
                UserType = "Creator",
                Timezone = "UTC"
            };

            await _client.PostAsJsonAsync("/auth/register", register);

            var login = new
            {
                Email = "loginapi@test.com",
                Password = "Password123!",
                UserType = "Creator"
            };

            var response = await _client.PostAsJsonAsync("/auth/login", login);

            response.EnsureSuccessStatusCode();

            //Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }




        [Fact]
        public async Task Login_ShouldReturn401Unauthorized_WhenPasswordWrong()
        {
            var register = new
            {
                Name = "User",
                Email = "wrongpassapi@test.com",
                Password = "Password123!",
                UserType = "Creator",
                Timezone = "UTC"
            };

            await _client.PostAsJsonAsync("/auth/register", register);

            var login = new
            {
                Email = "wrongpassapi@test.com",
                Password = "WrongPassword",
                UserType = "Creator"
            };

            var response = await _client.PostAsJsonAsync("/auth/login", login);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }




        [Fact]
        public async Task Login_ShouldReturn400ValidationError()
        {
            var register = new
            {
                Name = "User",
                Email = "wrongpassapi@test.com",
                Password = "Password123!",
                UserType = "Creator",
                Timezone = "UTC"
            };

            await _client.PostAsJsonAsync("/auth/register", register);

            var login = new
            {
                Email = "wrongpassapi@test.com",
                Password = "Wrong",
                UserType = "Creator"
            };

            var response = await _client.PostAsJsonAsync("/auth/login", login);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }



    }
}

