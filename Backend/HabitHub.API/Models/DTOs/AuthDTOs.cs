namespace HabitHub.API.Models.DTOs
{
    public class RegisterDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Timezone { get; set; } // IANA 
        public string UserType { get; set; } // "Creator" or "Member"
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; } 
    }

    public class AuthResponseDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
        public Guid SessionId { get; set; }
        public DateTime SessionExpiry { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ChangeEmailDto
    {
        public string NewEmail { get; set; }
        public string Password { get; set; }
    }

    public class SessionInfoDto
    {
        public Guid SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } 
    }
}
