namespace HabitHub.API.Models.DTOs
{
    public class SendMessageDto
    {
        public string Content { get; set; }
    }

    public class MessageResponseDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
}
