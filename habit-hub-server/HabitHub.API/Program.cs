using HabitHub.API.Services;
using HabitHub.API.Services.Interfaces;
using HabitHub.Data;
using Microsoft.EntityFrameworkCore;
using HabitHub.API.Controllers;
using Microsoft.AspNetCore.Identity;
using HabitHub.Models.Entities;
using HabitHub.API.Middleware;
using Microsoft.AspNetCore.Authentication;
using HabitHub.API.Auth;
using HabitHub.API.Models.Settings;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=tcp:habithub-server.database.windows.net,1433;Initial Catalog=HabitHubDB;User ID=habithubadmin;Password=NewPass123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IHabitService, HabitService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IReminderService, ReminderService>();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

//email configuration
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IEmailSender, EmailSender>();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Session";
    options.DefaultChallengeScheme = "Session";
})
.AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>("Session", null);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors();

app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<SessionAuthMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();  
}

app.Run();


public partial class Program { }
