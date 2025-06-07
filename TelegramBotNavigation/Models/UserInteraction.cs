using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Models
{
    public class UserInteraction
    {
        public int Id { get; set; }
        public long TelegramUserId { get; set; }
        public TelegramUser TelegramUser { get; set; } = null!;

        public ActionType ActionType { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public LanguageCode LanguageCode { get; set; }
        public long ChatId { get; set; }
    }
}
