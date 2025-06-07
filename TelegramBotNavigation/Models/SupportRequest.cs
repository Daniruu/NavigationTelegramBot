using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Models
{
    public class SupportRequest
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public SupportStatus Status { get; set; } = SupportStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public TelegramUser User { get; set; } = new TelegramUser();
    }
}
