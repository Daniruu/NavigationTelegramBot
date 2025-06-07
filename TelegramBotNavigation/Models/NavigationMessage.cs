using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Models
{
    public class NavigationMessage
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
        public LanguageCode LanguageCode { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
