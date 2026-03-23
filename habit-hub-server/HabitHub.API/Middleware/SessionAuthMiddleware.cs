using HabitHub.API.Exceptions;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HabitHub.API.Middleware
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
    
            if (!context.Request.Headers.TryGetValue("X-Session-Id", out var sessionIdStr))
            {
                // (let [Authorize] handle 401)
                await _next(context);
                return;
            }

            if (!Guid.TryParse(sessionIdStr, out var sessionId))
            {
                await _next(context);
                return;
            }

            var session = await dbContext.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.Status == SessionStatus.Active);


            if (session == null || session.ExpiryDate < DateTime.UtcNow)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired session");
                return;
            }

            session.LastActivity = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            var claims = new[]
            {
                new Claim("UserId", session.User.Id.ToString()),
                new Claim("SessionId", session.Id.ToString()),
            };
            var identity = new ClaimsIdentity(claims, "Session");
            var principal = new ClaimsPrincipal(identity);

            context.User = principal;

            await _next(context);
        }
    }
}