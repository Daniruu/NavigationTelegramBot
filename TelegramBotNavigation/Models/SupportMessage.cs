namespace TelegramBotNavigation.Models
{
    public class SupportMessage
    {
        public int Id { get; set; }
        public int SupportRequestId { get; set; }
        public SupportRequest SupportRequest { get; set; } = null!;

        public bool IsFromAdmin { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
