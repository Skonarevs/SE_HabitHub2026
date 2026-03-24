using HabitHub.API.Models.DTOs;
using HabitHub.API.Services.Interfaces;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using HabitHub.API.Exceptions;


namespace HabitHub.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                throw new ConflictException("Email already exists");
            }
                
            User user;
            if (string.Equals(dto.UserType, "Creator", StringComparison.OrdinalIgnoreCase))
            {
                user = new TeamCreator
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Timezone = dto.Timezone,
                    PasswordHash = _passwordHasher.HashPassword(null, dto.Password) // TO CHANGE null
                };
            }
            else if (string.Equals(dto.UserType, "Member", StringComparison.OrdinalIgnoreCase))
            {
                user = new TeamMember
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Timezone = dto.Timezone,
                    PasswordHash = _passwordHasher.HashPassword(null, dto.Password) // TO CHANGE null
                };
            }
            else
            {
                throw new ValidationException("Invalid user type");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var session = new Session
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = SessionStatus.Active
            };
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                UserType = dto.UserType,
                SessionId = session.Id,
                SessionExpiry = session.ExpiryDate
            };
        }
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                throw new InvalidCredentialsException("Invalid credentials");
            }
                
            var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, dto.Password);
            if (result != PasswordVerificationResult.Success)
            {
                throw new InvalidCredentialsException("Invalid credentials");
            }

            string actualType;
            if (user is TeamCreator)
            {
                actualType = "Creator";

            }
            else
            {
                actualType = "Member";
            }
            if (!actualType.Equals(dto.UserType, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidCredentialsException("Invalid credentials");
            }
 

            // Craete a new session
            var session = new Session
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = SessionStatus.Active
            };
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                UserType = actualType,
                SessionId = session.Id,
                SessionExpiry = session.ExpiryDate
            };
        }

        public async Task<List<SessionInfoDto>> GetActiveSessionsAsync(Guid userId)
        {
            var sessions = await _context.Sessions
                .Where(s => s.UserId == userId && s.Status == SessionStatus.Active)
                .Select(s => new SessionInfoDto
                {
                    SessionId = s.Id,
                    CreatedAt = s.CreatedAt,
                    LastActivity = s.LastActivity,
                    ExpiryDate = s.ExpiryDate,
                    Status = s.Status.ToString()
                })
                .ToListAsync();

            return sessions;
        }

        public async Task InvalidateSessionAsync(Guid userId, Guid sessionId)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
            if (session == null)
            {
                throw new NotFoundException("Session not found");
            }
                

            session.Status = SessionStatus.Invalidated;
            await _context.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, Guid currentSessionId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
                
            var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, dto.CurrentPassword);
            if (result != PasswordVerificationResult.Success)
            {
                throw new InvalidCredentialsException("Current password is incorrect");
            }
                
            user.PasswordHash = _passwordHasher.HashPassword(null, dto.NewPassword);

 
            var otherSessions = await _context.Sessions
                .Where(s => s.UserId == userId && s.Id != currentSessionId && s.Status == SessionStatus.Active)
                .ToListAsync();
            foreach (var session in otherSessions)
            {
                session.Status = SessionStatus.Invalidated;
            }

            await _context.SaveChangesAsync();

            // TODO: Send notification email about password change
        }

        public async Task ChangeEmailAsync(Guid userId, ChangeEmailDto dto, Guid currentSessionId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, dto.Password);
            if (result != PasswordVerificationResult.Success)
            {
                throw new InvalidCredentialsException("Password is incorrect");
            }
                

            var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.NewEmail && u.Id != userId);
            if (emailExists)
            {
                throw new ConflictException("Email already exists");
            }

            var oldEmail = user.Email;
            user.Email = dto.NewEmail;

            var otherSessions = await _context.Sessions
                .Where(s => s.UserId == userId && s.Id != currentSessionId && s.Status == SessionStatus.Active)
                .ToListAsync();
            foreach (var session in otherSessions)
            {
                session.Status = SessionStatus.Invalidated;
            }

            await _context.SaveChangesAsync();

            // TODO: Send notification to old and new email addresses
        }
    }
}